using System.Collections;
using System.Collections.Generic;
using JokerGO.Core;
using JokerGO.Game.Board;
using JokerGO.Game.Dice;
using JokerGO.Game.Fx;
using JokerGO.Game.Tweening;
using JokerGO.Game.Utils;
using JokerGO.UI;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Bridges the rules engine and the views: animates what GameSession decides,
    /// and reports animation completion back so the session can advance.
    /// </summary>
    public sealed class GameFlowPresenter : MonoBehaviour
    {
        private const float BaseHopDuration = 0.26f;
        private const float MinHopDuration = 0.06f;
        private const int HopAccelStartSteps = 8;
        private const int HopAccelMaxSteps = 60;
        private const float DiceTrayForwardOffset = 2.2f;
        private const float DiceTraySideOffset = 3.4f;
        private const float DiceRestHeight = EnvironmentBuilder.GroundTopY + 0.275f;
        private const float DiceTileClearance = 1.35f;
        private const float DiceClusterSpacing = 0.8f;
        private const float TotalRevealSeconds = 0.9f;
        private const float CameraReturnBeat = 0.45f;
        private const float WrapVanishDuration = 0.16f;
        private const float WrapFallHeight = 7f;
        private const float WrapFallDuration = 0.5f;

        private GameSession session;
        private BoardBuilder board;
        private PlayerTokenView token;
        private DiceRollDirector dice;
        private CameraDirector cameraDirector;
        private GameHud hud;
        private Camera viewCamera;

        public void Initialize(GameSession gameSession, BoardBuilder boardBuilder,
            PlayerTokenView tokenView, DiceRollDirector diceDirector,
            CameraDirector cameraDirectorView, GameHud gameHud, Camera camera)
        {
            session = gameSession;
            board = boardBuilder;
            token = tokenView;
            dice = diceDirector;
            cameraDirector = cameraDirectorView;
            hud = gameHud;
            viewCamera = camera;

            session.RollStarted += OnRollStarted;
            session.MoveStarted += OnMoveStarted;
            session.TileLanded += OnTileLanded;
            session.ItemsCollected += OnItemsCollected;
        }

        private void OnDestroy()
        {
            if (session == null)
            {
                return;
            }

            session.RollStarted -= OnRollStarted;
            session.MoveStarted -= OnMoveStarted;
            session.TileLanded -= OnTileLanded;
            session.ItemsCollected -= OnItemsCollected;
        }

        private void OnRollStarted(IReadOnlyList<int> values)
        {
            Vector3 clusterCenter = PickDiceClusterCenter();
            Vector3[] restPositions = DiceClusterLayout.Compute(values.Count, clusterCenter,
                DiceClusterSpacing, board.TilePositions, DiceTileClearance);

            Vector3 clusterCentroid = AverageOf(restPositions);
            cameraDirector.FocusDice(clusterCentroid, MaxDistanceFrom(clusterCentroid, restPositions));
            dice.Show(values, restPositions,
                () => StartCoroutine(DiceRevealRoutine(values)), OnDieImpact);
        }

        private Vector3 PickDiceClusterCenter()
        {
            Vector3 tokenPosition = board.TokenPositionOn(session.CurrentTileIndex);
            Vector3 direction = board.PathDirectionAt(session.CurrentTileIndex);
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;

            Vector3 left = tokenPosition + direction * DiceTrayForwardOffset - side * DiceTraySideOffset;
            Vector3 right = tokenPosition + direction * DiceTrayForwardOffset + side * DiceTraySideOffset;

            Vector3 center = MinTileDistance(left) >= MinTileDistance(right) ? left : right;
            center.y = DiceRestHeight;
            return center;
        }

        private float MinTileDistance(Vector3 point)
        {
            float best = float.MaxValue;
            IReadOnlyList<Vector3> tiles = board.TilePositions;
            for (int i = 0; i < tiles.Count; i++)
            {
                float dx = point.x - tiles[i].x;
                float dz = point.z - tiles[i].z;
                best = Mathf.Min(best, dx * dx + dz * dz);
            }

            return best;
        }

        private static Vector3 AverageOf(IReadOnlyList<Vector3> points)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                sum += points[i];
            }

            return sum / points.Count;
        }

        private static float MaxDistanceFrom(Vector3 center, IReadOnlyList<Vector3> points)
        {
            float max = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                max = Mathf.Max(max, Vector3.Distance(center, points[i]));
            }

            return max;
        }

        /// <summary>
        /// Dice have settled under the close-up camera: pop the animated total,
        /// hand the view back to the player, then let the move begin.
        /// </summary>
        private IEnumerator DiceRevealRoutine(IReadOnlyList<int> values)
        {
            if (hud != null)
            {
                hud.ShowDiceTotal(DiceRules.Sum(values));
            }

            yield return WaitHelper.WaitForSeconds(TotalRevealSeconds);
            cameraDirector.ResumeFromDice();
            yield return WaitHelper.WaitForSeconds(CameraReturnBeat);
            session.NotifyDiceShown();
        }

        private void OnDieImpact(Vector3 position)
        {
            PoolManager.Instance.PlayDust(position, 0.7f);
            if (cameraDirector != null)
            {
                cameraDirector.Shake(0.07f);
            }
        }

        private void OnMoveStarted(IReadOnlyList<int> path)
        {
            dice.Dismiss();
            StartCoroutine(MoveRoutine(path));
        }

        private void OnTileLanded(MapTile tile)
        {
            string reward = tile.HasReward
                ? $" — reward: {tile.Reward.Value}"
                : " — empty";
            Debug.Log($"[JokerGO] Landed on tile {tile.DisplayNumber}{reward}");
        }

        private void OnItemsCollected(Inventory inventory, ItemStack gained)
        {
            Vector3 collectPoint = token.transform.position + Vector3.up * 0.4f;
            PoolManager.Instance.PlayBurst(collectPoint, UiTheme.ItemColor(gained.Type));
            if (cameraDirector != null)
            {
                cameraDirector.Shake(0.05f);
            }

            if (hud != null && viewCamera != null)
            {
                hud.ShowCollectFlight(viewCamera.WorldToScreenPoint(collectPoint), gained);
            }

            Debug.Log($"[JokerGO] Collected {gained}. Totals — " +
                      $"Apples: {inventory.Get(ItemType.Apple)}, " +
                      $"Pears: {inventory.Get(ItemType.Pear)}, " +
                      $"Strawberries: {inventory.Get(ItemType.Strawberry)}");
        }

        private IEnumerator MoveRoutine(IReadOnlyList<int> path)
        {
            float hopDuration = HopDurationFor(path.Count);
            bool squashEveryHop = hopDuration > 0.18f;
            int previousIndex = session.CurrentTileIndex;

            for (int i = 0; i < path.Count; i++)
            {
                int tileIndex = path[i];
                bool isLast = i == path.Count - 1;

                if (tileIndex < previousIndex)
                {
                    yield return WrapEntryRoutine(tileIndex);
                }
                else
                {
                    yield return token.HopTo(
                        board.TokenPositionOn(tileIndex), hopDuration, squashEveryHop || isLast);
                }

                TileView landedView = board.TileViews[tileIndex];
                landedView.PressBounce();
                PoolManager.Instance.PlayDust(landedView.TokenAnchor, isLast ? 1.1f : 0.6f);
                previousIndex = tileIndex;
            }

            session.NotifyMoveCompleted();
        }

        /// <summary>
        /// Wrap-around entrance: the token pops away, the camera glides back to the
        /// start of the path, then the token drops in from the sky and play continues.
        /// </summary>
        private IEnumerator WrapEntryRoutine(int tileIndex)
        {
            yield return Tween.ScaleTo(token.transform, Vector3.zero, WrapVanishDuration,
                EaseType.EaseInQuad);

            Vector3 anchor = board.TokenPositionOn(tileIndex);
            cameraDirector.FocusPoint(anchor);
            yield return WaitHelper.WaitForSeconds(cameraDirector.HomeBlendDuration);

            yield return token.FallFrom(anchor, WrapFallHeight, WrapFallDuration);
            cameraDirector.Shake(0.12f);
            cameraDirector.ResumeFollow();
        }

        /// <summary>Long paths (many dice) hop faster so movement never drags.</summary>
        private static float HopDurationFor(int steps)
        {
            float t = Mathf.InverseLerp(HopAccelStartSteps, HopAccelMaxSteps, steps);
            return Mathf.Lerp(BaseHopDuration, MinHopDuration, t);
        }
    }
}

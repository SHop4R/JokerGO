using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JokerGO.Core.Project.Scripts.Core;
using JokerGO.Game.Project.Scripts.Game.Board;
using JokerGO.Game.Project.Scripts.Game.Dice;
using JokerGO.Game.Project.Scripts.Game.Fx;
using JokerGO.Game.Project.Scripts.Game.Tweening;
using JokerGO.Game.Project.Scripts.Game.Utils;
using JokerGO.UI.Project.Scripts.UI;
using UnityEngine;

namespace JokerGO.Game.Project.Scripts.Game
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

        private GameSession _session;
        private BoardBuilder _board;
        private PlayerTokenView _token;
        private DiceRollDirector _dice;
        private CameraDirector _cameraDirector;
        private GameHud _hud;
        private Camera _viewCamera;

        public void Initialize(GameSession gameSession, BoardBuilder boardBuilder,
            PlayerTokenView tokenView, DiceRollDirector diceDirector,
            CameraDirector cameraDirectorView, GameHud gameHud, Camera cam)
        {
            _session = gameSession;
            _board = boardBuilder;
            _token = tokenView;
            _dice = diceDirector;
            _cameraDirector = cameraDirectorView;
            _hud = gameHud;
            _viewCamera = cam;

            _session.RollStarted += OnRollStarted;
            _session.MoveStarted += OnMoveStarted;
            _session.TileLanded += OnTileLanded;
            _session.ItemsCollected += OnItemsCollected;
        }

        private void OnDestroy()
        {
            if (_session == null)
                return;

            _session.RollStarted -= OnRollStarted;
            _session.MoveStarted -= OnMoveStarted;
            _session.TileLanded -= OnTileLanded;
            _session.ItemsCollected -= OnItemsCollected;
        }

        private void OnRollStarted(IReadOnlyList<int> values)
        {
            Vector3 clusterCenter = PickDiceClusterCenter();
            Vector3[] restPositions = DiceClusterLayout.Compute(values.Count, clusterCenter,
                DiceClusterSpacing, _board.TilePositions, DiceTileClearance);

            Vector3 clusterCentroid = AverageOf(restPositions);
            _cameraDirector.FocusDice(clusterCentroid, MaxDistanceFrom(clusterCentroid, restPositions));
            _dice.Show(values, restPositions,
                () => StartCoroutine(DiceRevealRoutine(values)), OnDieImpact);
        }

        private Vector3 PickDiceClusterCenter()
        {
            Vector3 tokenPosition = _board.TokenPositionOn(_session.CurrentTileIndex);
            Vector3 direction = _board.PathDirectionAt(_session.CurrentTileIndex);
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
            IReadOnlyList<Vector3> tiles = _board.TilePositions;
            
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
            Vector3 sum = points.Aggregate(Vector3.zero, (current, t) => current + t);
            return sum / points.Count;
        }

        private static float MaxDistanceFrom(Vector3 center, IReadOnlyList<Vector3> points) 
            => points.Aggregate(0f, (current, t) 
                => Mathf.Max(current, Vector3.Distance(center, t)));

        /// <summary>
        /// Dice have settled under the close-up camera: pop the animated total,
        /// hand the view back to the player, then let the move begin.
        /// </summary>
        private IEnumerator DiceRevealRoutine(IReadOnlyList<int> values)
        {
            if (_hud) 
                _hud.ShowDiceTotal(values.Sum());

            yield return WaitHelper.WaitForSeconds(TotalRevealSeconds);
            _cameraDirector.ResumeFromDice();
            yield return WaitHelper.WaitForSeconds(CameraReturnBeat);
            _session.NotifyDiceShown();
        }

        private void OnDieImpact(Vector3 position)
        {
            PoolManager.Instance.PlayDust(position, 0.7f);
            
            if (_cameraDirector) 
                _cameraDirector.Shake(0.07f);
        }

        private void OnMoveStarted(IReadOnlyList<int> path)
        {
            _dice.Dismiss();
            StartCoroutine(MoveRoutine(path));
        }

        private static void OnTileLanded(MapTile tile)
        {
            if (tile.Reward == null) return;

            string reward = tile.HasReward
                ? $" — reward: {tile.Reward.Value}"
                : " — empty";
            
            Debug.Log($"[JokerGO] Landed on tile {tile.DisplayNumber}{reward}");
        }

        private void OnItemsCollected(Inventory inventory, ItemStack gained)
        {
            Vector3 collectPoint = _token.transform.position + Vector3.up * 0.4f;
            PoolManager.Instance.PlayBurst(collectPoint, UiTheme.ItemColor(gained.Type));
            
            if (_cameraDirector) 
                _cameraDirector.Shake(0.05f);

            if (_hud && _viewCamera) 
                _hud.ShowCollectFlight(_viewCamera.WorldToScreenPoint(collectPoint), gained);

            Debug.Log($"[JokerGO] Collected {gained}. Totals — " +
                      $"Apples: {inventory.Get(ItemType.Apple)}, " +
                      $"Pears: {inventory.Get(ItemType.Pear)}, " +
                      $"Strawberries: {inventory.Get(ItemType.Strawberry)}");
        }

        private IEnumerator MoveRoutine(IReadOnlyList<int> path)
        {
            float hopDuration = HopDurationFor(path.Count);
            bool squashEveryHop = hopDuration > 0.18f;
            int previousIndex = _session.CurrentTileIndex;

            for (int i = 0; i < path.Count; i++)
            {
                int tileIndex = path[i];
                bool isLast = i == path.Count - 1;

                if (tileIndex < previousIndex)
                    yield return WrapEntryRoutine(tileIndex);
                else
                    yield return _token.HopTo(_board.TokenPositionOn(tileIndex), hopDuration, squashEveryHop || isLast);

                TileView landedView = _board.TileViews[tileIndex];
                landedView.PressBounce();
                PoolManager.Instance.PlayDust(landedView.TokenAnchor, isLast ? 1.1f : 0.6f);
                previousIndex = tileIndex;
            }

            _session.NotifyMoveCompleted();
        }

        /// <summary>
        /// Wrap-around entrance: the token pops away, the camera glides back to the
        /// start of the path, then the token drops in from the sky and play continues.
        /// </summary>
        private IEnumerator WrapEntryRoutine(int tileIndex)
        {
            yield return Tween.ScaleTo(_token.transform, Vector3.zero, WrapVanishDuration, EaseType.EaseInQuad);

            Vector3 anchor = _board.TokenPositionOn(tileIndex);
            _cameraDirector.FocusPoint(anchor);
            yield return WaitHelper.WaitForSeconds(_cameraDirector.HomeBlendDuration);

            yield return _token.FallFrom(anchor, WrapFallHeight, WrapFallDuration);
            _cameraDirector.Shake(0.12f);
            _cameraDirector.ResumeFollow();
        }

        /// <summary>Long paths (many dice) hop faster so movement never drags.</summary>
        private static float HopDurationFor(int steps)
        {
            float t = Mathf.InverseLerp(HopAccelStartSteps, HopAccelMaxSteps, steps);
            return Mathf.Lerp(BaseHopDuration, MinHopDuration, t);
        }
    }
}

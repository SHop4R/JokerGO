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
        private const float DiceTrayForwardOffset = 3.5f;
        private const float MaxTrayLateralOffset = 1.2f;
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
            // The tray sits ahead along the path's travel direction; dice are gone
            // before the token hops through. Lateral drift is clamped so a wide
            // dice row cannot slide off the portrait frame on steep bends.
            Vector3 trayOffset = board.PathDirectionAt(session.CurrentTileIndex) * DiceTrayForwardOffset;
            trayOffset.x = Mathf.Clamp(trayOffset.x, -MaxTrayLateralOffset, MaxTrayLateralOffset);
            Vector3 trayCenter = board.TokenPositionOn(session.CurrentTileIndex) + trayOffset;

            // The dice camera glides down onto the tray while the dice tumble in.
            cameraDirector.FocusDice(trayCenter);
            dice.Show(values, trayCenter,
                () => StartCoroutine(DiceRevealRoutine(values)), OnDieImpact);
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
                    // Wrap-around: the index dropped, so the token passed the end of the line.
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

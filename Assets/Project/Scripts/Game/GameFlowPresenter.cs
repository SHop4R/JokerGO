using System.Collections;
using System.Collections.Generic;
using JokerGO.Core;
using JokerGO.Game.Board;
using JokerGO.Game.Dice;
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

        private GameSession session;
        private BoardBuilder board;
        private PlayerTokenView token;
        private DiceRollDirector dice;

        public void Initialize(GameSession gameSession, BoardBuilder boardBuilder,
            PlayerTokenView tokenView, DiceRollDirector diceDirector)
        {
            session = gameSession;
            board = boardBuilder;
            token = tokenView;
            dice = diceDirector;

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
            // The tray sits on the path ahead of the token; dice are gone before it hops through.
            Vector3 trayCenter = board.TokenPositionOn(session.CurrentTileIndex)
                                 + Vector3.forward * DiceTrayForwardOffset;
            dice.Show(values, trayCenter, session.NotifyDiceShown);
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
            Debug.Log($"[JokerGO] Collected {gained}. Totals — " +
                      $"Apples: {inventory.Get(ItemType.Apple)}, " +
                      $"Pears: {inventory.Get(ItemType.Pear)}, " +
                      $"Strawberries: {inventory.Get(ItemType.Strawberry)}");
        }

        private IEnumerator MoveRoutine(IReadOnlyList<int> path)
        {
            float hopDuration = HopDurationFor(path.Count);
            bool squashEveryHop = hopDuration > 0.18f;

            for (int i = 0; i < path.Count; i++)
            {
                bool isLast = i == path.Count - 1;
                yield return token.HopTo(
                    board.TokenPositionOn(path[i]), hopDuration, squashEveryHop || isLast);
            }

            session.NotifyMoveCompleted();
        }

        /// <summary>Long paths (many dice) hop faster so movement never drags.</summary>
        private static float HopDurationFor(int steps)
        {
            float t = Mathf.InverseLerp(HopAccelStartSteps, HopAccelMaxSteps, steps);
            return Mathf.Lerp(BaseHopDuration, MinHopDuration, t);
        }
    }
}

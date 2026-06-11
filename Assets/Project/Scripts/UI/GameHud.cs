using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.UI
{
    /// <summary>
    /// Owns the HUD views and connects them to the rules engine: input intents go in
    /// via TryRoll, session events come back out as panel updates.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        private GameSession session;
        private DicePanelView dicePanel;
        private InventoryPanelView inventoryPanel;
        private TileLogView tileLog;
        private CollectFlightLayer flightLayer;
        private DiceTotalView diceTotal;
        private RewardPopupView rewardPopup;
        private Inventory pendingInventory;

        public static GameHud Create(GameSession session)
        {
            Canvas canvas = UiFactory.CreateRootCanvas();
            var hud = canvas.gameObject.AddComponent<GameHud>();

            hud.session = session;
            hud.dicePanel = DicePanelView.Build(canvas.transform);
            hud.inventoryPanel = InventoryPanelView.Build(canvas.transform);
            hud.tileLog = TileLogView.Build(canvas.transform);
            hud.flightLayer = CollectFlightLayer.Build(canvas.transform);
            hud.diceTotal = DiceTotalView.Build(canvas.transform);
            hud.rewardPopup = RewardPopupView.Build(canvas.transform);

            hud.inventoryPanel.Refresh(session.Inventory);
            hud.dicePanel.RollRequested += hud.OnRollRequested;
            session.RollStarted += hud.OnRollStarted;
            session.TileLanded += hud.OnTileLanded;
            session.ItemsCollected += hud.OnItemsCollected;
            session.TurnEnded += hud.OnTurnEnded;
            return hud;
        }

        private void OnDestroy()
        {
            if (session == null)
            {
                return;
            }

            dicePanel.RollRequested -= OnRollRequested;
            session.RollStarted -= OnRollStarted;
            session.TileLanded -= OnTileLanded;
            session.ItemsCollected -= OnItemsCollected;
            session.TurnEnded -= OnTurnEnded;
        }

        private void OnRollRequested(IReadOnlyList<int> values)
        {
            RollValidation result = session.TryRoll(values);
            if (!result.IsValid)
            {
                dicePanel.ShowError(result.Error);
            }
        }

        private void OnRollStarted(IReadOnlyList<int> values)
        {
            dicePanel.SetInteractable(false);
        }

        private void OnTileLanded(MapTile tile)
        {
            // Plain hyphen: the project font has no em dash glyph.
            tileLog.Show(tile.HasReward
                ? $"Landed on tile {tile.DisplayNumber} - collected {tile.Reward.Value}!"
                : $"Landed on tile {tile.DisplayNumber} - empty.");
        }

        private void OnItemsCollected(Inventory inventory, ItemStack gained)
        {
            // Refresh is deferred until the collect flight lands (see ShowCollectFlight);
            // OnTurnEnded acts as the safety net if no flight was played.
            pendingInventory = inventory;
        }

        /// <summary>Pops the dice total over the dice close-up.</summary>
        public void ShowDiceTotal(int total)
        {
            diceTotal.Show(total);
        }

        /// <summary>Plays the collect feedback: "+N Item" popup plus chips flying to the counter.</summary>
        public void ShowCollectFlight(Vector2 screenPosition, ItemStack gained)
        {
            rewardPopup.Show(screenPosition, $"+{gained.Amount} {gained.Type}",
                UiTheme.ItemColor(gained.Type));

            int chips = Mathf.Clamp(gained.Amount, 3, 7);
            flightLayer.Fly(screenPosition, gained.Type, inventoryPanel.CounterTarget(gained.Type),
                chips, () =>
                {
                    if (pendingInventory != null)
                    {
                        inventoryPanel.Refresh(pendingInventory);
                    }

                    inventoryPanel.Punch(gained.Type);
                });
        }

        private void OnTurnEnded()
        {
            dicePanel.SetInteractable(true);
            inventoryPanel.Refresh(session.Inventory);
        }
    }
}

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

        public static GameHud Create(GameSession session)
        {
            Canvas canvas = UiFactory.CreateRootCanvas();
            var hud = canvas.gameObject.AddComponent<GameHud>();

            hud.session = session;
            hud.dicePanel = DicePanelView.Build(canvas.transform);
            hud.inventoryPanel = InventoryPanelView.Build(canvas.transform);
            hud.tileLog = TileLogView.Build(canvas.transform);

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
            inventoryPanel.Refresh(inventory);
        }

        private void OnTurnEnded()
        {
            dicePanel.SetInteractable(true);
        }
    }
}

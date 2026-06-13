using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.UI
{
    /// <summary>
    /// Owns the HUD views and connects them to the rules engine: input intents go in
    /// via TryRoll, session events come back out as panel updates. The whole canvas
    /// lives pre-built in the scene; Initialize only wires it to the session.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private DicePanelView dicePanel;
        [SerializeField] private InventoryPanelView inventoryPanel;
        [SerializeField] private TileLogView tileLog;
        [SerializeField] private CollectFlightLayer flightLayer;
        [SerializeField] private DiceTotalView diceTotal;
        [SerializeField] private RewardPopupView rewardPopup;

        private GameSession session;
        private Inventory pendingInventory;

        /// <summary>Connects the pre-authored HUD to a freshly created session.</summary>
        public void Initialize(GameSession gameSession)
        {
            session = gameSession;
            inventoryPanel.Refresh(session.Inventory);

            dicePanel.RollRequested += OnRollRequested;
            session.RollStarted += OnRollStarted;
            session.TileLanded += OnTileLanded;
            session.ItemsCollected += OnItemsCollected;
            session.TurnEnded += OnTurnEnded;
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
            tileLog.Show(tile.HasReward
                ? $"Landed on tile {tile.DisplayNumber} - collected {tile.Reward.Value}!"
                : $"Landed on tile {tile.DisplayNumber} - empty.");
        }

        private void OnItemsCollected(Inventory inventory, ItemStack gained)
        {
            pendingInventory = inventory;
        }

        private void OnTurnEnded()
        {
            dicePanel.SetInteractable(true);
            inventoryPanel.Refresh(session.Inventory);
        }

        /// <summary>Editor-time construction of the full HUD canvas; saved as the HUD prefab.</summary>
        public static GameHud Author()
        {
            Canvas canvas = UiFactory.CreateRootCanvas();
            var hud = canvas.gameObject.AddComponent<GameHud>();

            hud.dicePanel = DicePanelView.Author(canvas.transform);
            hud.inventoryPanel = InventoryPanelView.Author(canvas.transform);
            hud.tileLog = TileLogView.Author(canvas.transform);
            hud.flightLayer = CollectFlightLayer.Author(canvas.transform);
            hud.diceTotal = DiceTotalView.Author(canvas.transform);
            hud.rewardPopup = RewardPopupView.Author(canvas.transform);
            return hud;
        }
    }
}

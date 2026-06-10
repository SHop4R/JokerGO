using System;

namespace JokerGO.Core
{
    /// <summary>Maps between the domain state and its serializable snapshot.</summary>
    public static class SaveDataMapper
    {
        public static SaveData ToSaveData(Inventory inventory, int currentTileIndex)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            return new SaveData
            {
                apples = inventory.Get(ItemType.Apple),
                pears = inventory.Get(ItemType.Pear),
                strawberries = inventory.Get(ItemType.Strawberry),
                currentTileIndex = currentTileIndex
            };
        }

        /// <summary>
        /// Restores state from a snapshot. A null snapshot yields a fresh start; a tile index
        /// that no longer fits the map (the map may have changed) is clamped to the first tile.
        /// Negative counts mean a corrupt file and throw, so callers can fall back cleanly.
        /// </summary>
        public static (Inventory Inventory, int CurrentTileIndex) FromSaveData(SaveData data, int tileCount)
        {
            if (data == null)
            {
                return (Inventory.Empty, 0);
            }

            Inventory inventory = Inventory.FromCounts(data.apples, data.pears, data.strawberries);
            int tileIndex = data.currentTileIndex >= 0 && data.currentTileIndex < tileCount
                ? data.currentTileIndex
                : 0;
            return (inventory, tileIndex);
        }
    }
}

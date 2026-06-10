namespace JokerGO.Core
{
    /// <summary>
    /// A single board tile. <see cref="Index"/> is zero-based;
    /// <see cref="DisplayNumber"/> is the one-based number players see.
    /// </summary>
    public readonly struct MapTile
    {
        public int Index { get; }
        public ItemStack? Reward { get; }

        public bool HasReward => Reward.HasValue;
        public int DisplayNumber => Index + 1;

        public MapTile(int index, ItemStack? reward)
        {
            Index = index;
            Reward = reward;
        }
    }
}

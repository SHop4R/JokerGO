using System;

namespace JokerGO.Core
{
    /// <summary>An immutable quantity of a single item type.</summary>
    public readonly struct ItemStack : IEquatable<ItemStack>
    {
        public ItemType Type { get; }
        public int Amount { get; }

        public ItemStack(ItemType type, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Item amount must be positive.");
            }

            Type = type;
            Amount = amount;
        }

        public bool Equals(ItemStack other) => Type == other.Type && Amount == other.Amount;
        public override bool Equals(object obj) => obj is ItemStack other && Equals(other);
        public override int GetHashCode() => ((int)Type * 397) ^ Amount;
        public override string ToString() => $"{Amount} {Type}";
    }
}

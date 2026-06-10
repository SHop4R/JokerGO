using System;
using System.Collections.Generic;

namespace JokerGO.Core
{
    /// <summary>Immutable item counts. <see cref="Add"/> returns a new instance.</summary>
    public sealed class Inventory
    {
        public static readonly Inventory Empty = new Inventory(new Dictionary<ItemType, int>());

        private readonly Dictionary<ItemType, int> counts;

        private Inventory(Dictionary<ItemType, int> counts)
        {
            this.counts = counts;
        }

        public int Get(ItemType type) => counts.TryGetValue(type, out int amount) ? amount : 0;

        public Inventory Add(ItemStack stack)
        {
            var next = new Dictionary<ItemType, int>(counts)
            {
                [stack.Type] = Get(stack.Type) + stack.Amount
            };
            return new Inventory(next);
        }

        public static Inventory FromCounts(int apples, int pears, int strawberries)
        {
            RequireNonNegative(apples, nameof(apples));
            RequireNonNegative(pears, nameof(pears));
            RequireNonNegative(strawberries, nameof(strawberries));

            return new Inventory(new Dictionary<ItemType, int>
            {
                [ItemType.Apple] = apples,
                [ItemType.Pear] = pears,
                [ItemType.Strawberry] = strawberries
            });
        }

        private static void RequireNonNegative(int count, string parameterName)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, count,
                    "Inventory counts cannot be negative.");
            }
        }
    }
}

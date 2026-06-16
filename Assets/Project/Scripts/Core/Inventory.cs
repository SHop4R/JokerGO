using System;
using System.Collections.Generic;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Immutable item counts. <see cref="Add"/> returns a new instance.</summary>
    public sealed class Inventory
    {
        public static readonly Inventory Empty = new(new());

        private readonly Dictionary<ItemType, int> _counts;

        private Inventory(Dictionary<ItemType, int> counts) => _counts = counts;

        public int Get(ItemType type) => _counts.GetValueOrDefault(type, 0);

        public Inventory Add(ItemStack stack)
        {
            Dictionary<ItemType, int> next = new(_counts)
            {
                [stack.Type] = Get(stack.Type) + stack.Amount
            };
            return new(next);
        }

        public static Inventory FromCounts(int apples, int pears, int strawberries)
        {
            RequireNonNegative(apples, nameof(apples));
            RequireNonNegative(pears, nameof(pears));
            RequireNonNegative(strawberries, nameof(strawberries));

            return new Inventory(new()
            {
                [ItemType.Apple] = apples,
                [ItemType.Pear] = pears,
                [ItemType.Strawberry] = strawberries
            });
        }

        private static void RequireNonNegative(int count, string parameterName)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(parameterName, count, "Inventory counts cannot be negative.");
        }
    }
}

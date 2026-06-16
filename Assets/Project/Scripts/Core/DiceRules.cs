using System.Collections.Generic;
using System.Linq;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Dice constraints from the case study: 1-20 dice, each valued 1-6.</summary>
    public static class DiceRules
    {
        public const int MinValue = 1;
        public const int MaxValue = 6;
        public const int MinDiceCount = 1;
        public const int MaxDiceCount = 20;

        public static RollValidation Validate(IReadOnlyList<int> values)
        {
            if (values == null)
                throw new System.ArgumentNullException(nameof(values));

            switch (values.Count)
            {
                case < MinDiceCount:
                    return RollValidation.Fail("Enter a value for at least one die.");

                case > MaxDiceCount:
                    return RollValidation.Fail($"At most {MaxDiceCount} dice are supported.");
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < MinValue || values[i] > MaxValue)
                    return RollValidation.Fail($"Die {i + 1} must be between {MinValue} and {MaxValue}.");
            }

            return RollValidation.Success;
        }
    }
}

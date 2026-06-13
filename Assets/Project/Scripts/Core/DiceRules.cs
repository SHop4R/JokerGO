using System.Collections.Generic;

namespace JokerGO.Core
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
            {
                throw new System.ArgumentNullException(nameof(values));
            }

            if (values.Count < MinDiceCount)
            {
                return RollValidation.Fail("Enter a value for at least one die.");
            }

            if (values.Count > MaxDiceCount)
            {
                return RollValidation.Fail($"At most {MaxDiceCount} dice are supported.");
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < MinValue || values[i] > MaxValue)
                {
                    return RollValidation.Fail(
                        $"Die {i + 1} must be between {MinValue} and {MaxValue}.");
                }
            }

            return RollValidation.Success;
        }

        public static int Sum(IReadOnlyList<int> values)
        {
            int sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }
    }
}

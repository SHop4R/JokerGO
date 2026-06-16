using System;
using System.Collections.Generic;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>
    /// Pure path math: which tiles a token visits for a roll, wrapping past the
    /// last tile back to the first.
    /// </summary>
    public static class MovementCalculator
    {
        public static IReadOnlyList<int> ComputePath(int currentIndex, int steps, int tileCount)
        {
            if (tileCount < BoardMap.MinimumTileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(tileCount), tileCount,
                    $"Tile count must be at least {BoardMap.MinimumTileCount}.");
            }

            if (currentIndex < 0 || currentIndex >= tileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(currentIndex), currentIndex,
                    $"Current index must be within [0, {tileCount - 1}].");
            }

            if (steps <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(steps), steps, "Steps must be positive.");
            }

            int[] path = new int[steps];
            for (int i = 1; i <= steps; i++)
            {
                path[i - 1] = (currentIndex + i) % tileCount;
            }

            return path;
        }
    }
}

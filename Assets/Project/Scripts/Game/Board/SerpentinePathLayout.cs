using UnityEngine;

namespace JokerGO.Game.Project.Scripts.Game.Board
{
    /// <summary>
    /// Lays tiles along a winding serpentine (x = A·sin(2πz/λ)) with equal straight-line
    /// (chord) spacing between consecutive tiles, so the board reads as a path rather than
    /// a ruler — still topologically a line per the case rules, never a closed square.
    /// </summary>
    public static class SerpentinePathLayout
    {
        private const float StepSize = 0.01f;

        public static Vector3[] Compute(int tileCount, float spacing, float amplitude, float wavelength)
        {
            if (tileCount < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(tileCount), tileCount,
                    "At least one tile is required.");
            }

            if (spacing <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(spacing), spacing,
                    "Tile spacing must be positive.");
            }

            if (wavelength <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(wavelength), wavelength,
                    "Path wavelength must be positive.");
            }

            var positions = new Vector3[tileCount];
            positions[0] = PointAt(0f, amplitude, wavelength);

            float z = 0f;
            for (int i = 1; i < tileCount; i++)
            {
                Vector3 previous = positions[i - 1];

                Vector3 candidate = previous;
                while ((candidate - previous).sqrMagnitude < spacing * spacing)
                {
                    float advanced = z + StepSize;
                    if (advanced == z)
                    {
                        throw new System.InvalidOperationException(
                            $"Path layout step underflow at tile {i}; the map is too long to lay out.");
                    }

                    z = advanced;
                    candidate = PointAt(z, amplitude, wavelength);
                }

                positions[i] = candidate;
            }

            return positions;
        }

        private static Vector3 PointAt(float z, float amplitude, float wavelength)
        {
            float x = amplitude * Mathf.Sin(2f * Mathf.PI * z / wavelength);
            return new Vector3(x, 0f, z);
        }
    }
}

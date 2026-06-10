using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>
    /// Lays tiles along a winding serpentine (x = A·sin(2πz/λ)) with equal spacing
    /// measured along the curve, so the board reads as a path rather than a ruler —
    /// still topologically a line per the case rules, never a closed square.
    /// </summary>
    public static class SerpentinePathLayout
    {
        private const float StepSize = 0.01f;

        public static Vector3[] Compute(int tileCount, float spacing, float amplitude, float wavelength)
        {
            var positions = new Vector3[tileCount];
            positions[0] = PointAt(0f, amplitude, wavelength);

            float z = 0f;
            for (int i = 1; i < tileCount; i++)
            {
                Vector3 previous = positions[i - 1];

                // March forward until the next point is one tile-spacing away (chord distance).
                Vector3 candidate = previous;
                while ((candidate - previous).sqrMagnitude < spacing * spacing)
                {
                    z += StepSize;
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

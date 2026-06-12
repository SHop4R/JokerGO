using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game.Dice
{
    /// <summary>
    /// Organic landing spots for N dice: a jittered golden-angle cluster around a
    /// center point, with every spot pushed off the board tiles so dice always
    /// rest on the grass, never on the path.
    /// </summary>
    public static class DiceClusterLayout
    {
        private const float GoldenAngleDegrees = 137.508f;
        private const float JitterRadius = 0.14f;
        private const int RelocateAttempts = 10;
        // Keeps relocated dice near the group, so the close-up camera always frames them.
        private const float MaxClusterRadius = 2.6f;

        public static Vector3[] Compute(int count, Vector3 center, float spacing,
            IReadOnlyList<Vector3> tilePositions, float tileClearance)
        {
            var positions = new Vector3[count];
            float phase = Random.Range(0f, 360f);

            for (int i = 0; i < count; i++)
            {
                Vector3 candidate = SpiralPoint(center, i, spacing, phase);

                for (int attempt = 0; attempt < RelocateAttempts && TooCloseToTiles(candidate, tilePositions, tileClearance); attempt++)
                {
                    // First rotate in place (same ring, new direction); only then slide
                    // outward - and the ring radius is capped, so dice never fly off
                    // beyond what the close-up camera can frame.
                    candidate = attempt < RelocateAttempts / 2
                        ? SpiralPoint(center, i, spacing, phase + (attempt + 1) * 73f)
                        : SpiralPoint(center, i + count + attempt, spacing, phase);
                }

                positions[i] = candidate;
            }

            return positions;
        }

        private static Vector3 SpiralPoint(Vector3 center, int index, float spacing, float phaseDegrees)
        {
            float radius = Mathf.Min(spacing * Mathf.Sqrt(index + 0.5f), MaxClusterRadius);
            float angle = (phaseDegrees + index * GoldenAngleDegrees) * Mathf.Deg2Rad;

            Vector2 jitter = Random.insideUnitCircle * JitterRadius;
            return new Vector3(
                center.x + Mathf.Cos(angle) * radius + jitter.x,
                center.y,
                center.z + Mathf.Sin(angle) * radius + jitter.y);
        }

        private static bool TooCloseToTiles(Vector3 candidate, IReadOnlyList<Vector3> tilePositions,
            float clearance)
        {
            float clearanceSquared = clearance * clearance;
            for (int i = 0; i < tilePositions.Count; i++)
            {
                Vector3 tile = tilePositions[i];
                float dx = candidate.x - tile.x;
                float dz = candidate.z - tile.z;
                if (dx * dx + dz * dz < clearanceSquared)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

using UnityEngine;

namespace JokerGO.Game.Dice
{
    /// <summary>
    /// Standard die face layout (opposite faces sum to 7) and the rotation math
    /// that makes a requested value end face-up.
    /// </summary>
    public static class DieFaceLayout
    {
        public static Vector3 NormalOf(int value)
        {
            switch (value)
            {
                case 1: return Vector3.up;
                case 2: return Vector3.forward;
                case 3: return Vector3.right;
                case 4: return Vector3.left;
                case 5: return Vector3.back;
                case 6: return Vector3.down;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(value), value,
                        "Die faces are 1-6.");
            }
        }

        /// <summary>World rotation that points the given face up, with a yaw twist for variety.</summary>
        public static Quaternion RotationShowing(int value, float yawDegrees)
        {
            return Quaternion.AngleAxis(yawDegrees, Vector3.up)
                   * Quaternion.FromToRotation(NormalOf(value), Vector3.up);
        }
    }
}

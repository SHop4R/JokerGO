using UnityEngine;

namespace JokerGO.Game.Tweening
{
    public enum EaseType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseOutBack
    }

    /// <summary>Pure easing math for the in-house tween helper (no third-party tween libs allowed).</summary>
    public static class Easing
    {
        public static float Evaluate(EaseType type, float t)
        {
            t = Mathf.Clamp01(t);
            switch (type)
            {
                case EaseType.EaseInQuad:
                    return t * t;
                case EaseType.EaseOutQuad:
                    return 1f - (1f - t) * (1f - t);
                case EaseType.EaseOutBack:
                {
                    const float overshoot = 1.70158f;
                    const float amplified = overshoot + 1f;
                    float u = t - 1f;
                    return 1f + amplified * u * u * u + overshoot * u * u;
                }
                default:
                    return t;
            }
        }
    }
}

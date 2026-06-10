using System;
using System.Collections;
using UnityEngine;

namespace JokerGO.Game.Tweening
{
    /// <summary>
    /// Composable coroutine tweens. Routines are plain IEnumerators so callers can
    /// chain them with yield return inside their own coroutines.
    /// </summary>
    public static class Tween
    {
        public static IEnumerator MoveTo(Transform target, Vector3 to, float duration, EaseType ease)
        {
            Vector3 from = target.position;
            return Progress(duration, t => target.position = Vector3.LerpUnclamped(from, to, Easing.Evaluate(ease, t)));
        }

        /// <summary>Parabolic jump: linear travel plus a vertical arc of the given height.</summary>
        public static IEnumerator JumpTo(Transform target, Vector3 to, float arcHeight, float duration)
        {
            Vector3 from = target.position;
            return Progress(duration, t =>
            {
                Vector3 flat = Vector3.LerpUnclamped(from, to, t);
                float arc = arcHeight * 4f * t * (1f - t);
                target.position = flat + Vector3.up * arc;
            });
        }

        public static IEnumerator ScaleTo(Transform target, Vector3 to, float duration, EaseType ease)
        {
            Vector3 from = target.localScale;
            return Progress(duration, t => target.localScale = Vector3.LerpUnclamped(from, to, Easing.Evaluate(ease, t)));
        }

        public static IEnumerator RotateTo(Transform target, Quaternion to, float duration, EaseType ease)
        {
            Quaternion from = target.rotation;
            return Progress(duration, t => target.rotation = Quaternion.SlerpUnclamped(from, to, Easing.Evaluate(ease, t)));
        }

        /// <summary>Vertical squash that recovers to the original scale; sells landings.</summary>
        public static IEnumerator Squash(Transform target, float squashFactor, float duration)
        {
            Vector3 original = target.localScale;
            Vector3 squashed = new Vector3(
                original.x / squashFactor, original.y * squashFactor, original.z / squashFactor);

            return Progress(duration, t =>
            {
                // Down fast in the first 35%, recover with a slight spring after.
                float blend = t < 0.35f
                    ? Easing.Evaluate(EaseType.EaseOutQuad, t / 0.35f)
                    : 1f - Easing.Evaluate(EaseType.EaseOutBack, (t - 0.35f) / 0.65f);
                target.localScale = Vector3.LerpUnclamped(original, squashed, blend);
            });
        }

        private static IEnumerator Progress(float duration, Action<float> apply)
        {
            if (apply == null)
            {
                throw new ArgumentNullException(nameof(apply));
            }

            if (duration <= 0f)
            {
                apply(1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                apply(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
        }
    }
}

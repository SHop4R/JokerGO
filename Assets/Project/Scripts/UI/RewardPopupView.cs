using System.Collections;
using TMPro;
using UnityEngine;

namespace JokerGO.UI
{
    /// <summary>
    /// "+5 Apple" popup at the collect point: pops in with overshoot, drifts upward
    /// and fades — same language as the dice total reveal.
    /// </summary>
    public sealed class RewardPopupView : MonoBehaviour
    {
        private const float PopDuration = 0.3f;
        private const float HoldDuration = 0.55f;
        private const float FadeDuration = 0.3f;
        private const float DriftPixels = 110f;
        private const float FontSize = 64f;
        private const float Overshoot = 1.70158f;

        private TextMeshProUGUI label;
        private Coroutine playing;

        public static RewardPopupView Build(Transform canvasParent)
        {
            var go = new GameObject("RewardPopup", typeof(RectTransform));
            go.transform.SetParent(canvasParent, false);

            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(560f, 120f);

            var view = go.AddComponent<RewardPopupView>();
            view.label = UiFactory.CreateText(rect, "Value", string.Empty,
                FontSize, UiTheme.Accent, TextAlignmentOptions.Center);
            UiFactory.Stretch(view.label.rectTransform);
            view.label.raycastTarget = false;
            view.label.alpha = 0f;
            return view;
        }

        public void Show(Vector2 screenPosition, string text, Color color)
        {
            if (playing != null)
            {
                StopCoroutine(playing);
            }

            playing = StartCoroutine(PopRoutine(screenPosition, text, color));
        }

        private IEnumerator PopRoutine(Vector2 screenPosition, string text, Color color)
        {
            var rect = (RectTransform)transform;
            // In an overlay canvas, world position equals screen pixels.
            Vector3 start = new Vector3(screenPosition.x, screenPosition.y, 0f);

            label.text = text;
            label.color = color;
            label.alpha = 1f;

            float total = PopDuration + HoldDuration + FadeDuration;
            float elapsed = 0f;
            while (elapsed < total)
            {
                elapsed += Time.deltaTime;
                float drift = Easing01(elapsed / total);
                rect.position = start + Vector3.up * (DriftPixels * drift);

                if (elapsed < PopDuration)
                {
                    float t = Mathf.Clamp01(elapsed / PopDuration);
                    float u = t - 1f;
                    float eased = 1f + (Overshoot + 1f) * u * u * u + Overshoot * u * u;
                    label.rectTransform.localScale = Vector3.one * eased;
                }
                else if (elapsed > PopDuration + HoldDuration)
                {
                    float t = Mathf.Clamp01((elapsed - PopDuration - HoldDuration) / FadeDuration);
                    label.alpha = 1f - t;
                }

                yield return null;
            }

            label.alpha = 0f;
            label.rectTransform.localScale = Vector3.one;
            playing = null;
        }

        private static float Easing01(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t);
        }
    }
}

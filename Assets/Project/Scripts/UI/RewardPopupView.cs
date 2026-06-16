using System.Collections;
using TMPro;
using UnityEngine;

namespace JokerGO.UI.Project.Scripts.UI
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

        [SerializeField] private TextMeshProUGUI label;

        private Coroutine _playing;

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static RewardPopupView Author(Transform canvasParent)
        {
            GameObject go = new("RewardPopup", typeof(RectTransform));
            go.transform.SetParent(canvasParent, false);

            RectTransform rect = (RectTransform)go.transform;
            rect.sizeDelta = new(560f, 120f);

            RewardPopupView view = go.AddComponent<RewardPopupView>();
            view.label = UiFactory.CreateText(rect, "Value", string.Empty,
                FontSize, UiTheme.Accent, TextAlignmentOptions.Center);
            UiFactory.Stretch(view.label.rectTransform);
            view.label.raycastTarget = false;
            view.label.alpha = 0f;
            return view;
        }

        public void Show(Vector2 screenPosition, string text, Color color)
        {
            if (_playing != null) 
                StopCoroutine(_playing);

            _playing = StartCoroutine(PopRoutine(screenPosition, text, color));
        }

        private IEnumerator PopRoutine(Vector2 screenPosition, string text, Color color)
        {
            RectTransform rect = (RectTransform)transform;
            Vector3 start = new(screenPosition.x, screenPosition.y, 0f);

            label.text = text;
            label.color = color;
            label.alpha = 1f;

            const float total = PopDuration + HoldDuration + FadeDuration;
            float elapsed = 0f;
            while (elapsed < total)
            {
                elapsed += Time.deltaTime;
                float drift = Easing01(elapsed / total);
                rect.position = start + Vector3.up * (DriftPixels * drift);

                switch (elapsed)
                {
                    case < PopDuration:
                    {
                        float t = Mathf.Clamp01(elapsed / PopDuration);
                        float u = t - 1f;
                        float eased = 1f + (Overshoot + 1f) * u * u * u + Overshoot * u * u;
                        label.rectTransform.localScale = Vector3.one * eased;
                        break;
                    }

                    case > PopDuration + HoldDuration:
                    {
                        float t = Mathf.Clamp01((elapsed - PopDuration - HoldDuration) / FadeDuration);
                        label.alpha = 1f - t;
                        break;
                    }
                }

                yield return null;
            }

            label.alpha = 0f;
            label.rectTransform.localScale = Vector3.one;
            _playing = null;
        }

        private static float Easing01(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t);
        }
    }
}

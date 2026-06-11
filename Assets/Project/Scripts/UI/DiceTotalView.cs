using System.Collections;
using TMPro;
using UnityEngine;

namespace JokerGO.UI
{
    /// <summary>Big animated total that pops over the dice close-up once they settle.</summary>
    public sealed class DiceTotalView : MonoBehaviour
    {
        private const float PopDuration = 0.35f;
        private const float HoldDuration = 0.75f;
        private const float FadeDuration = 0.25f;
        private const float FontSize = 150f;
        private const float Overshoot = 1.70158f;

        [SerializeField] private TextMeshProUGUI label;

        private Coroutine playing;

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static DiceTotalView Author(Transform canvasParent)
        {
            var go = new GameObject("DiceTotal", typeof(RectTransform));
            go.transform.SetParent(canvasParent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.6f);
            rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.sizeDelta = new Vector2(600f, 220f);

            var view = go.AddComponent<DiceTotalView>();
            view.label = UiFactory.CreateText(rect, "Value", string.Empty,
                FontSize, UiTheme.Accent, TextAlignmentOptions.Center);
            UiFactory.Stretch(view.label.rectTransform);
            view.label.raycastTarget = false;
            view.label.alpha = 0f;
            return view;
        }

        public void Show(int total)
        {
            if (playing != null)
            {
                StopCoroutine(playing);
            }

            playing = StartCoroutine(PopRoutine(total));
        }

        private IEnumerator PopRoutine(int total)
        {
            label.text = total.ToString();
            label.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < PopDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / PopDuration);
                float u = t - 1f;
                float eased = 1f + (Overshoot + 1f) * u * u * u + Overshoot * u * u;
                label.rectTransform.localScale = Vector3.one * eased;
                yield return null;
            }

            yield return new WaitForSeconds(HoldDuration);

            elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeDuration);
                label.alpha = 1f - t;
                label.rectTransform.localScale = Vector3.one * (1f + t * 0.4f);
                yield return null;
            }

            label.alpha = 0f;
            label.rectTransform.localScale = Vector3.one;
            playing = null;
        }
    }
}

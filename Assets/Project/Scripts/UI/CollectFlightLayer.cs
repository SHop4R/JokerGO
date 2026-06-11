using System;
using System.Collections;
using JokerGO.Core;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI
{
    /// <summary>
    /// Animates collected-item chips from a screen point into the inventory counter,
    /// on a curved path with a small stagger per chip.
    /// </summary>
    public sealed class CollectFlightLayer : MonoBehaviour
    {
        private const float ChipSize = 38f;
        private const float FlightDuration = 0.55f;
        private const float StaggerStep = 0.07f;
        private const float CurveSideways = 160f;

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static CollectFlightLayer Author(Transform canvasParent)
        {
            var go = new GameObject("CollectFlight", typeof(RectTransform));
            go.transform.SetParent(canvasParent, false);
            UiFactory.Stretch((RectTransform)go.transform);
            return go.AddComponent<CollectFlightLayer>();
        }

        /// <summary>Flies chips to the target; onFirstArrival fires when the first chip lands.</summary>
        public void Fly(Vector2 fromScreen, ItemType type, RectTransform target,
            int chipCount, Action onFirstArrival)
        {
            if (target == null)
            {
                onFirstArrival?.Invoke();
                return;
            }

            StartCoroutine(FlyAll(fromScreen, type, target, chipCount, onFirstArrival));
        }

        private IEnumerator FlyAll(Vector2 fromScreen, ItemType type, RectTransform target,
            int chipCount, Action onFirstArrival)
        {
            bool arrivedOnce = false;
            for (int i = 0; i < chipCount; i++)
            {
                StartCoroutine(FlyOne(fromScreen, type, target, () =>
                {
                    if (!arrivedOnce)
                    {
                        arrivedOnce = true;
                        onFirstArrival?.Invoke();
                    }
                }));
                yield return new WaitForSeconds(StaggerStep);
            }
        }

        private IEnumerator FlyOne(Vector2 fromScreen, ItemType type, RectTransform target, Action onArrive)
        {
            var chip = new GameObject("Chip", typeof(Image));
            chip.transform.SetParent(transform, false);

            var image = chip.GetComponent<Image>();
            image.color = UiTheme.ItemColor(type);
            image.raycastTarget = false;

            var rect = (RectTransform)chip.transform;
            rect.sizeDelta = new Vector2(ChipSize, ChipSize);

            // In an overlay canvas, world position equals screen pixels.
            Vector3 start = new Vector3(fromScreen.x, fromScreen.y, 0f)
                            + (Vector3)UnityEngine.Random.insideUnitCircle * 26f;
            Vector3 control = Vector3.Lerp(start, target.position, 0.5f)
                              + Vector3.right * UnityEngine.Random.Range(-CurveSideways, CurveSideways);

            float elapsed = 0f;
            while (elapsed < FlightDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FlightDuration);
                float eased = t * t;

                Vector3 a = Vector3.Lerp(start, control, eased);
                Vector3 b = Vector3.Lerp(control, target.position, eased);
                rect.position = Vector3.Lerp(a, b, eased);
                rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.45f, eased);
                yield return null;
            }

            onArrive?.Invoke();
            Destroy(chip);
        }
    }
}

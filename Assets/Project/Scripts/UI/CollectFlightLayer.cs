using System;
using System.Collections;
using JokerGO.Core.Project.Scripts.Core;
using JokerGO.Pooling.Project.Scripts.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI.Project.Scripts.UI
{
    /// <summary>
    /// Animates collected-item chips from a screen point into the inventory counter,
    /// on a curved path with a small stagger per chip. Chips are pooled, never
    /// instantiated/destroyed per collection.
    /// </summary>
    public sealed class CollectFlightLayer : MonoBehaviour
    {
        private const float ChipSize = 38f;
        private const float FlightDuration = 0.55f;
        private const float StaggerStep = 0.07f;
        private const float CurveSideways = 160f;
        private const int ChipPoolDefault = 8;
        private const int ChipPoolMax = 24;

        private static readonly WaitForSeconds StaggerWait = new(StaggerStep);

        private Pool<Image> _chipPool;

        private void Awake()
        {
            Image template = CreateChip("ChipTemplate");
            template.gameObject.SetActive(false);
            _chipPool = new Pool<Image>(
                new PoolStats<Image>(template, ChipPoolDefault, ChipPoolMax, true), transform);
        }

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static CollectFlightLayer Author(Transform canvasParent)
        {
            GameObject go = new("CollectFlight", typeof(RectTransform));
            go.transform.SetParent(canvasParent, false);
            UiFactory.Stretch((RectTransform)go.transform);
            return go.AddComponent<CollectFlightLayer>();
        }

        /// <summary>Flies chips to the target; onFirstArrival fires when the first chip lands.</summary>
        public void Fly(Vector2 fromScreen, ItemType type, RectTransform target,
            int chipCount, Action onFirstArrival)
        {
            if (!target)
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
                    if (arrivedOnce) return;
                    arrivedOnce = true;
                    onFirstArrival?.Invoke();
                }));
                yield return StaggerWait;
            }
        }

        private IEnumerator FlyOne(Vector2 fromScreen, ItemType type, RectTransform target, Action onArrive)
        {
            Vector3 start = new Vector3(fromScreen.x, fromScreen.y, 0f)
                            + (Vector3)UnityEngine.Random.insideUnitCircle * 26f;
            Color color = UiTheme.ItemColor(type);

            Image chip = _chipPool.Spawn(start, image =>
            {
                image.color = color;
                image.rectTransform.localScale = Vector3.one;
            });
            RectTransform rect = chip.rectTransform;

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
            _chipPool.Return(chip);
        }

        /// <summary>Builds one chip Image (RectTransform sized, non-interactive).</summary>
        private Image CreateChip(string chipName)
        {
            GameObject go = new(chipName, typeof(Image));
            go.transform.SetParent(transform, false);

            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            ((RectTransform)go.transform).sizeDelta = new(ChipSize, ChipSize);
            return image;
        }
    }
}

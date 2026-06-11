using System;
using JokerGO.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI
{
    /// <summary>Top-right inventory: one row per item type with a color chip and live count.</summary>
    public sealed class InventoryPanelView : MonoBehaviour
    {
        private const float PanelWidth = 380f;
        private const float RowHeight = 64f;
        private const float ChipSize = 42f;

        // Indexed by (int)ItemType; wired at author time, persisted in the prefab.
        [SerializeField] private TextMeshProUGUI[] countLabels;

        public void Refresh(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            for (int i = 0; i < countLabels.Length; i++)
            {
                countLabels[i].text = inventory.Get((ItemType)i).ToString();
            }
        }

        /// <summary>Screen anchor that collect flights aim for.</summary>
        public RectTransform CounterTarget(ItemType type) => countLabels[(int)type].rectTransform;

        /// <summary>Pops the counter when items arrive.</summary>
        public void Punch(ItemType type)
        {
            StartCoroutine(PunchRoutine(countLabels[(int)type].rectTransform));
        }

        private System.Collections.IEnumerator PunchRoutine(RectTransform target)
        {
            const float duration = 0.28f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static InventoryPanelView Author(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "InventoryPanel", UiTheme.PanelBackground);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-24f, -24f);
            panel.sizeDelta = new Vector2(PanelWidth, 0f);

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 20);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI header = UiFactory.CreateText(panel, "Header", "INVENTORY",
                UiTheme.HeaderFontSize, UiTheme.Accent, TextAlignmentOptions.Left);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;

            var view = panel.gameObject.AddComponent<InventoryPanelView>();
            var types = (ItemType[])Enum.GetValues(typeof(ItemType));
            view.countLabels = new TextMeshProUGUI[types.Length];
            foreach (ItemType type in types)
            {
                view.countLabels[(int)type] = AuthorRow(panel, type);
            }

            return view;
        }

        private static TextMeshProUGUI AuthorRow(RectTransform panel, ItemType type)
        {
            var row = new GameObject($"{type}Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(panel, false);
            row.AddComponent<LayoutElement>().preferredHeight = RowHeight;

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.MiddleLeft;

            var chip = new GameObject("Chip", typeof(Image));
            chip.transform.SetParent(row.transform, false);
            chip.GetComponent<Image>().color = UiTheme.ItemColor(type);
            var chipElement = chip.AddComponent<LayoutElement>();
            chipElement.preferredWidth = ChipSize;
            chipElement.preferredHeight = ChipSize;

            TextMeshProUGUI name = UiFactory.CreateText(row.transform, "Name", type.ToString(),
                UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            TextMeshProUGUI count = UiFactory.CreateText(row.transform, "Count", "0",
                UiTheme.LabelFontSize * 1.15f, UiTheme.TextPrimary, TextAlignmentOptions.MidlineRight);
            count.gameObject.AddComponent<LayoutElement>().preferredWidth = 90f;
            return count;
        }
    }
}

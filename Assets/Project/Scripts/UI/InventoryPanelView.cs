using System;
using System.Collections.Generic;
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

        private readonly Dictionary<ItemType, TextMeshProUGUI> countLabels =
            new Dictionary<ItemType, TextMeshProUGUI>();

        public static InventoryPanelView Build(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "InventoryPanel", UiTheme.PanelBackground);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-24f, -24f);
            // Top-level rect: width must be set directly; the fitter only drives height.
            panel.sizeDelta = new Vector2(PanelWidth, 0f);

            var view = panel.gameObject.AddComponent<InventoryPanelView>();
            view.BuildContent(panel);
            return view;
        }

        public void Refresh(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            foreach (KeyValuePair<ItemType, TextMeshProUGUI> entry in countLabels)
            {
                entry.Value.text = inventory.Get(entry.Key).ToString();
            }
        }

        private void BuildContent(RectTransform panel)
        {
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

            foreach (ItemType type in (ItemType[])Enum.GetValues(typeof(ItemType)))
            {
                BuildRow(panel, type);
            }
        }

        private void BuildRow(RectTransform panel, ItemType type)
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

            countLabels[type] = count;
        }
    }
}

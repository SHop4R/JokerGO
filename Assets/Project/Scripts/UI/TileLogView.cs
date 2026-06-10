using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI
{
    /// <summary>Bottom strip showing the latest landing result ("Landed on tile 12 — +5 Apple!").</summary>
    public sealed class TileLogView : MonoBehaviour
    {
        private const float Height = 72f;
        private const float BottomMargin = 28f;
        private const float WidthMargin = 48f;

        private TextMeshProUGUI label;

        public static TileLogView Build(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "TileLog", UiTheme.PanelBackground);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(0f, BottomMargin);
            panel.sizeDelta = new Vector2(-WidthMargin * 2f, Height);

            var view = panel.gameObject.AddComponent<TileLogView>();
            view.label = UiFactory.CreateText(panel, "Message", "Type your dice and roll!",
                UiTheme.LogFontSize, UiTheme.TextPrimary, TextAlignmentOptions.Center);
            UiFactory.Stretch(view.label.rectTransform);
            return view;
        }

        public void Show(string message)
        {
            label.text = message;
        }
    }
}

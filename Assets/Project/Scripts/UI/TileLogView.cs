using TMPro;
using UnityEngine;

namespace JokerGO.UI.Project.Scripts.UI
{
    /// <summary>Bottom strip showing the latest landing result ("Landed on tile 12 - collected 5 Apple!").</summary>
    public sealed class TileLogView : MonoBehaviour
    {
        private const float Height = 72f;
        private const float BottomMargin = 28f;
        private const float WidthMargin = 48f;

        [SerializeField] private TextMeshProUGUI label;

        public void Show(string message) => label.text = message;

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static TileLogView Author(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "TileLog", UiTheme.PanelBackground);
            panel.anchorMin = new(0f, 0f);
            panel.anchorMax = new(1f, 0f);
            panel.pivot = new(0.5f, 0f);
            panel.anchoredPosition = new(0f, BottomMargin);
            panel.sizeDelta = new(-WidthMargin * 2f, Height);

            var view = panel.gameObject.AddComponent<TileLogView>();
            view.label = UiFactory.CreateText(panel, "Message", "Type your dice and roll!",
                UiTheme.LogFontSize, UiTheme.TextPrimary, TextAlignmentOptions.Center);
            UiFactory.Stretch(view.label.rectTransform);
            return view;
        }
    }
}

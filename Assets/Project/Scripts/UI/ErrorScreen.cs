using TMPro;
using UnityEngine;

namespace JokerGO.UI.Project.Scripts.UI
{
    /// <summary>Full-screen failure notice for unrecoverable startup errors (e.g. broken map data).</summary>
    public static class ErrorScreen
    {
        public static void Show(string message, string hint)
        {
            Canvas canvas = UiFactory.CreateRootCanvas();
            canvas.gameObject.name = "ErrorScreen";

            RectTransform backdrop = UiFactory.CreatePanel(
                canvas.transform, "Backdrop", new(0.08f, 0.05f, 0.04f, 0.92f));
            UiFactory.Stretch(backdrop);

            TextMeshProUGUI title = UiFactory.CreateText(backdrop, "Title", "OOPS!",
                UiTheme.HeaderFontSize * 1.6f, UiTheme.Error, TextAlignmentOptions.Center);
            Place(title.rectTransform, 0.62f, 0.85f);

            TextMeshProUGUI body = UiFactory.CreateText(backdrop, "Message", message,
                UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.Center);
            body.textWrappingMode = TextWrappingModes.Normal;
            Place(body.rectTransform, 0.35f, 0.6f);

            TextMeshProUGUI hintText = UiFactory.CreateText(backdrop, "Hint", hint,
                UiTheme.LabelFontSize * 0.85f, UiTheme.TextMuted, TextAlignmentOptions.Center);
            hintText.textWrappingMode = TextWrappingModes.Normal;
            Place(hintText.rectTransform, 0.22f, 0.33f);
        }

        private static void Place(RectTransform rect, float yMin, float yMax)
        {
            rect.anchorMin = new(0.08f, yMin);
            rect.anchorMax = new(0.92f, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

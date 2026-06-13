using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI
{
    /// <summary>
    /// Code-built uGUI controls used to author the HUD prefab and to build the per-die
    /// rows at runtime. Uses TMP_DefaultControls for the fiddly composite controls
    /// (input field, dropdown).
    /// </summary>
    public static class UiFactory
    {
        public static Canvas CreateRootCanvas()
        {
            var go = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color background)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = background;
            return (RectTransform)go.transform;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }

        public static Button CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.Accent;

            TextMeshProUGUI text = CreateText(go.transform, "Label", label,
                UiTheme.ButtonFontSize, UiTheme.FieldText, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            return go.GetComponent<Button>();
        }

        public static TMP_InputField CreateIntegerField(Transform parent, string name, string placeholder)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.FieldBackground;

            var field = go.GetComponent<TMP_InputField>();
            field.contentType = TMP_InputField.ContentType.IntegerNumber;
            field.characterLimit = 1;
            field.pointSize = UiTheme.FieldFontSize;
            field.textComponent.color = UiTheme.FieldText;
            field.textComponent.alignment = TextAlignmentOptions.Center;

            var placeholderText = (TextMeshProUGUI)field.placeholder;
            placeholderText.text = placeholder;
            placeholderText.fontSize = UiTheme.FieldFontSize * 0.8f;
            placeholderText.color = new Color(0.15f, 0.12f, 0.08f, 0.35f);
            placeholderText.alignment = TextAlignmentOptions.Center;
            placeholderText.fontStyle = FontStyles.Normal;
            return field;
        }

        public static TMP_Dropdown CreateCountDropdown(Transform parent, string name, int minValue, int maxValue)
        {
            GameObject go = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.FieldBackground;

            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            for (int value = minValue; value <= maxValue; value++)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(value.ToString()));
            }

            dropdown.captionText.color = UiTheme.FieldText;
            dropdown.captionText.fontSize = UiTheme.FieldFontSize;
            dropdown.itemText.color = UiTheme.FieldText;
            dropdown.itemText.fontSize = UiTheme.FieldFontSize * 0.85f;

            var item = (RectTransform)dropdown.itemText.transform.parent;
            item.sizeDelta = new Vector2(item.sizeDelta.x, 64f);
            dropdown.template.sizeDelta = new Vector2(dropdown.template.sizeDelta.x, 440f);
            return dropdown;
        }

        /// <summary>Vertical scroll area; rows added to the returned content auto-size it.</summary>
        public static ScrollRect CreateVerticalScroll(Transform parent, string name, out RectTransform content)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
            root.transform.SetParent(parent, false);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(root.transform, false);
            Stretch((RectTransform)viewport.transform);

            var contentGo = new GameObject("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);

            content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = root.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.scrollSensitivity = 30f;
            return scroll;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

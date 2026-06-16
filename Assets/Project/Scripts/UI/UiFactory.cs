using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI.Project.Scripts.UI
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
            GameObject go = new("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color background)
        {
            GameObject go = new(name, typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = background;
            return (RectTransform)go.transform;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = new(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            return tmp;
        }

        public static Button CreateButton(Transform parent, string name, string label)
        {
            GameObject go = new(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.Accent;

            TextMeshProUGUI text = CreateText(go.transform, "Label", label,
                UiTheme.ButtonFontSize, UiTheme.FieldText, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            return go.GetComponent<Button>();
        }

        public static TMP_InputField CreateIntegerField(Transform parent, string name, string placeholder)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(new());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.FieldBackground;

            TMP_InputField field = go.GetComponent<TMP_InputField>();
            field.contentType = TMP_InputField.ContentType.IntegerNumber;
            field.characterLimit = 1;
            field.pointSize = UiTheme.FieldFontSize;
            field.textComponent.color = UiTheme.FieldText;
            field.textComponent.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI placeholderText = (TextMeshProUGUI)field.placeholder;
            placeholderText.text = placeholder;
            placeholderText.fontSize = UiTheme.FieldFontSize * 0.8f;
            placeholderText.color = new(0.15f, 0.12f, 0.08f, 0.35f);
            placeholderText.alignment = TextAlignmentOptions.Center;
            placeholderText.fontStyle = FontStyles.Normal;
            return field;
        }

        public static TMP_Dropdown CreateCountDropdown(Transform parent, string name, int minValue, int maxValue)
        {
            GameObject go = TMP_DefaultControls.CreateDropdown(new());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiTheme.FieldBackground;

            TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            for (int value = minValue; value <= maxValue; value++)
            {
                dropdown.options.Add(new(value.ToString()));
            }

            dropdown.captionText.color = UiTheme.FieldText;
            dropdown.captionText.fontSize = UiTheme.FieldFontSize;
            dropdown.itemText.color = UiTheme.FieldText;
            dropdown.itemText.fontSize = UiTheme.FieldFontSize * 0.85f;

            RectTransform item = (RectTransform)dropdown.itemText.transform.parent;
            item.sizeDelta = new(item.sizeDelta.x, 64f);
            dropdown.template.sizeDelta = new(dropdown.template.sizeDelta.x, 440f);
            return dropdown;
        }

        /// <summary>Vertical scroll area; rows added to the returned content auto-size it.</summary>
        public static ScrollRect CreateVerticalScroll(Transform parent, string name, out RectTransform content)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(ScrollRect));
            root.transform.SetParent(parent, false);

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(root.transform, false);
            Stretch((RectTransform)viewport.transform);

            GameObject contentGo = new("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);

            content = (RectTransform)contentGo.transform;
            content.anchorMin = new(0f, 1f);
            content.anchorMax = new(1f, 1f);
            content.pivot = new(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = root.GetComponent<ScrollRect>();
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

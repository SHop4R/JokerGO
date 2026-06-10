using System;
using System.Collections;
using System.Collections.Generic;
using JokerGO.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI
{
    /// <summary>
    /// Top-left dice controls: count dropdown (1-20), one labeled value box per die,
    /// Roll button, and shake/flash feedback for invalid input.
    /// </summary>
    public sealed class DicePanelView : MonoBehaviour
    {
        private const float PanelWidth = 380f;
        private const float RowHeight = 72f;
        private const float MaxScrollHeight = 460f;
        private const float ErrorVisibleSeconds = 2.6f;

        /// <summary>Raised with parsed values when the player presses Roll with parseable input.</summary>
        public event Action<IReadOnlyList<int>> RollRequested;

        private readonly List<TMP_InputField> fields = new List<TMP_InputField>();
        private RectTransform panel;
        private RectTransform fieldsContent;
        private RectTransform scrollRect;
        private TMP_Dropdown countDropdown;
        private Button rollButton;
        private TextMeshProUGUI errorLabel;
        private CanvasGroup interactivity;
        private Coroutine feedbackRoutine;

        public static DicePanelView Build(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "DicePanel", UiTheme.PanelBackground);
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(24f, -24f);
            // Top-level rect: width must be set directly; the fitter only drives height.
            panel.sizeDelta = new Vector2(PanelWidth, 0f);

            var view = panel.gameObject.AddComponent<DicePanelView>();
            view.panel = panel;
            view.interactivity = panel.gameObject.AddComponent<CanvasGroup>();
            view.BuildContent();
            view.RebuildFields(2);
            return view;
        }

        public void SetInteractable(bool value)
        {
            interactivity.interactable = value;
            interactivity.alpha = value ? 1f : 0.6f;
        }

        public void ShowError(string message)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(FeedbackRoutine(message));
        }

        private void BuildContent()
        {
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 20);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI header = UiFactory.CreateText(panel, "Header", "DICE",
                UiTheme.HeaderFontSize, UiTheme.Accent, TextAlignmentOptions.Left);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;

            BuildCountRow();
            BuildFieldsScroll();
            BuildRollButton();
            BuildErrorLabel();
        }

        private void BuildCountRow()
        {
            RectTransform row = CreateRow(panel, "CountRow");

            TextMeshProUGUI label = UiFactory.CreateText(row, "Label", "Dice count",
                UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
            label.gameObject.AddComponent<LayoutElement>().preferredWidth = 170f;

            countDropdown = UiFactory.CreateCountDropdown(row, "CountDropdown",
                DiceRules.MinDiceCount, DiceRules.MaxDiceCount);
            countDropdown.GetComponent<RectTransform>().gameObject
                .AddComponent<LayoutElement>().flexibleWidth = 1f;
            countDropdown.value = 1; // option index 1 == 2 dice
            countDropdown.onValueChanged.AddListener(index => RebuildFields(index + DiceRules.MinDiceCount));
        }

        private void BuildFieldsScroll()
        {
            ScrollRect scroll = UiFactory.CreateVerticalScroll(panel, "Fields", out fieldsContent);
            scrollRect = (RectTransform)scroll.transform;
            scrollRect.gameObject.AddComponent<LayoutElement>();
        }

        private void BuildRollButton()
        {
            rollButton = UiFactory.CreateButton(panel, "RollButton", "ROLL");
            rollButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 84f;
            rollButton.onClick.AddListener(OnRollClicked);
        }

        private void BuildErrorLabel()
        {
            errorLabel = UiFactory.CreateText(panel, "ErrorLabel", string.Empty,
                UiTheme.LabelFontSize * 0.85f, UiTheme.Error, TextAlignmentOptions.Left);
            errorLabel.textWrappingMode = TextWrappingModes.Normal;
            errorLabel.gameObject.AddComponent<LayoutElement>().minHeight = 0f;
        }

        private void RebuildFields(int count)
        {
            var previous = new List<string>(fields.Count);
            foreach (TMP_InputField field in fields)
            {
                previous.Add(field.text);
            }

            foreach (Transform child in fieldsContent)
            {
                Destroy(child.gameObject);
            }

            fields.Clear();

            for (int i = 0; i < count; i++)
            {
                RectTransform row = CreateRow(fieldsContent, $"DieRow {i + 1}");
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;

                TextMeshProUGUI label = UiFactory.CreateText(row, "Label", $"Die {i + 1}",
                    UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
                label.textWrappingMode = TextWrappingModes.NoWrap;
                // minWidth, not just preferred: the input field's large preferred width
                // would otherwise squeeze the label to nothing.
                LayoutElement labelElement = label.gameObject.AddComponent<LayoutElement>();
                labelElement.minWidth = 140f;
                labelElement.preferredWidth = 140f;

                TMP_InputField field = UiFactory.CreateIntegerField(row, "Value",
                    $"{DiceRules.MinValue}-{DiceRules.MaxValue}");
                field.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
                if (i < previous.Count)
                {
                    field.SetTextWithoutNotify(previous[i]);
                }

                fields.Add(field);
            }

            // The scroll area grows with rows up to a cap, then actually scrolls.
            float desired = Mathf.Min(count * (RowHeight + 12f), MaxScrollHeight);
            scrollRect.GetComponent<LayoutElement>().preferredHeight = desired;
        }

        private void OnRollClicked()
        {
            var values = new List<int>(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                if (!int.TryParse(fields[i].text.Trim(), out int value))
                {
                    ShowError($"Die {i + 1} needs a value between " +
                              $"{DiceRules.MinValue} and {DiceRules.MaxValue}.");
                    return;
                }

                values.Add(value);
            }

            RollRequested?.Invoke(values);
        }

        private IEnumerator FeedbackRoutine(string message)
        {
            errorLabel.text = message;

            var fieldImages = new List<Image>(fields.Count);
            foreach (TMP_InputField field in fields)
            {
                fieldImages.Add(field.GetComponent<Image>());
            }

            Vector2 origin = panel.anchoredPosition;
            const float shakeDuration = 0.4f;
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - elapsed / shakeDuration;
                panel.anchoredPosition = origin + Vector2.right * (Mathf.Sin(elapsed * 55f) * 10f * decay);

                float flash = Mathf.PingPong(elapsed * 4f, 1f);
                foreach (Image image in fieldImages)
                {
                    image.color = Color.Lerp(UiTheme.FieldBackground, UiTheme.Error, flash * 0.6f);
                }

                yield return null;
            }

            panel.anchoredPosition = origin;
            foreach (Image image in fieldImages)
            {
                image.color = UiTheme.FieldBackground;
            }

            yield return new WaitForSeconds(ErrorVisibleSeconds);
            errorLabel.text = string.Empty;
            feedbackRoutine = null;
        }

        private static RectTransform CreateRow(Transform parent, string name)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            return (RectTransform)row.transform;
        }
    }
}

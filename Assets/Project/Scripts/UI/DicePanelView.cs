using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JokerGO.Core.Project.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JokerGO.UI.Project.Scripts.UI
{
    /// <summary>
    /// Top-left dice controls: count dropdown (1-20), one labeled value box per die,
    /// Roll button, and shake/flash feedback for invalid input. The static frame lives
    /// in the HUD prefab; only the per-die rows are rebuilt at runtime.
    /// </summary>
    public sealed class DicePanelView : MonoBehaviour
    {
        private const float PanelWidth = 380f;
        private const float RowHeight = 72f;
        private const float MaxScrollHeight = 460f;
        private const float ErrorVisibleSeconds = 2.6f;

        /// <summary>Raised with the final values (typed, or random for empty boxes) on Roll.</summary>
        public event Action<IReadOnlyList<int>> RollRequested;

        [SerializeField] private RectTransform fieldsContent;
        [SerializeField] private LayoutElement scrollSize;
        [SerializeField] private TMP_Dropdown countDropdown;
        [SerializeField] private Button rollButton;
        [SerializeField] private TextMeshProUGUI errorLabel;
        [SerializeField] private CanvasGroup interactivity;

        private readonly List<TMP_InputField> _fields = new();
        private RectTransform _panel;
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            _panel = (RectTransform)transform;
            countDropdown.onValueChanged.AddListener(index => RebuildFields(index + DiceRules.MinDiceCount));
            rollButton.onClick.AddListener(OnRollClicked);
            RebuildFields(countDropdown.value + DiceRules.MinDiceCount);
        }

        public void SetInteractable(bool value)
        {
            interactivity.interactable = value;
            interactivity.alpha = value ? 1f : 0.6f;
        }

        public void ShowError(string message)
        {
            if (_feedbackRoutine != null) 
                StopCoroutine(_feedbackRoutine);

            _feedbackRoutine = StartCoroutine(FeedbackRoutine(message));
        }

        private void RebuildFields(int count)
        {
            List<string> previous = new(_fields.Count);
            previous.AddRange(_fields.Select(field => field.text));

            foreach (Transform child in fieldsContent)
            {
                Destroy(child.gameObject);
            }

            _fields.Clear();

            for (int i = 0; i < count; i++)
            {
                RectTransform row = CreateRow(fieldsContent, $"DieRow {i + 1}");
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;

                TextMeshProUGUI label = UiFactory.CreateText(row, "Label", $"Die {i + 1}",
                    UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
                label.textWrappingMode = TextWrappingModes.NoWrap;
                LayoutElement labelElement = label.gameObject.AddComponent<LayoutElement>();
                labelElement.minWidth = 140f;
                labelElement.preferredWidth = 140f;

                TMP_InputField field = UiFactory.CreateIntegerField(row, "Value", $"{DiceRules.MinValue}-{DiceRules.MaxValue} / ?");
                field.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
                
                if (i < previous.Count)
                    field.SetTextWithoutNotify(previous[i]);

                _fields.Add(field);
            }

            scrollSize.preferredHeight = Mathf.Min(count * (RowHeight + 12f), MaxScrollHeight);
        }

        private void OnRollClicked()
        {
            List<int> values = new(_fields.Count);
            for (int i = 0; i < _fields.Count; i++)
            {
                string raw = _fields[i].text.Trim();
                if (string.IsNullOrEmpty(raw))
                {
                    values.Add(UnityEngine.Random.Range(DiceRules.MinValue, DiceRules.MaxValue + 1));
                    continue;
                }

                if (!int.TryParse(raw, out int value))
                {
                    ShowError($"Die {i + 1} needs a value between " + $"{DiceRules.MinValue} and {DiceRules.MaxValue}, or empty for random.");
                    return;
                }

                values.Add(value);
            }

            RollRequested?.Invoke(values);
        }

        private IEnumerator FeedbackRoutine(string message)
        {
            errorLabel.text = message;

            List<Image> fieldImages = new(_fields.Count);
            fieldImages.AddRange(_fields.Select(field => field.GetComponent<Image>()));

            Vector2 origin = _panel.anchoredPosition;
            const float shakeDuration = 0.4f;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - elapsed / shakeDuration;
                _panel.anchoredPosition = origin + Vector2.right * (Mathf.Sin(elapsed * 55f) * 10f * decay);

                float flash = Mathf.PingPong(elapsed * 4f, 1f);
                foreach (Image image in fieldImages)
                {
                    image.color = Color.Lerp(UiTheme.FieldBackground, UiTheme.Error, flash * 0.6f);
                }

                yield return null;
            }

            _panel.anchoredPosition = origin;
            foreach (Image image in fieldImages)
            {
                image.color = UiTheme.FieldBackground;
            }

            yield return new WaitForSeconds(ErrorVisibleSeconds);
            errorLabel.text = string.Empty;
            _feedbackRoutine = null;
        }

        private static RectTransform CreateRow(Transform parent, string name)
        {
            GameObject row = new(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            return (RectTransform)row.transform;
        }

        /// <summary>Editor-time construction; the result is saved into the HUD prefab.</summary>
        public static DicePanelView Author(Transform canvasParent)
        {
            RectTransform panel = UiFactory.CreatePanel(canvasParent, "DicePanel", UiTheme.PanelBackground);
            panel.anchorMin = new(0f, 1f);
            panel.anchorMax = new(0f, 1f);
            panel.pivot = new(0f, 1f);
            panel.anchoredPosition = new(24f, -24f);
            panel.sizeDelta = new(PanelWidth, 0f);

            DicePanelView view = panel.gameObject.AddComponent<DicePanelView>();
            view.interactivity = panel.gameObject.AddComponent<CanvasGroup>();

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new(20, 20, 16, 20);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI header = UiFactory.CreateText(panel, "Header", "DICE",
                UiTheme.HeaderFontSize, UiTheme.Accent, TextAlignmentOptions.Left);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;

            RectTransform countRow = CreateRow(panel, "CountRow");
            TextMeshProUGUI countLabel = UiFactory.CreateText(countRow, "Label", "Dice count",
                UiTheme.LabelFontSize, UiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
            countLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 170f;

            view.countDropdown = UiFactory.CreateCountDropdown(countRow, "CountDropdown",
                DiceRules.MinDiceCount, DiceRules.MaxDiceCount);
            view.countDropdown.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            view.countDropdown.SetValueWithoutNotify(1);

            ScrollRect scroll = UiFactory.CreateVerticalScroll(panel, "Fields", out view.fieldsContent);
            view.scrollSize = scroll.gameObject.AddComponent<LayoutElement>();

            view.rollButton = UiFactory.CreateButton(panel, "RollButton", "ROLL");
            view.rollButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 84f;

            view.errorLabel = UiFactory.CreateText(panel, "ErrorLabel", string.Empty,
                UiTheme.LabelFontSize * 0.85f, UiTheme.Error, TextAlignmentOptions.Left);
            view.errorLabel.textWrappingMode = TextWrappingModes.Normal;
            view.errorLabel.gameObject.AddComponent<LayoutElement>().minHeight = 0f;

            return view;
        }
    }
}

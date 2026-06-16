using JokerGO.Core.Project.Scripts.Core;
using UnityEngine;

namespace JokerGO.UI.Project.Scripts.UI
{
    /// <summary>Shared HUD colors and sizes; item colors mirror the board materials.</summary>
    public static class UiTheme
    {
        public static readonly Color PanelBackground = new(0.08f, 0.06f, 0.05f, 0.55f);
        public static readonly Color FieldBackground = new(0.98f, 0.96f, 0.9f, 0.95f);
        public static readonly Color FieldText = new(0.15f, 0.12f, 0.08f);
        public static readonly Color TextPrimary = new(0.98f, 0.96f, 0.9f);
        public static readonly Color TextMuted = new(0.98f, 0.96f, 0.9f, 0.6f);
        public static readonly Color Accent = new(0.95f, 0.62f, 0.22f);
        public static readonly Color Error = new(0.95f, 0.33f, 0.27f);

        public const float HeaderFontSize = 38f;
        public const float LabelFontSize = 30f;
        public const float FieldFontSize = 34f;
        public const float ButtonFontSize = 40f;
        public const float LogFontSize = 32f;

        public static Color ItemColor(ItemType type) =>
            type switch
            {
                ItemType.Apple => new(0.85f, 0.2f, 0.18f),
                ItemType.Pear => new(0.72f, 0.82f, 0.25f),
                _ => new(0.95f, 0.35f, 0.5f)
            };
    }
}

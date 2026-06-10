using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Visual constants and shared materials for the gray-box board (replaced by art on Day 4).</summary>
    public sealed class BoardStyle
    {
        public float TileSpacing { get; private set; }
        public Vector3 TileScale { get; private set; }
        public float NumberFontSize { get; private set; }
        public Color NumberColor { get; private set; }
        public Material TileMaterialA { get; private set; }
        public Material TileMaterialB { get; private set; }

        private Material appleMaterial;
        private Material pearMaterial;
        private Material strawberryMaterial;

        public Material GetItemMaterial(ItemType type)
        {
            switch (type)
            {
                case ItemType.Apple: return appleMaterial;
                case ItemType.Pear: return pearMaterial;
                default: return strawberryMaterial;
            }
        }

        public static BoardStyle CreateDefault()
        {
            return new BoardStyle
            {
                TileSpacing = 2.0f,
                TileScale = new Vector3(1.7f, 0.25f, 1.7f),
                NumberFontSize = 8f,
                NumberColor = new Color(0.15f, 0.12f, 0.08f),
                TileMaterialA = CreateLitMaterial(new Color(0.85f, 0.78f, 0.62f)),
                TileMaterialB = CreateLitMaterial(new Color(0.72f, 0.64f, 0.48f)),
                appleMaterial = CreateLitMaterial(new Color(0.85f, 0.2f, 0.18f)),
                pearMaterial = CreateLitMaterial(new Color(0.72f, 0.82f, 0.25f)),
                strawberryMaterial = CreateLitMaterial(new Color(0.95f, 0.35f, 0.5f))
            };
        }

        private static Material CreateLitMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new MapValidationException("URP Lit shader not found; is URP active?");
            }

            return new Material(shader) { color = color };
        }

        private BoardStyle()
        {
        }
    }
}

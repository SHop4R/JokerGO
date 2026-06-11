using System;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Visual constants for the board plus the shared material assets it renders with.</summary>
    public sealed class BoardStyle
    {
        public float TileSpacing { get; private set; }
        public float PathAmplitude { get; private set; }
        public float PathWavelength { get; private set; }
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
                // Spacing/amplitude pairing matters: at the steepest slope (A*2pi/wavelength)
                // the along-Z gap must stay above the 1.7 tile footprint or corners overlap.
                TileSpacing = 2.25f,
                PathAmplitude = 2.6f,
                PathWavelength = 24f,
                TileScale = new Vector3(1.7f, 0.25f, 1.7f),
                NumberFontSize = 8f,
                NumberColor = new Color(0.15f, 0.12f, 0.08f),
                TileMaterialA = LoadMaterial("TileA"),
                TileMaterialB = LoadMaterial("TileB"),
                appleMaterial = LoadMaterial("Apple"),
                pearMaterial = LoadMaterial("Pear"),
                strawberryMaterial = LoadMaterial("Strawberry")
            };
        }

        private static Material LoadMaterial(string materialName)
        {
            var material = Resources.Load<Material>($"Materials/{materialName}");
            if (material == null)
            {
                throw new InvalidOperationException(
                    $"Material '{materialName}' is missing; run JokerGO > Generate Art Assets in the editor.");
            }

            return material;
        }

        private BoardStyle()
        {
        }
    }
}

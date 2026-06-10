using System;
using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Visual constants and shared materials for the gray-box board (replaced by art on Day 4).</summary>
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

        private readonly List<Material> ownedMaterials = new List<Material>();
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
            var style = new BoardStyle
            {
                // Spacing/amplitude pairing matters: at the steepest slope (A*2pi/wavelength)
                // the along-Z gap must stay above the 1.7 tile footprint or corners overlap.
                TileSpacing = 2.25f,
                PathAmplitude = 2.6f,
                PathWavelength = 24f,
                TileScale = new Vector3(1.7f, 0.25f, 1.7f),
                NumberFontSize = 8f,
                NumberColor = new Color(0.15f, 0.12f, 0.08f)
            };

            style.TileMaterialA = style.CreateLitMaterial(new Color(0.85f, 0.78f, 0.62f));
            style.TileMaterialB = style.CreateLitMaterial(new Color(0.72f, 0.64f, 0.48f));
            style.appleMaterial = style.CreateLitMaterial(new Color(0.85f, 0.2f, 0.18f));
            style.pearMaterial = style.CreateLitMaterial(new Color(0.72f, 0.82f, 0.25f));
            style.strawberryMaterial = style.CreateLitMaterial(new Color(0.95f, 0.35f, 0.5f));
            return style;
        }

        /// <summary>Destroys the runtime-created materials. Call from the owner's OnDestroy.</summary>
        public void DestroyMaterials()
        {
            foreach (Material material in ownedMaterials)
            {
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                }
            }

            ownedMaterials.Clear();
        }

        private Material CreateLitMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP Lit shader not found; is URP active?");
            }

            var material = new Material(shader) { color = color };
            ownedMaterials.Add(material);
            return material;
        }

        private BoardStyle()
        {
        }
    }
}

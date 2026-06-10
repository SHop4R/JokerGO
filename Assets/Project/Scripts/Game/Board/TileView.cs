using JokerGO.Core;
using TMPro;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Gray-box visual for one tile: base block, tile number, optional reward marker.</summary>
    public sealed class TileView : MonoBehaviour
    {
        private const float LabelLift = 0.02f;
        private const float RewardLabelHeight = 1.1f;
        private const float RewardMarkerScale = 0.45f;

        public MapTile Tile { get; private set; }

        /// <summary>Where a token should stand on this tile.</summary>
        public Vector3 TokenAnchor { get; private set; }

        public static TileView Create(MapTile tile, Vector3 position, BoardStyle style)
        {
            var root = new GameObject($"Tile {tile.DisplayNumber}");
            root.transform.position = position;

            TileView view = root.AddComponent<TileView>();
            view.Tile = tile;
            view.TokenAnchor = position + Vector3.up * (style.TileScale.y * 0.5f);
            view.BuildVisuals(style);
            return view;
        }

        private void BuildVisuals(BoardStyle style)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Block";
            block.transform.SetParent(transform, false);
            block.transform.localScale = style.TileScale;
            block.GetComponent<Renderer>().sharedMaterial =
                Tile.Index % 2 == 0 ? style.TileMaterialA : style.TileMaterialB;

            GameObject number = CreateLabel("Number", Tile.DisplayNumber.ToString(),
                style.NumberFontSize, style.NumberColor);
            number.transform.SetParent(transform, false);
            number.transform.localPosition = Vector3.up * (style.TileScale.y * 0.5f + LabelLift);
            number.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            if (Tile.HasReward)
            {
                BuildRewardMarker(Tile.Reward.Value, style);
            }
        }

        private void BuildRewardMarker(ItemStack reward, BoardStyle style)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "RewardMarker";
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = Vector3.one * RewardMarkerScale;
            marker.transform.localPosition = new Vector3(0.5f, style.TileScale.y * 0.5f + RewardMarkerScale * 0.5f, 0.4f);
            marker.GetComponent<Renderer>().sharedMaterial = style.GetItemMaterial(reward.Type);

            GameObject label = CreateLabel("RewardLabel", $"{reward.Amount} {reward.Type}",
                style.NumberFontSize * 0.45f, style.GetItemMaterial(reward.Type).color);
            label.transform.SetParent(transform, false);
            label.transform.localPosition = Vector3.up * RewardLabelHeight;
        }

        private static GameObject CreateLabel(string name, string text, float fontSize, Color color)
        {
            var go = new GameObject(name);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(2f, 1f);
            return go;
        }
    }
}

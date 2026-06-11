using JokerGO.Core;
using TMPro;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>
    /// One board tile, instantiated from the Tile prefab and configured per map data:
    /// number, alternating block material, optional reward marker.
    /// </summary>
    public sealed class TileView : MonoBehaviour
    {
        private const float LabelLift = 0.02f;
        private const float RewardMarkerScale = 0.45f;

        [SerializeField] private Transform block;
        [SerializeField] private Renderer blockRenderer;
        [SerializeField] private TextMeshPro numberLabel;
        [SerializeField] private GameObject rewardMarker;
        [SerializeField] private Renderer rewardMarkerRenderer;

        public MapTile Tile { get; private set; }

        /// <summary>Where a token should stand on this tile.</summary>
        public Vector3 TokenAnchor { get; private set; }

        private Coroutine pressRoutine;

        /// <summary>Applies map data to a freshly spawned tile instance.</summary>
        public void Configure(MapTile tile, BoardStyle style)
        {
            Tile = tile;
            TokenAnchor = transform.position + Vector3.up * (style.TileScale.y * 0.5f);

            numberLabel.text = tile.DisplayNumber.ToString();
            blockRenderer.sharedMaterial = tile.Index % 2 == 0 ? style.TileMaterialA : style.TileMaterialB;

            rewardMarker.SetActive(tile.HasReward);
            if (tile.HasReward)
            {
                rewardMarkerRenderer.sharedMaterial = style.GetItemMaterial(tile.Reward.Value.Type);
            }
        }

        /// <summary>Quick dip-and-spring of the block when something lands on the tile.</summary>
        public void PressBounce()
        {
            if (pressRoutine != null)
            {
                StopCoroutine(pressRoutine);
            }

            pressRoutine = StartCoroutine(PressRoutine());
        }

        private System.Collections.IEnumerator PressRoutine()
        {
            const float dip = 0.085f;
            const float downTime = 0.06f;
            const float upTime = 0.22f;

            Vector3 origin = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < downTime)
            {
                elapsed += Time.deltaTime;
                block.localPosition = origin + Vector3.down * (dip * Mathf.Clamp01(elapsed / downTime));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < upTime)
            {
                elapsed += Time.deltaTime;
                float t = Tweening.Easing.Evaluate(Tweening.EaseType.EaseOutBack, elapsed / upTime);
                block.localPosition = origin + Vector3.down * (dip * (1f - t));
                yield return null;
            }

            block.localPosition = origin;
            pressRoutine = null;
        }

        /// <summary>Editor-time construction of the Tile prefab hierarchy.</summary>
        public static GameObject Author(BoardStyle style)
        {
            var root = new GameObject("Tile");
            var view = root.AddComponent<TileView>();

            var blockGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blockGo.name = "Block";
            blockGo.transform.SetParent(root.transform, false);
            blockGo.transform.localScale = style.TileScale;
            view.block = blockGo.transform;
            view.blockRenderer = blockGo.GetComponent<Renderer>();
            view.blockRenderer.sharedMaterial = style.TileMaterialA;

            var labelGo = new GameObject("Number");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = Vector3.up * (style.TileScale.y * 0.5f + LabelLift);
            labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            view.numberLabel = labelGo.AddComponent<TextMeshPro>();
            view.numberLabel.text = "0";
            view.numberLabel.fontSize = style.NumberFontSize;
            view.numberLabel.color = style.NumberColor;
            view.numberLabel.alignment = TextAlignmentOptions.Center;
            view.numberLabel.rectTransform.sizeDelta = new Vector2(2f, 1f);

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "RewardMarker";
            marker.transform.SetParent(root.transform, false);
            marker.transform.localScale = Vector3.one * RewardMarkerScale;
            marker.transform.localPosition = new Vector3(
                0.5f, style.TileScale.y * 0.5f + RewardMarkerScale * 0.5f, 0.4f);
            view.rewardMarker = marker;
            view.rewardMarkerRenderer = marker.GetComponent<Renderer>();
            marker.SetActive(false);

            return root;
        }
    }
}

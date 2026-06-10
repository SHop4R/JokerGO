using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Instantiates tile views for a <see cref="BoardMap"/> along a winding serpentine path.</summary>
    public sealed class BoardBuilder : MonoBehaviour
    {
        private readonly List<TileView> tileViews = new List<TileView>();
        private Vector3[] positions;

        public IReadOnlyList<TileView> TileViews => tileViews;

        /// <summary>Tile ground positions, exposed so the environment can keep the path clear.</summary>
        public IReadOnlyList<Vector3> TilePositions => positions;

        public void Build(BoardMap map, BoardStyle style)
        {
            positions = SerpentinePathLayout.Compute(
                map.TileCount, style.TileSpacing, style.PathAmplitude, style.PathWavelength);

            for (int i = 0; i < map.TileCount; i++)
            {
                TileView view = TileView.Create(map[i], positions[i], style);
                view.transform.SetParent(transform, worldPositionStays: true);
                tileViews.Add(view);
            }
        }

        /// <summary>
        /// Normalized travel direction at a tile. The last tile continues its incoming
        /// segment instead of pointing back across the board along the wrap chord —
        /// this feeds the dice tray, which must stay in front of the camera.
        /// </summary>
        public Vector3 PathDirectionAt(int tileIndex)
        {
            if (positions == null || positions.Length < 2)
            {
                return Vector3.forward;
            }

            Vector3 direction = tileIndex >= positions.Length - 1
                ? positions[positions.Length - 1] - positions[positions.Length - 2]
                : positions[tileIndex + 1] - positions[tileIndex];
            direction.y = 0f;
            return direction.sqrMagnitude < 0.0001f ? Vector3.forward : direction.normalized;
        }

        public Vector3 TokenPositionOn(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= tileViews.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(tileIndex), tileIndex,
                    $"Tile index must be within [0, {tileViews.Count - 1}]; was Build() called first?");
            }

            return tileViews[tileIndex].TokenAnchor;
        }
    }
}

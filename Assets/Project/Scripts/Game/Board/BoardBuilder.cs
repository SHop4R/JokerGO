using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Board
{
    /// <summary>Instantiates tile views for a <see cref="BoardMap"/> along a straight line.</summary>
    public sealed class BoardBuilder : MonoBehaviour
    {
        private readonly List<TileView> tileViews = new List<TileView>();

        public IReadOnlyList<TileView> TileViews => tileViews;

        public void Build(BoardMap map, BoardStyle style)
        {
            for (int i = 0; i < map.TileCount; i++)
            {
                Vector3 position = new Vector3(0f, 0f, i * style.TileSpacing);
                TileView view = TileView.Create(map[i], position, style);
                view.transform.SetParent(transform, worldPositionStays: true);
                tileViews.Add(view);
            }
        }

        public Vector3 TokenPositionOn(int tileIndex) => tileViews[tileIndex].TokenAnchor;
    }
}

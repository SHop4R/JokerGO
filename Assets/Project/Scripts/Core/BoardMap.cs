using System;
using System.Collections.Generic;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Validated, immutable sequence of tiles forming the linear board.</summary>
    public sealed class BoardMap
    {
        public const int MinimumTileCount = 2;

        private readonly MapTile[] _tiles;

        public int TileCount => _tiles.Length;
        public MapTile this[int index] => _tiles[index];

        public BoardMap(IReadOnlyList<MapTile> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count < MinimumTileCount)
                throw new($"A board needs at least {MinimumTileCount} tiles, got {source.Count}.");

            _tiles = new MapTile[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].Index != i)
                    throw new($"Tile at position {i} carries mismatched index {source[i].Index}.");

                _tiles[i] = source[i];
            }
        }
    }
}

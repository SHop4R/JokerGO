using System;
using System.Collections.Generic;

namespace JokerGO.Core
{
    /// <summary>Validated, immutable sequence of tiles forming the linear board.</summary>
    public sealed class BoardMap
    {
        public const int MinimumTileCount = 2;

        private readonly MapTile[] tiles;

        public int TileCount => tiles.Length;
        public IReadOnlyList<MapTile> Tiles => tiles;
        public MapTile this[int index] => tiles[index];

        public BoardMap(IReadOnlyList<MapTile> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Count < MinimumTileCount)
            {
                throw new MapValidationException(
                    $"A board needs at least {MinimumTileCount} tiles, got {source.Count}.");
            }

            tiles = new MapTile[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].Index != i)
                {
                    throw new MapValidationException(
                        $"Tile at position {i} carries mismatched index {source[i].Index}.");
                }

                tiles[i] = source[i];
            }
        }
    }
}

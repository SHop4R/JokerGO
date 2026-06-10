using System;
using System.Collections.Generic;

namespace JokerGO.Core
{
    /// <summary>Turns raw map data into a validated <see cref="BoardMap"/>, failing with clear messages.</summary>
    public static class BoardMapFactory
    {
        private const string EmptyKeyword = "empty";

        public static BoardMap Create(MapFileDto dto)
        {
            if (dto?.tiles == null)
            {
                throw new MapValidationException("Map file has no 'tiles' array.");
            }

            var tiles = new List<MapTile>(dto.tiles.Count);
            for (int i = 0; i < dto.tiles.Count; i++)
            {
                tiles.Add(CreateTile(dto.tiles[i], i));
            }

            return new BoardMap(tiles);
        }

        private static MapTile CreateTile(TileDto dto, int index)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.item)
                || dto.item.Trim().Equals(EmptyKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return new MapTile(index, null);
            }

            if (!TryParseItem(dto.item, out ItemType type))
            {
                throw new MapValidationException(
                    $"Tile {index + 1}: unknown item '{dto.item}'. Expected apple, pear, strawberry or empty.");
            }

            if (dto.amount <= 0)
            {
                throw new MapValidationException(
                    $"Tile {index + 1}: amount for '{dto.item}' must be positive, got {dto.amount}.");
            }

            return new MapTile(index, new ItemStack(type, dto.amount));
        }

        private static bool TryParseItem(string raw, out ItemType type)
        {
            return Enum.TryParse(raw.Trim(), ignoreCase: true, out type) && Enum.IsDefined(typeof(ItemType), type);
        }
    }
}

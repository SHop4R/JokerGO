using System;
using System.Collections.Generic;
using System.Linq;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Turns raw map data into a validated <see cref="BoardMap"/>, failing with clear messages.</summary>
    public static class BoardMapFactory
    {
        private const string EmptyKeyword = "empty";

        public static BoardMap Create(MapFileDto dto)
        {
            if (dto?.tiles == null)
                throw new("Map file is missing a 'tiles' array (check the JSON structure).");

            List<MapTile> tiles = new(dto.tiles.Count);
            tiles.AddRange(dto.tiles.Select(CreateTile));

            return new(tiles);
        }

        private static MapTile CreateTile(TileDto dto, int index)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.item) || dto.item.Trim().Equals(EmptyKeyword, StringComparison.OrdinalIgnoreCase))
                return new(index, null);

            if (!TryParseItem(dto.item, out ItemType type))
                throw new($"Tile {index + 1}: unknown item '{dto.item}'. Expected apple, pear, strawberry or empty.");

            return dto.amount <= 0 
                ? throw new($"Tile {index + 1}: amount for '{dto.item}' must be positive, got {dto.amount}.")
                : new(index, new ItemStack(type, dto.amount));
        }

        private static bool TryParseItem(string raw, out ItemType type) 
            => Enum.TryParse(raw.Trim(), ignoreCase: true, out type) && Enum.IsDefined(typeof(ItemType), type);
    }
}

using System;
using System.Collections.Generic;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Raw shape of map.json before validation. Field names match the JSON keys.</summary>
    [Serializable]
    public sealed class MapFileDto
    {
        public List<TileDto> tiles;
    }

    /// <summary>One raw tile entry: an item name ("apple", "pear", "strawberry", "empty") and amount.</summary>
    [Serializable]
    public sealed class TileDto
    {
        public string item;
        public int amount;
    }
}

using System;
using System.IO;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Data
{
    /// <summary>Loads the board from StreamingAssets/map.json so evaluators can edit it without rebuilding.</summary>
    public sealed class JsonMapSource : IMapSource
    {
        public const string FileName = "map.json";

        public BoardMap Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
            if (!File.Exists(path))
            {
                throw new MapValidationException($"Map file not found: {path}");
            }

            MapFileDto dto;
            try
            {
                dto = JsonUtility.FromJson<MapFileDto>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                throw new MapValidationException($"Map file is not valid JSON: {e.Message}");
            }

            return BoardMapFactory.Create(dto);
        }
    }
}

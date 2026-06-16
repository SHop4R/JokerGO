using System;
using System.IO;
using JokerGO.Core.Project.Scripts.Core;
using UnityEngine;

namespace JokerGO.Game.Project.Scripts.Game.Data
{
    /// <summary>Loads the board from StreamingAssets/map.json so evaluators can edit it without rebuilding.</summary>
    public sealed class JsonMapSource : IMapSource
    {
        private const string FileName = "map.json";

        public BoardMap Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
            
            if (!File.Exists(path))
                throw new($"Map file not found: {path}");

            MapFileDto dto;
            try
            {
                dto = JsonUtility.FromJson<MapFileDto>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                throw new($"Map file is not valid JSON: {e.Message}");
            }

            return BoardMapFactory.Create(dto);
        }
    }
}

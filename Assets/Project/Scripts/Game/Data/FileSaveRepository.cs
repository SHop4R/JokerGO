using System;
using System.IO;
using JokerGO.Core;
using UnityEngine;

namespace JokerGO.Game.Data
{
    /// <summary>Stores SaveData as JSON under Application.persistentDataPath.</summary>
    public sealed class FileSaveRepository : ISaveRepository
    {
        public const string FileName = "save.json";

        private readonly string path = Path.Combine(Application.persistentDataPath, FileName);

        public SaveData Load()
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JokerGO] Save file unreadable, starting fresh. ({e.Message})");
                return null;
            }
        }

        public void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(data, prettyPrint: true));
            }
            catch (Exception e)
            {
                Debug.LogError($"[JokerGO] Could not write save file: {e.Message}");
            }
        }
    }
}

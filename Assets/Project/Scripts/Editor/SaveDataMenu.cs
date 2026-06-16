using System;
using System.IO;
using JokerGO.Game.Project.Scripts.Game.Data;
using UnityEditor;
using UnityEngine;

namespace JokerGO.Editor.Project.Scripts.Editor
{
    /// <summary>Editor utility: deletes the persisted save file for a fresh start.</summary>
    public static class SaveDataMenu
    {
        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, FileSaveRepository.FileName);

        [MenuItem("JokerGO/Clear Save Data")]
        public static void ClearSaveData()
        {
            string path = SavePath;

            if (!File.Exists(path))
            {
                Debug.Log($"[JokerGO] No save file to clear at {path}.");
                return;
            }

            if (!EditorUtility.DisplayDialog("Clear Save Data",
                    $"Delete the save file?\n\n{path}", "Delete", "Cancel"))
                return;

            try
            {
                File.Delete(path);
                Debug.Log($"[JokerGO] Save data cleared ({path}).");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JokerGO] Could not delete save file: {e.Message}");
            }
        }
    }
}

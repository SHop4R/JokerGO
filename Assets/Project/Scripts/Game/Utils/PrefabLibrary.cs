using System;
using UnityEngine;

namespace JokerGO.Game.Utils
{
    /// <summary>Loads generated prefabs from Resources with a clear error when they are missing.</summary>
    public static class PrefabLibrary
    {
        public static GameObject Load(string prefabName)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Prefab '{prefabName}' is missing; run JokerGO > Generate Art Assets in the editor.");
            }

            return prefab;
        }

        public static T LoadComponent<T>(string prefabName) where T : Component
        {
            var component = Load(prefabName).GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Prefab '{prefabName}' has no {typeof(T).Name} component; regenerate art assets.");
            }

            return component;
        }
    }
}

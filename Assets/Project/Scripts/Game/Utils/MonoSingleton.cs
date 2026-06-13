using UnityEngine;

namespace JokerGO.Game.Utils
{
    /// <summary>
    /// An abstract class that provides a singleton pattern for <see cref="MonoBehaviour"/> derived classes.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        /// <summary>
        /// Returns the cached instance, otherwise finds one in the scene (including inactive
        /// objects) and caches it, creating an empty host only as a last resort.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                if (!_instance)
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();

                return _instance;
            }
        }
    }
}

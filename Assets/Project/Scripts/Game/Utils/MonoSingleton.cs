using UnityEngine;

namespace JokerGO.Game.Utils
{
    /// <summary>
    /// An abstract class that provides a singleton pattern for <see cref="MonoBehaviour"/> derived classes.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static volatile T _instance;

        /// <summary>
        /// Gets the singleton instance of the class. If the instance is not already created, it finds or creates it.
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

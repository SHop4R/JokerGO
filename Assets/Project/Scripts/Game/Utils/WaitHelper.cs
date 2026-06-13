using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game.Utils
{
    /// <summary>
    /// Caches <see cref="UnityEngine.WaitForSeconds"/> instances so repeated waits of
    /// the same duration do not allocate a new yield object each time.
    /// </summary>
    public static class WaitHelper
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitCache = new();

        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!WaitCache.TryGetValue(seconds, out WaitForSeconds wait))
            {
                wait = new WaitForSeconds(seconds);
                WaitCache.Add(seconds, wait);
            }

            return wait;
        }
    }
}

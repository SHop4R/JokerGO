using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game.Utils
{
    /// <summary>
    /// Provides cached yield instructions so coroutines do not allocate a new
    /// <see cref="UnityEngine.WaitForSeconds"/> on every wait.
    /// </summary>
    public static class WaitHelper
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new();
        private static WaitForEndOfFrame _waitForEndOfFrame;
        private static WaitForFixedUpdate _waitForFixedUpdate;

        public static WaitForEndOfFrame WaitForEndOfFrame => _waitForEndOfFrame ??= new();

        public static WaitForFixedUpdate WaitForFixedUpdate => _waitForFixedUpdate ??= new();

        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!WaitDictionary.ContainsKey(seconds))
                WaitDictionary.Add(seconds, new(seconds));

            return WaitDictionary[seconds];
        }
    }
}

using System;

namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>
    /// Serializable snapshot of persistent progress. Field names define the save JSON keys.
    /// Fields are public and mutable because JsonUtility requires it; treat instances as
    /// write-once transfer objects, never as live state.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int apples;
        public int pears;
        public int strawberries;
        public int currentTileIndex;
    }
}

using System;

namespace JokerGO.Core
{
    /// <summary>Serializable snapshot of persistent progress. Field names define the save JSON keys.</summary>
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

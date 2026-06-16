namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Persistence boundary. Implementations decide where and how SaveData is stored.</summary>
    public interface ISaveRepository
    {
        /// <summary>Returns the stored snapshot, or null when absent or unreadable.</summary>
        SaveData Load();

        void Save(SaveData data);
    }
}

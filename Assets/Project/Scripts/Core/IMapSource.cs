namespace JokerGO.Core.Project.Scripts.Core
{
    /// <summary>Map data boundary. Implementations decide where map data comes from.</summary>
    public interface IMapSource
    {
        /// <summary>Loads and validates the board. Throws <see cref="MapValidationException"/> on bad data.</summary>
        BoardMap Load();
    }
}

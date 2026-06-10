using System;

namespace JokerGO.Core
{
    /// <summary>Raised when map data fails validation; the message is safe to show on screen.</summary>
    public sealed class MapValidationException : Exception
    {
        public MapValidationException(string message) : base(message)
        {
        }
    }
}

namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents a player identifier.
    /// </summary>
    public interface IPlayerId
    {
        /// <summary>
        /// The <see cref="AuthType"/>-specific identifier of the player.
        /// </summary>
        public object Id { get; }

        /// <summary>
        /// The <see cref="XPSystem.API.Enums.AuthType"/> of the <see cref="IPlayerId"/>.
        /// </summary>
        public AuthType AuthType { get; }

        /// <summary>
        /// Returns Id@AuthType.
        /// </summary>
        public string ToString();
    }
}
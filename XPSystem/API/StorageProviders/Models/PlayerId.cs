namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents a player identifier.
    /// </summary>
    /// <typeparam name="T">The type of the identifier.</typeparam>
    public interface IPlayerId<T> : IPlayerId
    {
        /// <summary>
        /// The <see cref="AuthType"/>-specific identifier of the player.
        /// </summary>
        public T Id { get; }
    }

    /// <summary>
    /// Non-generic <see cref="IPlayerId{T}"/>>.
    /// </summary>
    public interface IPlayerId
    {
        /// <summary>
        /// The <see cref="XPSystem.API.Enums.AuthType"/> of the <see cref="IPlayerId{T}"/>.
        /// </summary>
        public AuthType AuthType { get; }

        /// <summary>
        /// Returns Id@AuthType.
        /// </summary>
        public string ToString();
    }
}
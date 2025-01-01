namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses numbers (ex. SteamIds).
    /// </summary>
    public class NumberPlayerId : IPlayerId
    {
        /// <summary>
        /// The identifier of the player, in number form.
        /// </summary>
        public readonly ulong IdNumber;

        /// <inheritdoc />
        public object Id => IdNumber;
        /// <inheritdoc />
        public AuthType AuthType { get; }

        /// <inheritdoc cref="IPlayerId.ToString" />
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";

        public NumberPlayerId(ulong id, AuthType authType)
        {
            IdNumber = id;
            AuthType = authType;
        }
    }
}
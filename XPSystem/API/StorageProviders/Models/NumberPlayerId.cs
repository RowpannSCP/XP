namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses numbers (ex. SteamIds).
    /// </summary>
    public class NumberPlayerId : IPlayerId<ulong>
    {
        /// <inheritdoc />
        public ulong Id { get; }
        /// <inheritdoc />
        public AuthType AuthType { get; }

        /// <inheritdoc cref="IPlayerId{T}.ToString" />
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";

        public NumberPlayerId(ulong id, AuthType authType)
        {
            AuthType = authType;
            Id = id;
        }
    }
}
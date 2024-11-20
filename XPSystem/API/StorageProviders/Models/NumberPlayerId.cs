namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses numbers (ex. SteamIds).
    /// </summary>
    public readonly struct NumberPlayerId : IPlayerId<ulong>, IPlayerId<object>
    {
        /// <inheritdoc />
        public ulong Id { get; }

        /// <inheritdoc />
        object IPlayerId<object>.Id => Id;

        /// <inheritdoc cref="IPlayerId{T}.AuthType" />
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
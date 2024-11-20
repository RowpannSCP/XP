namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses strings (ex. NorthwoodIds).
    /// </summary>
    public readonly struct StringPlayerId : IPlayerId<string>, IPlayerId<object>
    {
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        object IPlayerId<object>.Id => Id;

        /// <inheritdoc cref="IPlayerId{T}.AuthType" />
        public AuthType AuthType { get; }

        /// <inheritdoc cref="IPlayerId{T}.ToString" />
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";

        public StringPlayerId(string id, AuthType authType)
        {
            Id = id;
            AuthType = authType;
        }
    }
}
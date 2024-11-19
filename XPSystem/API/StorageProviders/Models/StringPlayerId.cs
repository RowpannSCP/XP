namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses strings (ex. NorthwoodIds).
    /// </summary>
    public class StringPlayerId : IPlayerId<string>
    {
        /// <inheritdoc />
        public string Id { get; }
        /// <inheritdoc />
        public AuthType AuthType { get; }

        /// <inheritdoc cref="IPlayerId.ToString" />
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";

        public StringPlayerId(string id, AuthType authType)
        {
            Id = id;
            AuthType = authType;
        }
    }
}
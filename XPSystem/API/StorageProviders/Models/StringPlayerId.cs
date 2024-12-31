namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    /// <summary>
    /// Represents an identifier that uses strings (ex. NorthwoodIds).
    /// </summary>
    public class StringPlayerId : IPlayerId
    {
        /// <summary>
        /// The identifier of the player, in string form.
        /// </summary>
        public readonly string IdString;

        /// <inheritdoc />
        public object Id => IdString;
        /// <inheritdoc />
        public AuthType AuthType { get; }

        /// <inheritdoc cref="IPlayerId.ToString" />
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";

        public StringPlayerId(string id, AuthType authType)
        {
            IdString = id;
            AuthType = authType;
        }
    }
}
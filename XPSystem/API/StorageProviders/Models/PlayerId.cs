namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;

    public struct PlayerId
    {
        public AuthType AuthType { get; set; }
        public ulong Id { get; set; }

        /// <summary>
        /// Returns Id@AuthType.
        /// </summary>
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";
    }
}
namespace XPSystem.API.StorageProviders.Models
{
    using XPSystem.API.Enums;
    using XPSystem.API.Exceptions;

    public struct PlayerId
    {
        public AuthType AuthType { get; set; }
        public ulong Id { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="PlayerId"/> is valid.
        /// AddXP will throw if this is true, unless forced.
        /// </summary>
        public bool IsValid => Id != 0;

        /// <summary>
        /// Throws an <see cref="InvalidPlayerIdException"/> if this <see cref="PlayerId"/> is not valid.
        /// </summary>
        /// <exception cref="InvalidPlayerIdException">This <see cref="PlayerId"/> is not valid.</exception>
        public void EnsureValid()
        {
            if (!IsValid)
                throw new InvalidPlayerIdException();
        }

        /// <summary>
        /// Returns Id@AuthType.
        /// </summary>
        public override string ToString() => $"{Id}@{AuthType.ToString().ToLower()}";
    }
}
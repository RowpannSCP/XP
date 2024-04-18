namespace XPSystem.Config.Events.Types
{
    using System;

    /// <summary>
    /// Represents a XP Event Config file type with a generic argument.
    /// </summary>
    public interface IGenericXPECFile
    {
        /// <summary>
        /// The type of the generic argument.
        /// </summary>
        public Type GenericType { get; }
    }
}
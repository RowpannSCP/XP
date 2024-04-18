namespace XPSystem.Config.Events.Types
{
    /// <summary>
    /// Represents a XP Event Config file.
    /// </summary>
    public abstract class XPECFile
    {
        /// <summary>
        /// Gets an item with the specified keys.
        /// </summary>
        /// <param name="keys">The keys of the item.</param>
        /// <returns>The item.</returns>
        public abstract XPECItem Get(params object[] keys);

        /// <summary>
        /// Checks if the specified object is of the same type as this file.
        /// Used by the loader for validation.
        /// </summary>
        /// <param name="obj">The object whose type check.</param>
        /// <returns>Whether the object is of the same type as this file.</returns>
        public virtual bool IsEqualType(object obj) => obj.GetType() == GetType();
    }
}
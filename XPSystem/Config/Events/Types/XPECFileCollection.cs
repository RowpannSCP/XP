namespace XPSystem.Config.Events.Types
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a collection of <see cref="XPECFile"/>.
    /// </summary>
    public class XPECFileCollection : IEnumerable<KeyValuePair<string, XPECFile>>
    {
        /// <summary>
        /// The files in the collection.
        /// </summary>
        public readonly IDictionary<string, XPECFile> Files = new Dictionary<string, XPECFile>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets a <see cref="XPECItem"/> with the specified key and subkeys.
        /// </summary>
        /// <param name="key">The key of the file.</param>
        /// <param name="subkeys">The (optional) subkeys of the item.</param>
        /// <returns>The item, if found, otherwise null.</returns>
        public XPECItem Get(string key, params object[] subkeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return Files.TryGetValue(key, out var file)
                ? file.Get(subkeys)
                : null;
        }

        /// <summary>
        /// Returns a new collection with the same files, but made readonly.
        /// </summary>
        /// <returns>The new collection.</returns>
        public XPECFileCollection AsReadonly()
        {
            return new XPECFileCollection(new ReadOnlyDictionary<string, XPECFile>(Files));
        }

        public XPECFileCollection() {}
        private XPECFileCollection(IDictionary<string, XPECFile> files)
        {
            Files = files;
        }

        /// <summary>
        /// Gets or sets a file with the specified key.
        /// </summary>
        /// <param name="key">The key of the file.</param>
        public XPECFile this[string key]
        {
            get => Files[key];
            set => Files[key] = value;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, XPECFile>> GetEnumerator() => Files.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
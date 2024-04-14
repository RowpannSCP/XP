namespace XPSystem.Config.Events.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of XPEC files.
    /// </summary>
    public class XPECFileCollection
    {
        public Dictionary<string, IXPECFile> Files { get; set; } = new (StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets a XP event config item.
        /// </summary>
        /// <param name="key">The key of the event config file.</param>
        /// <param name="subkey">The subkey of the event config.</param>
        /// <typeparam name="T">The type of the subkey.</typeparam>
        /// <returns>The XP event config item, if found, otherwise default value or null.</returns>
        /// <exception cref="InvalidOperationException">The subkey types don't match.</exception>
        public static XPECItem GetXPEC<T>(string key, T subkey)
        {
            key = key.ToLower();

            foreach (var file in Files)
            {
                if (file.Key == key)
                {
                    if (subkey == null)
                        return file.Default;

                    var fileSubkeyType = file.GetSubkeyType();
                    if (fileSubkeyType != typeof(T))
                        throw new InvalidOperationException($"Wrong subkey type for key {key} (was: {typeof(T).FullName}, expected: {fileSubkeyType.FullName})!");

                    return file.GetItem(subkey);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the default XP event config item of the XPEC file with the specified key.
        /// </summary>
        /// <param name="key">The key of the event config file.</param>
        /// <returns>The default XP event config item of the file, if found, otherwise null.</returns>
        public static XPECItem GetXPEC(string key)
        {
            return GetXPEC<object>(key, null);
        }

        /// <summary>
        /// Gets the XPEC file with the specified key.
        /// </summary>
        /// <param name="key">The key of the event config file.</param>
        /// <returns>The XPEC file, if found, otherwise null.</returns>
        public static IXPECFile GetXPECFile(string key)
        {
            return Files.FirstOrDefault(x => x.Key == key);
        }
    }
}
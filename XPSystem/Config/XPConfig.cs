namespace XPSystem.Config
{
    using System.Collections;
    using System.Collections.Generic;
    using static XPSystem.API.LoaderSpecific;

    /// <summary>
    /// <see cref="XPConfigCollection{T}"/> just without generic parameters.
    /// </summary>
    public interface IXPConfigCollection
    {
        public bool CheckType(object key);
        public bool CheckType(IXPConfigCollection collection);
        public bool TryGetConfig<T>(T key, out XPConfigElement config);
        public void SetValues(IXPConfigCollection collection);
    }

    /// <summary>
    /// <see cref="XPConfig{T}"/> just without generic parameters.
    /// </summary>
    public interface IXPConfig
    {
        public XPConfigElement DefaultValue { get; set; }
        public IDictionary IDictionaryValues { get; }
    }

    /// <summary>
    /// Represents a config collection.
    /// Supports default and override default values, and default and override key values.
    /// </summary>
    /// <typeparam name="T">The key type.</typeparam>
    public class XPConfigCollection<T> : IXPConfigCollection
    {
        /// <summary>
        /// The defaults provided by the plugin.
        /// Avoid touching.
        /// </summary>
        public XPConfig<T> Default { get; init; } = new();

        /// <summary>
        /// The values read from the config.
        /// </summary>
        public XPConfig<T> Values { get; set; } = new();

        public XPConfigCollection(){}

        public XPConfigCollection(int defaultAmount)
        {
            Default.DefaultValue.Amount = defaultAmount;
        }

        public XPConfigCollection(string defaultTranslation)
        {
            Default.DefaultValue.Translation = defaultTranslation;
        }

        public XPConfigCollection(int defaultAmount, string defaultTranslation)
        {
            Default.DefaultValue.Amount = defaultAmount;
            Default.DefaultValue.Translation = defaultTranslation;
        }

        /// <summary>
        /// Checks if the key is of the correct type.
        /// </summary>
        public bool CheckType(object key)
        {
            return key is T;
        }

        /// <summary>
        /// Checks if the specified collection is of the same type.
        /// </summary>
        public bool CheckType(IXPConfigCollection collection)
        {
            return collection is XPConfigCollection<T>;
        }

        /// <summary>
        /// Attempts to get a config from the collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="config">The returned config, or <c>null</c> if it wasn't found..</param>
        /// <returns>Whether the config was found.</returns>
        public bool TryGetConfig<T1>(T1 key, out XPConfigElement config)
        {
            config = null;
            if (key is not T keyT)
            {
                LogWarn($"Wrong key type used for TryGetString (was: {typeof(T1).FullName}, expected: {typeof(T).FullName})!");
                return false;
            }

            if (Values.Values.TryGetValue(keyT, out config))
                return true;

            if (Values.DefaultValue != null)
            {
                config = Values.DefaultValue;
                return true;
            }

            if (Default.Values.TryGetValue(keyT, out config))
                return true;

            config = Default.DefaultValue;
            return true;
        }

        /// <summary>
        /// Sets the non-default values of the collection.
        /// Used by the loader.
        /// </summary>
        /// <param name="collection">The collection to get the values from.</param>
        public void SetValues(IXPConfigCollection collection)
        {
            if (collection is not XPConfigCollection<T> xpConfigCollection)
            {
                LogError($"Wrong type used for SetValues (was: {collection.GetType().FullName}, expected: {typeof(XPConfigCollection<T>).FullName})!");
                return;
            }

            Values = xpConfigCollection.Values;
        }
    }

    /// <summary>
    /// Represents a config collection element.
    /// Supports a default value and key values.
    /// </summary>
    /// <typeparam name="T">The key type.</typeparam>
    public class XPConfig<T> : IXPConfig
    {
        /// <summary>
        /// The fallback value.
        /// </summary>
        public XPConfigElement DefaultValue { get; set; } = new();

        /// <summary>
        /// The values.
        /// </summary>
        public Dictionary<T, XPConfigElement> Values { get; set; } = new();

        /// <summary>
        /// Just here for the interface.
        /// </summary>
        public IDictionary IDictionaryValues => Values;
    }

    /// <summary>
    /// Represents a config.
    /// Fancy tuple lol.
    /// </summary>
    public class XPConfigElement
    {
        public int Amount { get; set; }
        public string Translation { get; set; }
    }
}
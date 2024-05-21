namespace XPSystem.Config.Events.Types
{
    using System;
    using System.Collections.Generic;
    using XPSystem.API;

    /// <summary>
    /// Represents a XP Event Config file with a dictionary of items.
    /// </summary>
    /// <typeparam name="T">The type of the subkeys.</typeparam>
    public class XPECDictFile<T> : XPECFile
    {
        public XPECItem Default { get; set; }

        public Dictionary<T, XPECItem> Items { get; set; } = new();

        /// <inheritdoc />
        public override XPECItem Get(params object[] keys)
        {
            if (keys == null || keys.Length == 0)
                return Default;

            var keyObj = keys[0];
            if (keyObj is not T key)
                throw new InvalidCastException($"Key is not of the correct type (was: {keyObj.GetType().FormatType()}, expected: {typeof(T).FormatType()})");

            if (Items == null)
            {
                XPAPI.LogDebug("Items null in XPECDictFile with keytype " + typeof(T).FormatType());
                return Default;
            }

            return Items.TryGetValue(key, out var item)
                ? item
                : Default;
        }

        /// <inheritdoc />
        public override Type[][] ParametersTypes { get; } = { new[] { typeof(T) } };

        /// <inheritdoc />
        public override bool IsEqualType(object obj) => obj is XPECDictFile<T>;
    }
}
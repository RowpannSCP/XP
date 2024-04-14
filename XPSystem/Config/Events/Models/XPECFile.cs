namespace XPSystem.Config.Events.Models
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    /// <summary>
    /// <see cref="XPECFile{T}"/>, just not generic.
    /// </summary>
    public interface IXPECFile
    {
        public string Key { get; set; }
        public XPECItem Default { get; set; }
        public Type GetSubkeyType();
        public XPECItem GetItem(object subkey);
    }

    /// <summary>
    /// Represents a translation file for a key.
    /// </summary>
    /// <typeparam name="T">The type of the subkeys.</typeparam>
    public class XPECFile<T> : IXPECFile
    {
        [YamlIgnore]
        public string Key { get; set; }
        public XPECItem Default { get; set; }

        public Dictionary<T, XPECItem> Items { get; set; } = new();

        public Type GetSubkeyType() => typeof(T);
        public XPECItem GetItem(object subkey) => Items.TryGetValue((T)subkey, out var item) ? item : Default;
    }
}
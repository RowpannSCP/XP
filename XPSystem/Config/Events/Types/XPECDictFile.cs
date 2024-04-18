namespace XPSystem.Config.Events.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a translation file for a key.
    /// </summary>
    /// <typeparam name="T">The type of the subkeys.</typeparam>
    public class XPECDictFile<T> : XPECFile, IGenericXPECFile
    {
        public XPECItem Default { get; set; }

        public Dictionary<T, XPECItem> Items { get; set; } = new();

        public override XPECItem Get(params object[] keys)
        {
            throw new NotImplementedException();
        }

        public Type GenericType => typeof(T);
        public override bool IsEqualType(object obj) => obj is XPECDictFile<T>;
    }
}
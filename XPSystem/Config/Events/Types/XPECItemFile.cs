namespace XPSystem.Config.Events.Types
{
    /// <summary>
    /// Represents a XP Event Config file with a single item.
    /// </summary>
    public class XPECItemFile : XPECFile
    {
        public XPECItem Item { get; set; } = null!;

        /// <inheritdoc />
        public override XPECItem Get(params object?[]? keys)
        {
            base.Get(keys);
            return Item;
        }
    }
}
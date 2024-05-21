﻿namespace XPSystem.Config.Events.Types
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a XP Event Config Item.
    /// </summary>
    public class XPECItem
    {
        [Description("The amount of XP to give. Can be negative.")]
        public int Amount { get; set; }

        [Description("The message to show. Leave empty for none.")]
        public string Translation { get; set; }
    }
}
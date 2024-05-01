namespace XPSystem.Config
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using XPSystem.API.Enums;

    public abstract class Config
    {
        [Description("Print debug messages?")]
        public bool Debug { get; set; } = false;

        [Description("The amount of XP required for each level.")]
        public uint XPPerLevel { get; set; } = 100;

        [Description("The amount of XP required for each level in addition to XPPerLevel, starting at the specified level." +
                     "Example: 10: 100, starting from level 10 to the next entry, XPPerLevel + 100XP will be required for each level.")]
        public Dictionary<uint, uint> XPPerLevelExtra { get; set; } = new()
        {
            [10] = 100,
        };

        [Description("Path to the event configs folder, relative to the ExtendedConfigPath.")]
        public string EventConfigsFolder { get; set; } = "XPEventConfigs";

        [Description("Whether or not to enable the .xp get (client) command")]
        public bool EnableGetXPCommand { get; set; } = true;

        [Description("Whether or not to enable the .xp leaderboard (client) command")]
        public bool EnableLeaderboardCommand { get; set; } = true;

        [Description("The maximum amount of entries that may be retrieved with the client leaderboard command.")]
        public byte LeaderboardMaxLength { get; set; } = 10;

        [Description("The message to show to players who have DNT enabled.")]
        public string DNTMessage { get; set; } =
            "We can't track your stats while you have DNT enabled in your game options!";

        [Description("Whether or not to format a message according to a template when adding xp.")]
        public bool UseAddedXPTemplate { get; set; } = true;

        [Description("When enabled, template used for messages when adding xp. Parameters: %message%, %currentxp%, %currentlevel%, %neededxp%, %nextlevel.")]
        public string AddedXPTemplate { get; set; } = "%message%, (%currentxp% / %neededxp%)";

        [Description("Whether or not to show a message to a player if they advance a level.")]
        public bool ShowAddedLVL { get; set; } = true;

        [Description("When enabled, what message to show if player advances a level.")]
        public string AddedLVLMessage { get; set; } = "NEW LEVEL: <color=red>%level%</color>";

        [Description("Decide how messages (ex. xp gain, level up) are displayed.")]
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Hint;

        [Description("The duration of the message, if applicable.")]
        public float DisplayDuration { get; set; } = 5f;

        [Description("Appended to all messages.")]
        public string TextPrefix { get; set; } = "";

        [Description("Prepended to all messages.")]
        public string TextSuffix { get; set; } = "";

        [Description("The assembly qualified type name of the storage provider to use (default: XPSystem.LiteDBProvider.LiteDBProvider) (not assembly qualified because it's built-in, unlike yours).")]
        public string StorageProvider { get; set; } = "XPSystem.LiteDBProvider.LiteDBProvider";

        [Description("The assembly qualified type names of xp display providers to load, in addition to the built-in rank and nick ones.")]
        public List<string> AdditionalDisplayProviders { get; set; } = new()
        {
        };

        [Description("Prints used keys to console at end of round. Not recommended to keep on - performance.")]
        public bool LogXPGainedMethods { get; set; } = false;
        
        public abstract string ExtendedConfigPath { get; set; }
        public abstract string LegacyDefaultDatabasePath { get; set; }
    }
}
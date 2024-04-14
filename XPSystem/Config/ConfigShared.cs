namespace XPSystem.Config
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using XPSystem.API.Enums;
    using static API.LoaderSpecific;

    public class ConfigShared
    {
        [Description("Print debug messages?")]
        public bool Debug { get; set; } = false;

        [Description("Path to folder with extended configs.")]
        public string ExtendedConfigPath { get; set; } =
            Path.Combine(ConfigPath, "XPSystem");

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

        [Description("Decide how messages (ex. xp gain, level up) are displayed.")]
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Hint;

        [Description("The duration of the message, if applicable.")]
        public float DisplayDuration { get; set; } = 5f;

        [Description("Appended to all messages.")]
        public string TextPrefix { get; set; } = "";

        [Description("Prepended to all messages.")]
        public string TextSuffix { get; set; } = "";
    }
}
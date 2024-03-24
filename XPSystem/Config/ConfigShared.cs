namespace XPSystem.Config
{
    using System.ComponentModel;
    using System.IO;
    using XPSystem.API;
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
        public int XPPerLevel { get; set; } = 100;

        [Description("Path to the XP configs file, relative to the ExtendedConfigPath.")]
        public string XPConfigsFile { get; set; } = "XPConfigs.yml";

        [Description("Path to the database parameters file, relative to the ExtendedConfigPath.")]
        public string DatabaseParametersFile { get; set; } = "Database.yml";

        [Description("Enable nickname modifications (see below)?")]
        public bool EnableNicks { get; set; } = true;

        [Description("Enable badge modifications (see below)?")]
        public bool EnableBadges { get; set; } = true;

        [Description("Required for VSR compliance.")]
        public bool SkipGlobalBadges { get; set; } = true;

        [Description("Whether or not to change how badge hiding works")]
        public bool EditBadgeHiding { get; set; } = true;
   
        [Description("Whether or not to write the config after read and the plugin has changed it (missing/broken entries).")]
        public bool WriteConfig { get; set; } = true;

        [Description("Whether or not to show badges to players depending whether or not they have the view hidden badges permission")]
        public bool ShowHiddenBadgesToAdmins { get; set; } = true;

        [Description("Whether or not to enable the .xp get (client) command")]
        public bool EnableGetXPCommand { get; set; } = true;

        [Description("Whether or not to enable the .xp leaderboard (client) command")]
        public bool EnableLeaderboardCommand { get; set; } = true;

        [Description("The maximum amount of entries that may be retrieved with the client leaderboard command.")]
        public int LeaderboardMaxLength { get; set; } = 10;

        [Description("Decide how messages (ex. xp gain, level up) are displayed.")]
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Hint;

        [Description("The default text to display when a player gains XP (should only be used by external plugins).")]
        public string DefaultXPAddedText { get; set; } = "You have gained {0} XP!";
    }
}
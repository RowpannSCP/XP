namespace XPSystem.Config
{
    using System.ComponentModel;
    using System.IO;
    using static API.LoaderSpecific;

    /// <summary>
    /// Config class shared by configs made for in-game usage.
    /// </summary>
    public abstract class ConfigShared : Config
    {
        [Description("Path to folder with extended configs.")]
        public override string ExtendedConfigPath { get; set; } =
            Path.Combine(ConfigPath, "XPSystem");

        [Description("Path to the legacy database file.")]
        public override string LegacyDefaultDatabasePath { get; set; } =
            Path.Combine(
#if EXILED
                Exiled.API.Features.Paths.Configs, 
#else
                Path.Combine(PluginAPI.Helpers.Paths.LocalPlugins.Plugins, "XPSystem"),
#endif
                "Players.db");
    }
}
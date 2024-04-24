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
    }
}
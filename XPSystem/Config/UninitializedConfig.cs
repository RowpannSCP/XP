namespace XPSystem.Config
{
    /// <summary>
    /// Config used to avoid issues when running from console.
    /// Should not be used when running on SL.
    /// </summary>
    public class UninitializedConfig : Config
    {
        public override string ExtendedConfigPath { get; set; } = "extendedconfigs";
    }
}
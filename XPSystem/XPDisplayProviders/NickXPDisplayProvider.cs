namespace XPSystem.XPDisplayProviders
{
    using System.ComponentModel;
    using XPSystem.API;

    public class NickXPDisplayProvider : XPDisplayProvider<NickXPDisplayProvider.NickConfig>
    {
        public override void Enable()
        {
            throw new System.NotImplementedException();
        }

        #error
        public override void Disable()
        {
            throw new System.NotImplementedException();
        }

        public override void Refresh(XPPlayer player)
        {
            throw new System.NotImplementedException();
        }

        public override void RefreshAll()
        {
            throw new System.NotImplementedException();
        }

        public class NickConfig : IXPDisplayProviderConfig
        {
            [Description("Enable nick modifications?")]
            public bool Enabled { get; set; } = true;

            [Description("The structure of the player nick. Variables: %lvl% - the level. %name% - the players nickname/name")]
            public string NickStructure { get; set; } = "LVL %lvl% | %name%";
        }
    }
}
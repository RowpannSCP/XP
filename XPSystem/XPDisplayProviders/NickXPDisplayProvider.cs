namespace XPSystem.XPDisplayProviders
{
    using System.ComponentModel;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;

    public class NickXPDisplayProvider : XPDisplayProvider<NickXPDisplayProvider.NickConfig>
    {
        public override void Refresh(XPPlayer player, PlayerInfoWrapper playerInfo)
        {
            if (!Config.Enabled)
            {
                player.ResyncSyncVar(typeof(NicknameSync), nameof(NicknameSync.Network_displayName));
                return;
            }

            string nick = Config.NickStructure
                .Replace("%lvl%", playerInfo.Level.ToString())
                .Replace("%name%", player.Nickname);

            player.SetNick(nick, true);
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
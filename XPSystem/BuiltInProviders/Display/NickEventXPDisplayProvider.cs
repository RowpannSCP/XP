namespace XPSystem.BuiltInProviders.Display
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using LabApi.Events.Arguments.PlayerEvents;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;

    public class NickEventXPDisplayProvider : XPDisplayProvider<NickEventXPDisplayProvider.NickConfig>
    {
        private Dictionary<string, string> DisplayNameOverrides { get; } = new();

        public override void Enable()
        {
            base.Enable();
            LabApi.Events.Handlers.PlayerEvents.ChangingNickname += OnChangingNickname;
        }

        public override void Disable()
        {
            base.Disable();
            LabApi.Events.Handlers.PlayerEvents.ChangingNickname -= OnChangingNickname;
        }

        protected override void RefreshOfEnabled(BaseXPPlayer player, PlayerInfoWrapper? playerInfo) => Refresh(player);
        protected override void RefreshOfDisabled(BaseXPPlayer player) => Refresh(player);
        private void Refresh(BaseXPPlayer player)
        {
            if (player is not XPPlayer)
                return;

            player.Hub.nicknameSync.DisplayName = player.Nickname;
        }

        private void OnChangingNickname(PlayerChangingNicknameEventArgs ev)
        {
            if (!Config.Enabled)
                return;

            if (!XPPlayer.TryGetXP(ev.Player.ReferenceHub, out XPPlayer? player))
                return;

            string name = Config.UseEvNewNick ? (ev.NewNickname ?? player.Nickname) : player.Nickname;
            ev.NewNickname = Config.NickStructure
                .Replace("%lvl%", XPAPI.GetPlayerInfo(player).Level.ToString())
                .Replace("%name%", name);
        }

        public class NickConfig : IXPDisplayProviderConfig
        {
            [Description("Enable nick modifications?")]
            public bool Enabled { get; set; } = true;

            [Description("Use the NewNickname of the EventArgs instead of MyNick for %name%.")]
            public bool UseEvNewNick { get; set; } = true;

            [Description("The structure of the player nick. Variables: %lvl% - the level. %name% - the players nickname/name")]
            public string NickStructure { get; set; } = "LVL %lvl% | %name%";
        }
    }
}
namespace XPSystem.BuiltInProviders.Display
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using LabApi.Events.Arguments.PlayerEvents;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;

    public class NickEventXPDisplayProvider : XPDisplayProvider<NickEventXPDisplayProvider.NickConfig>
    {
        private Dictionary<int, string> DisplayNameOverrides { get; } = new();

        public override void Enable()
        {
            base.Enable();
            LabApi.Events.Handlers.PlayerEvents.ChangingNickname += OnChangingNickname;
            LabApi.Events.Handlers.PlayerEvents.Joined += OnPlayerJoined;
            LabApi.Events.Handlers.PlayerEvents.Left += OnPlayerLeft;
        }

        public override void Disable()
        {
            base.Disable();
            LabApi.Events.Handlers.PlayerEvents.ChangingNickname -= OnChangingNickname;
            LabApi.Events.Handlers.PlayerEvents.Joined -= OnPlayerJoined;
            LabApi.Events.Handlers.PlayerEvents.Left -= OnPlayerLeft;
        }

        protected override void RefreshOfEnabled(BaseXPPlayer player, PlayerInfoWrapper? playerInfo) => Refresh(player);
        protected override void RefreshOfDisabled(BaseXPPlayer player) => Refresh(player);
        private void Refresh(BaseXPPlayer player)
        {
            if (player is not XPPlayer)
                return;

            UpdateNick(player);
        }

        private void UpdateNick(BaseXPPlayer player, string? customNick = null)
        {
            if (!player.IsReady)
                return;
            if (CheckRecursion())
                return;
            _updateNickUnsafe(player, customNick);
        }
        private void _updateNickUnsafe(BaseXPPlayer player, string? customNick)
        {
            if (!XPPlayer.TryGetXP(player, out XPPlayer? xpPlayer))
                return;

            string name;
            if (Config.UseEvNewNick)
            {
                if (!string.IsNullOrEmpty(customNick))
                {
                    name = customNick!;
                    DisplayNameOverrides[player.PlayerServerId] = customNick!;
                }
                else
                {
                    name = DisplayNameOverrides.TryGetValue(player.PlayerServerId, out string cached)
                        ? cached
                        : xpPlayer.Nickname;
                }
            }
            else
            {
                name = xpPlayer.Nickname;
            }

            player.Hub.nicknameSync.DisplayName = Config.NickStructure
                .Replace("%lvl%", XPAPI.GetPlayerInfo(xpPlayer).Level.ToString())
                .Replace("%name%", name);
        }

        private MethodBase? _updateMethod;
        private bool CheckRecursion()
        {
            if  (_updateMethod == null)
                _updateMethod = typeof(NickEventXPDisplayProvider).GetMethod(nameof(_updateNickUnsafe), BindingFlags.NonPublic | BindingFlags.Instance);

            StackTrace stackTrace = new StackTrace();
            foreach (StackFrame frame in stackTrace.GetFrames() ?? Array.Empty<StackFrame>())
            {
                if (frame.GetMethod() == _updateMethod)
                    return true;
            }

            return false;
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            UpdateNick(new BaseXPPlayer(ev.Player.ReferenceHub));
        }

        private void OnChangingNickname(PlayerChangingNicknameEventArgs ev)
        {
            UpdateNick(new BaseXPPlayer(ev.Player.ReferenceHub), ev.NewNickname);
        }

        private void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            DisplayNameOverrides.Remove(ev.Player.PlayerId);
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
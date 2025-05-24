namespace XPSystem.BuiltInProviders.Display.Patch
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;

    public class NickPatchXPDisplayProvider : XPDisplayProvider<NickPatchXPDisplayProvider.NickConfig>
    {
        public Dictionary<string, string> DisplayNameOverrides { get; } = new();

        protected override void RefreshOfEnabled(XPPlayer player, PlayerInfoWrapper playerInfo) => Refresh(player);
        protected override void RefreshOfDisabled(XPPlayer player) => Refresh(player);

        private void Refresh(XPPlayer player)
        {
            player.Hub.nicknameSync.DisplayName = DisplayNameOverrides.TryGetValue(player.UserId, out string cached)
                ? cached
                : player.DisplayedName;
        }

        [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.DisplayName), MethodType.Setter)]
        internal static class DisplayNamePatch
        {
            public static void Prefix(NicknameSync __instance, ref string value)
            {
                if (!__instance._hub || __instance._hub.IsHost || __instance._hub.IsDummy)
                    return;

                foreach (IXPDisplayProvider provider in XPAPI.DisplayProviders)
                {
                    if (provider is NickPatchXPDisplayProvider nickProvider)
                    {
                        if (!nickProvider.Config.Enabled)
                            return;

                        nickProvider.DisplayNameOverrides[__instance._hub.authManager.UserId] = value;
                        value = nickProvider.Config.NickStructure
                            .Replace("%lvl%", XPAPI.GetPlayerInfo(__instance._hub).Level.ToString())
                            .Replace("%name%", value);

                        return;
                    }
                }
            }
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
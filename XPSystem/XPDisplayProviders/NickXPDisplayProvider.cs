namespace XPSystem.XPDisplayProviders
{
    using System;
    using System.ComponentModel;
    using HarmonyLib;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.StorageProviders;

    public class NickXPDisplayProvider : SyncVarXPDisplayProvider<NickXPDisplayProvider.NickConfig, string>
    {
        protected override string VariableKey { get; } = "NickXPDisplayProvider_nick";

        protected override (Type typeName, string methodName, Func<XPPlayer, string, object> getFakeSyncVar, Func<XPPlayer, object> getResyncVar)[] SyncVars { get; } =
        {
            (typeof(NicknameSync), nameof(NicknameSync.Network_displayName), (_, obj) => obj, player => player.DisplayedName)
        };

        protected override string CreateObject(XPPlayer player, PlayerInfoWrapper playerInfo)
        {
            return Config.NickStructure
                .Replace("%lvl%", playerInfo.Level.ToString())
                .Replace("%name%", player.DisplayedName);
        }

        [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.DisplayName), MethodType.Setter)]
        internal static class NickCommandPatch
        {
            public static void Prefix(NicknameSync __instance)
            {
                foreach (IXPDisplayProvider provider in XPAPI.DisplayProviders)
                {
                    if (provider is NickXPDisplayProvider nickProvider && nickProvider.Config.PatchNickCommand)
                    {
                        Timing.CallDelayed(.5f, () =>
                        {
                            nickProvider.RefreshOf(__instance._hub);
                        });

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

            [Description("Whether or not to patch the nick command to automatically update the nickname.")]
            public bool PatchNickCommand { get; set; } = true;
        }
    }
}
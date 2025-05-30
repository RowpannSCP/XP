namespace XPSystem.BuiltInProviders.Display.Patch
{
    using System.ComponentModel;
    using HarmonyLib;
    using XPSystem.API;
    using XPSystem.API.Player;

    public class MyNickPatchXPDisplayProvider : XPDisplayProvider<MyNickPatchXPDisplayProvider.NickConfig>
    {
        public override void RefreshAll() {}
        public override void RefreshTo(BaseXPPlayer player) {}

        [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.Network_myNickSync), MethodType.Setter)]
        internal static class NicknamePatch
        {
            public static void Prefix(NicknameSync __instance, ref string value)
            {
                if (!XPPlayer.TryGetXP(__instance._hub, out XPPlayer? player))
                    return;

                foreach (IXPDisplayProvider provider in XPAPI.DisplayProviders)
                {
                    if (provider is MyNickPatchXPDisplayProvider nickProvider)
                    {
                        if (!nickProvider.Config.Enabled)
                            return;

                        value = nickProvider.Config.NickStructure
                            .Replace("%lvl%", XPAPI.GetPlayerInfo(player).Level.ToString())
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
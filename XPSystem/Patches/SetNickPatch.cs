using HarmonyLib;
using XPSystem.API;
namespace XPSystem.Patches
{
    [HarmonyPatch(typeof(NicknameSync), "set_Network_displayName")]
    public class SetNickPatch
    {
        internal static void Postfix(NicknameSync __instance, string value)
        {
            if (!Main.EnabledNick)
                return;
            if (__instance._hub == null)
                return;
            var log = __instance._hub.GetLog();

            __instance._displayName = Main.Instance.Config.NickStructure
                .Replace("%lvl%", __instance._hub.authManager.DoNotTrack ? "DNT" : log.LVL.ToString())
                .Replace("%name%", value)
                .Replace("$IGNORE$", "");
        }
    }
}
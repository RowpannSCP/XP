using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using XPSystem.API;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.Patches
{
    [HarmonyPatch(typeof(NicknameSync), "set_Network_displayName")]
    public class SetNickPatch
    {
        internal static void Postfix(NicknameSync __instance, string value)
        {
            var ply = Player.Get(__instance._hub);
            var log = ply.GetLog();
            
            __instance._displayName = Main.Instance.Config.NickStructure
                .Replace("%lvl%", ply.DoNotTrack ? "DNT" : log.LVL.ToString())
                .Replace("%name%", value)
                .Replace("$IGNORE$", "");
        }
    }
}
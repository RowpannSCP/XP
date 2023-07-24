namespace XPSystem.Patches
{
    using System;
    using CommandSystem;
    using CommandSystem.Commands.RemoteAdmin;
    using HarmonyLib;
    using RemoteAdmin;
    using XPSystem.API;

    [HarmonyPatch(typeof(HideTagCommand), nameof(HideTagCommand.Execute))]
    public static class HideTagPatch
    {
        public static bool Prefix(ref bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;
            if (!Main.Instance.Config.EditBadgeHiding) return true;
            PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
            if (playerCommandSender == null)
            {
                response = "You must be in-game to use this command!";
                __result = false;
                return false;
            }
            ServerRoles serverRoles = playerCommandSender.ReferenceHub.serverRoles;
            if (!serverRoles.BypassStaff)
            {
                if (!string.IsNullOrEmpty(serverRoles.HiddenBadge))
                {
                    response = "Your badge is already hidden.";
                    __result = false;
                    return false;
                }
                if (string.IsNullOrEmpty(serverRoles.MyText))
                {
                    response = "Your don't have any badge.";
                    __result = false;
                    return false;
                }
            }

            serverRoles.GlobalHidden = serverRoles.GlobalSet;
            serverRoles.NetworkGlobalBadge = null;
            API.UpdateBadge(playerCommandSender.ReferenceHub, null, true);
            if (Main.Instance.Config.ShowHiddenBadgesToAdmins)
                serverRoles.RefreshHiddenTag();
            response = "Tag hidden!";
            __result = true;
            return false;
        }
    }
}
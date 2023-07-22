namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class RefreshNicks : ICommand
    {
        public string Command { get; } =  "refresh";
        public string[] Aliases { get; } =  new string[] { "setnicks" };
        public string Description { get; } =  "Refresh every players nicknames to match their level (used after reloading).";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.refresh"))
            {
                response = "You don't have permission (xps.refresh) to use this command.";
                return false;
            }

            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub == ReferenceHub.HostHub)
                    continue;
                hub.nicknameSync.DisplayName = hub.nicknameSync.Network_myNickSync;
            }

            response = "Nicks refreshed.";
            return true;
        }
    }
}
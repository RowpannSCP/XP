namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class Refresh : ICommand
    {
        public string Command { get; } =  "Refresh";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } =  "Refresh every players nicknames/badges to match their level (used after reloading).";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.refresh"))
            {
                response = "You don't have permission (xps.refresh) to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "nicks/badges";
                return false;
            }

            var arg = arguments.At(0).ToLower();
            bool nicks = arg.StartsWith("n");
            if (!nicks && !arg.StartsWith("b"))
            {
                response = $"Invalid argument: {arg}, use: nicks or badges";
                return false;
            }

            if (nicks)
            {
                if (!Main.EnabledNick)
                {
                    response = "Nicknames are disabled.";
                    return false;
                }
                foreach (var hub in ReferenceHub.AllHubs)
                {
                    if (hub == ReferenceHub.HostHub)
                        continue;
                    hub.nicknameSync.DisplayName = hub.nicknameSync.Network_myNickSync;
                }
            }
            else
            {
                if (!Main.EnabledRank)
                {
                    response = "Badges are disabled.";
                    return false;
                }
                foreach (var hub in ReferenceHub.AllHubs)
                {
                    if (hub == ReferenceHub.HostHub)
                        continue;
                    API.UpdateBadge(hub);
                }
            }


            response = $"{(nicks ? "Nicks" : "Badges")} refreshed.";
            return true;
        }
    }
}
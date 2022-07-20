namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    public class RefreshNicks : ICommand
    {
        public string Command { get; } =  "refresh";
        public string[] Aliases { get; } =  new string[] { "setnicks" };
        public string Description { get; } =  "Refresh every players nicknames to match their level (used after reloading).";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("xps.refresh"))
            {
                response = "You don't have permission (xps.refresh) to use this command.";
                return false;
            }

            foreach (var ply in Player.List)
            {
                ply.DisplayNickname = ply.Nickname;
            }

            response = "Nicks refreshed.";
            return true;
        }
    }
}
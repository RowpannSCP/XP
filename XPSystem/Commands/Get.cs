using System;
using CommandSystem;
using XPSystem.API;

namespace XPSystem.Commands
{
    internal class Get : ICommand
    {
        public string Command { get; } = "get";
        public string[] Aliases { get; } =  new string[] { };
        public string Description { get; } =  "Gets the player's XP and LVL values by userid";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.get"))
            {
                response = "You don't have permission (xps.get) to use this command.";
                return false;
            }
            if (arguments.Count == 0)
            {
                response = "Usage : XPSystem get (userid)";
                return false;
            }

            int.TryParse(arguments.At(0), out var usernetid);
            ReferenceHub ply = ReferenceHub.GetHub(usernetid);
            if (!(API.API.TryGetLog(arguments.At(0), out var log) || ply != null))
            {
                response = "incorrect userid";
                return false;
            }

            log ??= ply.GetLog();

            response = $"LVL: {log.LVL} | XP: {log.XP}";
            return true;
        }
    }
}

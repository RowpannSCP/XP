using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using XPSystem.API;
using XPSystem.API.Serialization;

namespace XPSystem.Commands
{
    internal class Get : ICommand
    {
        public string Command { get; } = "get";
        public string[] Aliases { get; } =  new string[] { };
        public string Description { get; } =  "Gets the player's XP and LVL values by userid";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender.CheckPermission("xps.get")))
            {
                response = "You don't have permission (xps.get) to use this command.";
                return false;
            }
            if (arguments.Count == 0)
            {
                response = "Usage : XPSystem get (userid)";
                return false;
            }
            
            Player ply = Player.Get(arguments.At(0));
            if (!(API.API.TryGetLog(arguments.At(0), out var log) || ply != null))
            {
                response = "incorrect userid";
                return false;
            }

            if (log == null)
                log = ply.GetLog();
            
            response = $"LVL: {log.LVL} | XP: {log.XP}";
            return true;
        }
    }
}

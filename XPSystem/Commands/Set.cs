using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace XPSystem.Commands
{
    internal class Set : ICommand
    {
        public string Command { get; } =  "set";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  $"Set a certain value in player's lvl variable.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("xps.set"))
            {
                response = "You don't have permission (xps.set) to use this command.";
                return false;
            }
            if (arguments.Count != 2)
            {
                response = "Usage : XPSystem set (UserId | in-game id) (int amount)";
                return false;
            }
            PlayerLog log;
            Player ply = Player.Get(arguments.At(0));
            if (!(Main.Players.TryGetValue(arguments.At(0), out log) || (ply != null && Main.Players.TryGetValue(ply.UserId, out log))))
            {
                response = "incorrect userid";
                return false;
            }
            if (int.TryParse(arguments.At(1), out int lvl) && lvl > 0)
            {
                log.LVL = lvl;
                response = $"{arguments.At(0)}'s LVL is now {log.LVL}";
                log.ApplyRank();
                return true;
            }
            response = $"Invalid amount of LVLs : {lvl}";
            return false;
        }
    }
}

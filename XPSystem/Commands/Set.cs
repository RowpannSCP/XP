using System;
using CommandSystem;
using XPSystem.API;

namespace XPSystem.Commands
{
    internal class Set : ICommand
    {
        public string Command { get; } =  "set";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  $"Set a certain value in player's lvl variable.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.set"))
            {
                response = "You don't have permission (xps.set) to use this command.";
                return false;
            }
            if (arguments.Count != 2)
            {
                response = "Usage : XPSystem set (UserId | in-game id) (int amount)";
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
            
            if (int.TryParse(arguments.At(1), out int lvl) && lvl > 0)
            {
                log.LVL = lvl;
                log.UpdateLog();
                response = $"{arguments.At(0)}'s LVL is now {log.LVL}";
                ply.serverRoles.SetText("");
                ply.nicknameSync.DisplayName = ply.nicknameSync.Network_myNickSync;
                return true;
            }
            response = $"Invalid amount of LVLs : {lvl}";
            return false;
        }
    }
}

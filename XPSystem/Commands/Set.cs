using System;
using CommandSystem;
using XPSystem.API;

namespace XPSystem.Commands
{
    using API = XPSystem.API.API;

    internal class Set : ICommand
    {
        public string Command { get; } =  "set";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  "Set a certain value in player's lvl variable.";
        
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
            if (!(API.TryGetLog(arguments.At(0), out var log) || ply != null))
            {
                response = "incorrect userid";
                return false;
            }

            log ??= ply.GetLog();

            var arg = arguments.At(1);
            if (int.TryParse(arg, out int lvl) && lvl > 0)
            {
                log.LVL = lvl;
                log.UpdateLog();
                response = $"{arguments.At(0)}'s LVL is now {log.LVL}";
                if (Main.EnabledRank)
                    API.UpdateBadge(ply);
                if (Main.EnabledNick)
                    ply.nicknameSync.DisplayName = ply.nicknameSync.Network_myNickSync;
                return true;
            }
            response = $"Invalid amount of LVLs : {arg}";
            return false;
        }
    }
}

namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    internal class Give : ICommand
    {
        public string Command { get; } =  "give";
        public string[] Aliases { get; } =  Array.Empty<string>();
        public string Description { get; } =  "Give someone xp.";
        
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.give"))
            {
                response = "You don't have permission (xps.give) to use this command.";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Usage : XPSystem give (UserId | in-game id) (int amount)";
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
            if (int.TryParse(arg, out int xp) && xp > 0)
            {
                log.AddXP(xp);
                response = $"{arguments.At(0)}'s LVL is now {log.LVL}";
                return true;
            }

            response = $"Invalid amount of xp : {arg}";
            return false;
        }
    }
}
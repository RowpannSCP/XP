namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class PauseCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.pause"))
            {
                response = "You don't have permission (xps.pause) to use this command.";
                return false;
            }

            if (arguments.Count > 0)
            {
                switch (arguments.At(0).ToLower())
                {
                    case "pause":
                    case "p":
                    case "true":
                    case "1":
                        XPAPI.XPGainPaused = true;
                        response = "XP gain paused.";
                        return true;
                    case "unpause":
                    case "up":
                    case "false":
                    case "0":
                        XPAPI.XPGainPaused = false;
                        response = "XP gain unpaused.";
                        return true;
                }
            }

            response = $"XP gain is currently {(XPAPI.XPGainPaused ? "paused" : "unpaused")}.";
            return true;
        }

        public override string Command { get; } = "pause";
        public override string[] Aliases { get; } = new[] { "p" };
        public override string Description { get; } = "Pause xp gain.";
    }
}
namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class MultiplierCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xp.setmultiplier"))
            {
                response = "You do not have permission to run this command.";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: xp setmultiplier global (multiplier) or xp setmultiplier player (player) (multiplier)";
                return false;
            }

            float multiplier;
            switch (arguments.At(0).ToLower())
            {
                case "g":
                case "global":
                    if (!float.TryParse(arguments.At(1), out multiplier))
                    {
                        response = $"Global multiplier: {XPAPI.Config.GlobalXPMultiplier}.";
                        return true;
                    }

                    XPAPI.Config.GlobalXPMultiplier = multiplier;
                    response = $"Global XP multiplier temporarily set to {multiplier}.";
                    return true;
                case "p":
                case "player":
                    XPPlayer player;
                    int intArgLocation;

                    if (arguments.Count > 2)
                    {
                        intArgLocation = 2;

                        if (!XPPlayer.TryGet(arguments.At(1), out player))
                        {
                            response = "Invalid player.";
                            return false;
                        }
                    }
                    else if (XPPlayer.TryGet(sender, out player))
                    {
                        intArgLocation = 1;
                    }
                    else
                    {
                        response = "You must specify a player.";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(intArgLocation), out multiplier))
                    {
                        response = $"{player.DisplayedName}'s multiplier: {player.XPMultiplier}.";
                        return false;
                    }

                    player.XPMultiplier = multiplier;
                    response = $"{player.DisplayedName}'s multiplier set to {multiplier}.";
                    return true;
                default:
                    response = "Invalid target.";
                    return false;
            }
        }

        public override string Command { get; } = "multiplier";
        public override string[] Aliases { get; } = new[] { "mult" };
        public override string Description { get; } = "Gets or sets XP multipliers.";
    }
}
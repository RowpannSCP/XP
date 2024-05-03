namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class MultiplierCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xp.setmultiplier"))
            {
                response = "You do not have permission to run this command.";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: xp setmultiplier <player/global> (player) <multiplier>";
                return false;
            }

            float multiplier;
            switch (arguments.At(0))
            {
                case "g":
                case "global":
                    if (!float.TryParse(arguments.At(1), out multiplier))
                    {
                        response = $"Global multiplier: {XPAPI.GlobalXPMultiplier}.";
                        return true;
                    }

                    XPAPI.GlobalXPMultiplier = multiplier;
                    response = $"Global XP multiplier set to {multiplier}.";
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

        public string Command { get; } = "multiplier";
        public string[] Aliases { get; } = new[] { "mult" };
        public string Description { get; } = "Gets or sets XP multipliers.";
    }
}
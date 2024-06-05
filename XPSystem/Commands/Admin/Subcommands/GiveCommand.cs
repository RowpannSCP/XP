namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class GiveCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.give", out var player))
            {
                response = "You don't have permission (xps.give) to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: xp give <amount> or xp give <amount> <player>";
                return false;
            }
            
            var amountString = arguments.At(0);
            if (!int.TryParse(amountString, out int amount))
            {
                response = $"Invalid amount: {amountString}.";
                return false;
            }

            response = null;
            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out var playerInfo, out var playerId,
                    out string nickname))
                return false;

            playerInfo.XP += amount;

            response = $"Gave {amount} XP to {playerId.ToString()} ({nickname}).";
            return true;
        }

        public override string Command { get; } = "give";
        public override string Description { get; } = "Give a player XP.";
    }
}
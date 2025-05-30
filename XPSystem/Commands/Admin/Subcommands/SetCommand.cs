namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class SetCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.set", out BaseXPPlayer? player))
            {
                response = "You don't have permission (xps.set) to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            if (arguments.Count < 1)
            {
                response = "Usage: xp set (amount) or xp set (amount) (player)";
                return false;
            }

            string amountString = arguments.At(0);
            if (!int.TryParse(amountString, out int amount) || amount < 0)
            {
                response = $"Invalid amount: {amountString}.";
                return false;
            }

            response = "temp";
            XPAPI.EnsureStorageProviderValid();

            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out PlayerInfoWrapper playerInfo, out IPlayerId playerId))
                return false;

            playerInfo.XP = amount;

            response = $"Set {playerId.ToString()} ({playerInfo.Nickname})'s XP to {amount}.";
            return true;
        }

        public override string Command { get; } = "set";
        public override string Description { get; } = "Set a player's XP.";
    }
}
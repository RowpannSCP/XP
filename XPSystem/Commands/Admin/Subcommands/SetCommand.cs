﻿namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class SetCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.set", out var player))
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

            var amountString = arguments.At(0);
            if (!int.TryParse(amountString, out int amount) || amount < 0)
            {
                response = $"Invalid amount: {amountString}.";
                return false;
            }

            response = null;
            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out var playerInfo, out var playerId))
                return false;

            playerInfo.XP = amount;

            response = $"Set {playerId.ToString()} ({playerInfo.Nickname})'s XP to {amount}.";
            return true;
        }

        public override string Command { get; } = "set";
        public override string Description { get; } = "Set a player's XP.";
    }
}
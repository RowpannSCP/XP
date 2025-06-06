﻿namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class GiveCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.give", out BaseXPPlayer? player))
            {
                response = "You don't have permission (xps.give) to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: xp give (amount) or xp give (amount) (player)";
                return false;
            }
            
            string amountString = arguments.At(0);
            if (!int.TryParse(amountString, out int amount))
            {
                response = $"Invalid amount: {amountString}.";
                return false;
            }

            response = "temp";
            XPAPI.EnsureStorageProviderValid();

            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out PlayerInfoWrapper playerInfo, out IPlayerId playerId))
                return false;

            playerInfo.XP += amount;

            response = $"Gave {amount} XP to {playerId.ToString()} ({playerInfo.Nickname}).";
            return true;
        }

        public override string Command { get; } = "give";
        public override string Description { get; } = "Give a player XP.";
    }
}
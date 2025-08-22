namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class GetCommandAdmin : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.get", out BaseXPPlayer? player))
            {
                response = "You don't have permission (xps.get) to use this command.";
                return false;
            }

            response = "temp";
            XPAPI.EnsureStorageProviderValid();

            if (!DoThingWithArgs(ref arguments, 0, player, ref response, out PlayerInfoWrapper playerInfo, out IPlayerId playerId))
                return false;

            response = $"{playerId.ToString()} ({playerInfo.Nickname})] XP: {playerInfo.XP} (Level {playerInfo.Level})";
            return true;
        }

        public override string Command { get; } = "get";
        public override string Description { get; } = "Get a player's XP and LVL.";
    }
}
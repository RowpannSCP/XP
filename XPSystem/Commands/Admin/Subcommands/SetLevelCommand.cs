namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public class SetLevelCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.setlevel", out BaseXPPlayer? player))
            {
                response = "You don't have permission (xps.setlevel) to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            if (arguments.Count < 1)
            {
                response = "Usage: xp setlevel (level) or xp setlevel (level) (player)";
                return false;
            }

            string levelString = arguments.At(0);
            if (!int.TryParse(levelString, out int level) || level < 0)
            {
                response = $"Invalid level: {levelString}.";
                return false;
            }

            response = "temp";
            XPAPI.EnsureStorageProviderValid();

            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out PlayerInfoWrapper playerInfo, out IPlayerId playerId))
                return false;

            playerInfo.Level = level;

            response = $"Set {playerId.ToString()} ({playerInfo.Nickname})'s level to {level}.";
            return true;
        }

        public override string Command { get; } = "setlevel";
        public override string Description { get; } = "Set a player's level.";
    }
}
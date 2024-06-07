namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class SetLevelCommand : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.setlevel", out var player))
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

            var levelString = arguments.At(0);
            if (!int.TryParse(levelString, out int level) || level < 0)
            {
                response = $"Invalid level: {levelString}.";
                return false;
            }

            response = null;
            if (!DoThingWithArgs(ref arguments, 1, player, ref response, out var playerInfo, out var playerId,
                    out string nickname))
                return false;

            playerInfo.Level = level;

            response = $"Set {playerId.ToString()} ({nickname})'s level to {level}.";
            return true;
        }

        public override string Command { get; } = "setlevel";
        public override string Description { get; } = "Set a player's level.";
    }
}
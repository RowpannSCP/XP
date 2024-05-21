namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class GetCommandAdmin : DatabasePlayerCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGetAndCheckPermission(sender, "xps.get", out var player))
            {
                response = "You don't have permission (xps.get) to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            response = null;
            if (!DoThingWithArgs(ref arguments, 0, player, ref response, out var playerInfo, out var playerId,
                    out string nickname))
                return false;

            response = $"{playerId.ToString()} ({nickname})] XP: {playerInfo.XP} (Level {playerInfo.Level})";
            return true;
        }

        public override string Command { get; } = "get";
        public override string Description { get; } = "Get a player's XP and LVL.";
    }
}
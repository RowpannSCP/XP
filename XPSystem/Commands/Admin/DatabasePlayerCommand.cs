namespace XPSystem.Commands.Admin
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Command base for commands that get a database entry for a player from a specific argument.
    /// </summary>
    public abstract class DatabasePlayerCommand : ICommand
    {
        /// <summary>
        /// Does the thing with the arguments.
        /// Only works if the to be specified player is the last argument.
        /// </summary>
        /// <param name="arguments">The command arguments.</param>
        /// <param name="targetPlayerArgumentIndex">The index of the argument that would specify the targeted player.</param>
        /// <param name="player">The player that executed the command.</param>
        /// <param name="response">The response to be sent back to the player.</param>
        /// <param name="playerInfo">The targeted player's info, if found, otherwise default.</param>
        /// <param name="playerId">The targeted player's id, if found, otherwise 0.</param>
        /// <param name="nickname">The targeted player's nickname, if found, otherwise null.</param>
        /// <returns>Whether the operation was successful (whether or not to return immediately after).</returns>
        /// <remarks>If the argument count and the target player's argument position don't match, the sender will become the targeted player.</remarks>
        protected bool DoThingWithArgs(ref ArraySegment<string> arguments, byte targetPlayerArgumentIndex, XPPlayer player, ref string response, out PlayerInfo playerInfo, out PlayerId playerId, out string nickname)
        {
            nickname = null;
            playerId = default;
            playerInfo = default;

            // If player is specified.
            if (arguments.Count > targetPlayerArgumentIndex)
            {
                var arg = arguments.At(targetPlayerArgumentIndex);
                // Try to get player by name or user id.
                if (XPPlayer.TryGet(arg, out player))
                {
                    playerId = player.GetPlayerId();
                    nickname = player.Nickname;
                }
                else
                {
                    // if player isn't online, try to parse user id.
                    if (!arg.TryParseUserId(out playerId))
                    {
                        response = "Invalid player.";
                        return false;
                    }

                    nickname = "OFFLINE";
                }
            }
            // Player not specified, use sender.
            else
            {
                playerId = player.GetPlayerId();
                nickname = "You";
            }

            if (XPAPI.StorageProvider.TryGetPlayerInfo(playerId, out playerInfo))
                return true;

            response = "Player does not exist within database.";
            return false;

        }

        public string[] Aliases { get; } = Array.Empty<string>();

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);
        public abstract string Command { get; }
        public abstract string Description { get; }
    }
}
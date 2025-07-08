namespace XPSystem.Commands.Admin
{
    using System;
    using XPSystem.API;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    /// <summary>
    /// Command base for commands that get a database entry for a player from a specific argument.
    /// </summary>
    public abstract class DatabasePlayerCommand : SanitizedInputCommand
    {
        /// <summary>
        /// Does the thing with the arguments.
        /// Only works if the to be specified player is the last argument.
        /// </summary>
        /// <param name="arguments">The command arguments.</param>
        /// <param name="targetPlayerArgumentIndex">The index of the argument that would specify the targeted player.</param>
        /// <param name="sender">The player that executed the command.</param>
        /// <param name="response">The response to be sent back to the player.</param>
        /// <param name="playerInfo">The targeted player's info, if found, otherwise default.</param>
        /// <param name="playerId">The targeted player's id, if found, otherwise 0.</param>
        /// <returns>Whether the operation was successful (whether or not to return immediately after).</returns>
        /// <remarks>If the argument count and the target player's argument position don't match, the sender will become the targeted player.</remarks>
        protected bool DoThingWithArgs(ref ArraySegment<string> arguments, byte targetPlayerArgumentIndex, BaseXPPlayer? sender, ref string response, out PlayerInfoWrapper playerInfo, out IPlayerId playerId)
        {
            playerId = null!;
            playerInfo = null!;

            // If player is specified.
            if (arguments.Count > targetPlayerArgumentIndex)
            {
                string arg = arguments.At(targetPlayerArgumentIndex);
                // Try to get player by name or user id.
                if (XPPlayer.TryGet(arg, out BaseXPPlayer? basePlayer))
                {
                    if (basePlayer is not XPPlayer player)
                    {
                        response = "Player is not a xp player, can't do that.";
                        return false;
                    }

                    playerId = player.PlayerId;
                }
                else
                {
                    // if player isn't online, try to parse user id.
                    if (!arg.TryParseUserId(out playerId!))
                    {
                        response = "Invalid player ID.";
                        return false;
                    }
                }
            }
            // Player not specified, use sender.
            else if (sender is XPPlayer player)
            {
                playerId = player.PlayerId;
            }
            else
            {
                response = "You must specify a player (sender not a XPPlayer).";
                return false;
            }

            // XPAPI.EnsureStorageProviderValid(); - called in calling command(s)
            if (XPAPI.StorageProvider!.TryGetPlayerInfo(playerId, out playerInfo!))
                return true;

            response = "Player does not exist within database.";
            return false;

        }
    }
}
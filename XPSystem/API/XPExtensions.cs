namespace XPSystem.API
{
    using System.Collections.Generic;
    using CommandSystem;
    using XPSystem.API.StorageProviders.Models;

    public static class XPExtensions
    {
        /// <inheritdoc cref="XPAPI.GetPlayerId"/>
        public static PlayerId GetPlayerId(this XPPlayer player) => XPAPI.GetPlayerId(player);

        /// <inheritdoc cref="XPAPI.GetPlayerId"/>
        public static bool TryParseUserId(this string @string, out PlayerId playerId) => XPAPI.TryParseUserId(@string, out playerId);

        /// <inheritdoc cref="XPAPI.AddXP(XPPlayer, int, bool)"/>
        public static bool AddXP(this XPPlayer player, int amount, bool notify = false) => XPAPI.AddXP(player, amount, notify);

        /// <inheritdoc cref="XPAPI.GetLevel"/>
        public static int GetLevel(this PlayerInfo playerInfo) => XPAPI.GetLevel(playerInfo);

        /// <inheritdoc cref="XPAPI.FormatLeaderboard"/>
        public static string FormatLeaderboard(this IEnumerable<PlayerInfo> players) => XPAPI.FormatLeaderboard(players);

        /// <inheritdoc cref="LoaderSpecific.CheckPermission(ICommandSender, string)"/>
        public static bool CheckPermissionLS(this ICommandSender sender, string permission) => LoaderSpecific.CheckPermission(sender, permission);
    }
}
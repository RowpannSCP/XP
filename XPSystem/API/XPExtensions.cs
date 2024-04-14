namespace XPSystem.API
{
    using System.Collections.Generic;
    using CommandSystem;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;

    public static class XPExtensions
    {
        /// <inheritdoc cref="XPAPI.TryParseUserId"/>
        public static bool TryParseUserId(this string @string, out PlayerId playerId) => XPAPI.TryParseUserId(@string, out playerId);

        /// <inheritdoc cref="XPAPI.FormatLeaderboard"/>
        public static string FormatLeaderboard(this IEnumerable<PlayerInfoWrapper> players) => XPAPI.FormatLeaderboard(players);

        /// <inheritdoc cref="LoaderSpecific.CheckPermission(ICommandSender, string)"/>
        public static bool CheckPermissionLS(this ICommandSender sender, string permission) => LoaderSpecific.CheckPermission(sender, permission);

        /// <inheritdoc cref="XPAPI.GetPlayerInfo(PlayerId)"/>
        public static PlayerInfoWrapper GetPlayerInfo(this PlayerId playerId) => XPAPI.GetPlayerInfo(playerId);

        /// <inheritdoc cref="XPAPI.GetPlayerInfo(XPPlayer)"/>
        public static PlayerInfoWrapper GetPlayerInfo(this XPPlayer player) => XPAPI.GetPlayerInfo(player);

        /// <inheritdoc cref="XPAPI.AddXP(XPPlayer, int, bool)"/>
        public static bool AddXP(this XPPlayer player, int amount, bool notify = false) => XPAPI.AddXP(player, amount, notify);

        /// <inheritdoc cref="XPAPI.AddXPAndDisplayMessage{T}(XPPlayer, string, T)"/>
        public static void AddXP
    }
}
namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CommandSystem;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config.Events.Types;

    public static class XPExtensions
    {
        /// <inheritdoc cref="XPAPI.TryParseUserId"/>
        public static bool TryParseUserId(this string @string, [NotNullWhen(true)] out IPlayerId? playerId) => XPAPI.TryParseUserId(@string, out playerId);

        /// <inheritdoc cref="XPAPI.FormatLeaderboard"/>
        public static string FormatLeaderboard(this IEnumerable<PlayerInfoWrapper> players) => XPAPI.FormatLeaderboard(players);

        /// <inheritdoc cref="LoaderSpecific.CheckPermission(ICommandSender, string)"/>
        public static bool CheckPermissionLS(this ICommandSender sender, string permission) => LoaderSpecific.CheckPermission(sender, permission);

        /// <inheritdoc cref="LoaderSpecific.GetCategory"/>
        public static ItemCategory GetCategory(this ItemType itemType) => LoaderSpecific.GetCategory(itemType);

        /// <inheritdoc cref="XPAPI.GetPlayerInfo(IPlayerId)"/>
        public static PlayerInfoWrapper GetPlayerInfo(this IPlayerId playerId) => XPAPI.GetPlayerInfo(playerId);

        /// <inheritdoc cref="XPAPI.GetPlayerInfo(XPPlayer)"/>
        public static PlayerInfoWrapper GetPlayerInfo(this XPPlayer player) => XPAPI.GetPlayerInfo(player);

        /// <inheritdoc cref="XPAPI.DisplayMessage(BaseXPPlayer, string)"/>
        public static void DisplayMessage(this BaseXPPlayer player, string message) => XPAPI.DisplayMessage(player, message);

        /// <inheritdoc cref="XPAPI.AddXP(XPPlayer, int, bool, PlayerInfoWrapper)"/>
        public static bool AddXP(this XPPlayer player, int amount, bool notify = false) => XPAPI.AddXP(player, amount, notify);

        /// <inheritdoc cref="XPAPI.AddXPAndDisplayMessage(XPPlayer, XPECItem)"/>
        public static void TryAddXPAndDisplayMessage(this XPPlayer player, XPECItem? item) => XPAPI.AddXPAndDisplayMessage(player, item);

        /// <inheritdoc cref="XPAPI.TryAddXPAndDisplayMessage(XPPlayer, string, object[])"/>
        public static void TryAddXPAndDisplayMessage(this XPPlayer? player, string key, params object?[] args) => XPAPI.TryAddXPAndDisplayMessage(player, key, args);

        /// <inheritdoc cref="XPAPI.FormatType"/>
        public static string FormatType(this Type type) => XPAPI.FormatType(type);
    }
}
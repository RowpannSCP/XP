namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using XPSystem.API.Enums;
    using XPSystem.API.Exceptions;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config;
    using static LoaderSpecific;

    public static class XPAPI
    {
        /// <summary>
        /// Whether the plugin is enabled or not.
        /// </summary>
        public static bool PluginEnabled { get; internal set; } = false;

        /// <summary>
        /// The plugin config, for nwapi or exiled-specific configs use safe casts.
        /// </summary>
        public static ConfigShared Config = new();

        /// <summary>
        /// The display provider for the plugin.
        /// May be null (<see cref="XPSystem.API.Enums.DisplayMode.None"/> or uninitialized).
        /// </summary>
        public static IMessagingProvider MessagingProvider;

        /// <summary>
        /// The parameters for the storage provider.
        /// </summary>
        public static Dictionary<string, string> StorageProviderParameters = new();

        /// <summary>
        /// The storage provider for the plugin.
        /// Change it using <see cref="SetStorageProvider"/>.
        /// </summary>
        public static IStorageProvider StorageProvider { get; private set; }

        /// <summary>
        /// The xp display providers.
        /// </summary>
        public static readonly XPDisplayProviderCollection DisplayProviders = new();

        /// <summary>
        /// Whether xp gain is currently paused.
        /// </summary>
        public static bool XPGainPaused = false;

        /// <summary>
        /// Sets the storage provider for the plugin.
        /// </summary>
        /// <param name="provider">The storage provider to set.</param>
        public static void SetStorageProvider(IStorageProvider provider)
        {
            try
            {
                StorageProvider?.Dispose();
            }
            catch (Exception e)
            {
                LogError("Error while disposing the previous storage provider: " + e);
            }

            try
            {
                LoadStorageProviderParameters(provider);
                provider.Initialize(StorageProviderParameters);
            }
            catch (Exception e)
            {
                LogError("Could not set storage provider: " + e);
                LogError("No data will be saved!");
                return;
            }

            StorageProvider = provider;
            LogInfo("Set storage provider to " + provider.GetType().Name);
        }

        /// <summary>
        /// Loads the storage provider parameters from the file.
        /// </summary>
        /// <param name="provider">The storage provider used to load the parameters.</param>
        public static void LoadStorageProviderParameters(IStorageProvider provider)
        {
            try
            {
                var file = Path.Combine(Config.ExtendedConfigPath, Config.DatabaseParametersFile);
                using var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                StorageProviderParameters = provider.LoadParameters(fs);
            }
            catch (Exception e)
            {
                throw new Exception("Could not load storage provider parameters: ", e);
            }
        }

        /// <summary>
        /// Ensures that the storage provider is valid.
        /// </summary>
        /// <exception cref="StorageProviderInvalidException">Thrown if the storage provider has not been successfully set.</exception>
        public static void EnsureStorageProviderValid()
        {
            if (StorageProvider == null)
                throw new StorageProviderInvalidException("No storage provider has been set successfully.");
        }

#if STORENICKS
        /// <summary>
        /// Updates the stored nickname of a player.
        /// </summary>
        public static void UpdateNickname(XPPlayer player)
        {
            EnsureStorageProviderValid();

            var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.GetPlayerId());
            playerInfo.Nickname = player.Nickname;
            StorageProvider.SetPlayerInfo(playerInfo);
        }
#endif

        /// <summary>
        /// Adds XP to a player.
        /// </summary>
        /// <param name="player">The player to add XP to.</param>
        /// <param name="amount">The amount of XP to add.</param>
        /// <param name="force">Whether to force the addition of XP, even if <see cref="XPGainPaused"/>.</param>
        /// <returns>Whether or not the XP was added.</returns>
        public static bool AddXP(XPPlayer player, int amount, bool force = false)
        {
            EnsureStorageProviderValid();

            if (XPGainPaused && !force)
                return false;

            var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.GetPlayerId());
            int prevLevel = GetLevel(playerInfo);

            playerInfo.XP += amount;
            StorageProvider.SetPlayerInfo(playerInfo);

            if (GetLevel(playerInfo) > prevLevel)
                DisplayProviders.Refresh(player);

            return true;
        }

        /// <summary>
        /// Adds XP to a player.
        /// </summary>
        /// <param name="playerInfo">The <see cref="PlayerInfo"/> of the player to add XP to.</param>
        /// <param name="amount">The amount of XP to add.</param>
        /// <param name="force">Whether to force the addition of XP, even if <see cref="XPGainPaused"/>.</param>
        /// <returns>Whether or not the XP was added.</returns>
        public static bool AddXP(PlayerInfo playerInfo, int amount, bool force = false)
        {
            EnsureStorageProviderValid();

            if (XPGainPaused && !force)
                return false;

            int prevLevel = GetLevel(playerInfo);

            playerInfo.XP += amount;
            StorageProvider.SetPlayerInfo(playerInfo);

            if (GetLevel(playerInfo) > prevLevel && TryGetPlayer(playerInfo.Player, out var xpPlayer))
                DisplayProviders.Refresh(xpPlayer);

            return true;
        }

        /// <summary>
        /// Gets the level of a player.
        /// </summary>
        /// <param name="playerInfo">The player's info.</param>
        /// <returns>The player's level.</returns>
        public static int GetLevel(PlayerInfo playerInfo)
        {
            return playerInfo.XP / Config.XPPerLevel;
        }

        /// <summary>
        /// Gets the <see cref="PlayerId"/> of a <see cref="XPPlayer"/>.
        /// </summary>
        public static PlayerId GetPlayerId(XPPlayer player)
        {
            if (!TryParseUserId(player.UserId, out var playerId))
                throw new InvalidOperationException("PlayerId of player is invalid (GetPlayerId).");

            return playerId;
        }

        /// <summary>
        /// Attempts to get a player using a <see cref="PlayerId"/>.
        /// </summary>
        /// <param name="playerId">The <see cref="PlayerId"/> of the player.</param>
        /// <param name="player">The player, if on the server.</param>
        /// <returns>Whether or not the player is on the server.</returns>
        public static bool TryGetPlayer(PlayerId playerId, out XPPlayer player)
        {
            return XPPlayer.TryGet(playerId.ToString(), out player);
        }

        /// <summary>
        /// Attempts to parse a string into a <see cref="PlayerId"/>.
        /// </summary>
        /// <param name="string">The string to parse.</param>
        /// <param name="playerId">The equivalent <see cref="PlayerId"/>.</param>
        /// <returns>Whether or not the parsing was successful.</returns>
        public static bool TryParseUserId(string @string, out PlayerId playerId)
        {
            playerId = default;
            if (@string == null)
                return false;

            var split = @string.Split('@');
            switch (split[1].ToLower())
            {
                case "steam":
                    playerId.AuthType = AuthType.Steam;
                    break;
                case "discord":
                    playerId.AuthType = AuthType.Discord;
                    break;
                case "northwood":
                    playerId.AuthType = AuthType.Northwood;
                    break;
                default:
                    return false;
            }

            if (!ulong.TryParse(split[0], out var ulongId))
                return false;

            playerId.Id = ulongId;
            return true;
        }

        /// <summary>
        /// Formats the leaderboard entries into a string.
        /// </summary>
        /// <param name="players">The player data to format.</param>
        /// <returns>The formatted leaderboard, as a string..</returns>
        public static string FormatLeaderboard(IEnumerable<PlayerInfo> players)
        {
            StringBuilder sb = new();
            foreach (var playerInfo in players)
            {
#if STORENICKS
                sb.AppendLine(string.IsNullOrWhiteSpace(playerInfo.Nickname)
                    ? $"{playerInfo.Player.Id.ToString()} - {playerInfo.XP} ({playerInfo.GetLevel()})"
                    : $"{playerInfo.Nickname} - {playerInfo.XP} ({playerInfo.GetLevel()})");
#else
                sb.AppendLine($"{playerInfo.Player.Id.ToString()} - {playerInfo.XP} ({playerInfo.GetLevel()})");
#endif
            }

            return sb.ToString();
        }
    }
}
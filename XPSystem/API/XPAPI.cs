namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.Enums;
    using XPSystem.API.Exceptions;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config;
    using XPSystem.Config.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;
    using static LevelCalculator;
    using static LoaderSpecific;

    public static class XPAPI
    {
#region Properties
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
        /// The storage provider for the plugin.
        /// Change it using <see cref="SetStorageProvider"/>.
        /// </summary>
        public static IStorageProvider StorageProvider { get; private set; }

        /// <summary>
        /// The xp display providers.
        /// </summary>
        public static readonly XPDisplayProviderCollection DisplayProviders = new();

        /// <summary>
        /// Gets the serializer for the plugin.
        /// </summary>
        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithLoaderTypeConverters()
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreFields()
            .Build();

        /// <summary>
        /// Gets the deserializer for the plugin.
        /// </summary>
        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithLoaderTypeConverters()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), 
                deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

        /// <summary>
        /// Whether xp gain is currently paused.
        /// </summary>
        public static bool XPGainPaused = false;
#endregion

#region Storage
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
                if (provider is StorageProvider { ConfigTypeInternal: not null } storageProvider)
                    LoadStorageProviderConfig(storageProvider);

                provider.Initialize();
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
        /// Loads the storage providers config.
        /// </summary>
        /// <param name="provider">The storage provider whose config is to be loaded.</param>
        public static void LoadStorageProviderConfig(StorageProvider provider)
        {
            string name = provider.GetType().Name;
            string file = Path.Combine(Config.ExtendedConfigPath, name + ".yml");

            if (File.Exists(file))
            {
                try
                {
                    provider.ConfigPropertyInternal =
                        Deserializer.Deserialize(File.ReadAllText(file), provider.ConfigTypeInternal);
                }
                catch (Exception e)
                {
                    LogError($"Error loading storageprovider config for {name}: {e}");
                }
            }
            else
            {
                var obj = provider.ConfigPropertyInternal;

                File.WriteAllText(file, Serializer.Serialize(obj));
                provider.ConfigPropertyInternal = obj;
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

        /// <summary>
        /// Gets the player info of a player.
        /// Will create a new one if it doesn't exist.
        /// </summary>
        /// <param name="playerId">The <see cref="PlayerId"/> of the player to get the info of.</param>
        /// <returns>The <see cref="PlayerInfoWrapper"/> belonging to the player.</returns>
        public static PlayerInfoWrapper GetPlayerInfo(PlayerId playerId)
        {
            EnsureStorageProviderValid();
            return StorageProvider.GetPlayerInfoAndCreateOfNotExist(playerId);
        }

        /// <summary>
        /// Gets the player info of a player.
        /// </summary>
        /// <param name="player">The player to get the info of.</param>
        /// <returns>The <see cref="PlayerInfoWrapper"/> belonging to the player.</returns>
        public static PlayerInfoWrapper GetPlayerInfo(XPPlayer player)
        {
            return GetPlayerInfo(player.PlayerId);
        }

#if STORENICKS
        /// <summary>
        /// Updates the stored nickname of a player.
        /// </summary>
        public static void UpdateNickname(XPPlayer player)
        {
            EnsureStorageProviderValid();

            var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.PlayerId);
            playerInfo.PlayerInfo.Nickname = player.Nickname;
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
            if (amount == 0)
                return false;

            EnsureStorageProviderValid();

            if (XPGainPaused && !force)
                return false;

            var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.PlayerId);
            int prevLevel = GetLevel(playerInfo);

            playerInfo.PlayerInfo.XP += amount;
            StorageProvider.SetPlayerInfo(playerInfo);

            if (player.IsConnected && GetLevel(playerInfo) != prevLevel)
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
        public static bool AddXP(PlayerInfoWrapper playerInfo, int amount, bool force = false)
        {
            if (amount == 0)
                return false;

            if (XPGainPaused && !force)
                return false;

            playerInfo.XP += amount;
            return true;
        }
#endregion
#region Translations
        /// <summary>
        /// Formats and displays a message to a player.
        /// <remarks>If <see cref="MessagingProvider"/> is null, it will not be displayed.</remarks>
        /// </summary>
        /// <param name="player">The player to display the message to.</param>
        /// <param name="message">The message to display.</param>
        public static void DisplayMessage(XPPlayer player, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            MessagingProvider?.DisplayMessage(player, Config.TextPrefix + message + Config.TextSuffix, Config.DisplayDuration);
        }
#endregion
#region Mixed
        /// <summary>
        /// Adds XP to a player and displays it's corresponding message.
        /// <remarks>Uses <see cref="XPECManager.GetXPEC{T}(string, T)"/>.</remarks>
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="key">The key of the XPEC.</param>
        /// <param name="subkey">The subkey of the XPEC.</param>
        /// <typeparam name="T">The type of the subkey.</typeparam>
        /// <exception cref="Exception">The XPEC file for the key cannot be found.</exception>
        public static void AddXPAndDisplayMessage<T>(XPPlayer player, string key, T subkey)
        #error
        {
            var xpecitem = XPECManager.GetXPEC(key, subkey)
                           ?? throw new Exception("Key does not match any XPEC.");

            AddXP(player, xpecitem.Amount);
            DisplayMessage(player, xpecitem.Translation);
        }

        /// <summary>
        /// Adds XP to a player and displays it's corresponding message.
        /// <remarks>Uses <see cref="XPECManager.GetXPEC(string)"/>.</remarks>
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="key">The key of the XPEC.</param>
        /// <exception cref="Exception">The XPEC file for the key cannot be found.</exception>
        public static void AddXPAndDisplayMessage(XPPlayer player, string key)
        {
            AddXPAndDisplayMessage<object>(player, key, null);
        }
#endregion
#region Misc
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
        public static string FormatLeaderboard(IEnumerable<PlayerInfoWrapper> players)
        {
            StringBuilder sb = new();
            foreach (var playerInfo in players)
            {
#if STORENICKS
                sb.AppendLine(string.IsNullOrWhiteSpace(playerInfo.Nickname)
                    ? $"{playerInfo.Player.Id.ToString()} - {playerInfo.XP} ({GetLevel(playerInfo)})"
                    : $"{playerInfo.Nickname} - {playerInfo.XP} ({GetLevel(playerInfo)})");
#else
                sb.AppendLine($"{playerInfo.Player.Id.ToString()} - {playerInfo.XP} ({playerInfo.GetLevel()})");
#endif
            }

            return sb.ToString();
        }
#endregion
    }
}
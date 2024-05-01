namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NorthwoodLib.Pools;
    using PlayerRoles;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.Enums;
    using XPSystem.API.Exceptions;
    using XPSystem.API.StorageProviders;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.Config;
    using XPSystem.Config.Events;
    using XPSystem.Config.Events.Types;
    using XPSystem.Config.Events.Types.Custom;
    using XPSystem.Config.YamlConverters;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;

    public static class XPAPI
    {
#region Properties
        /// <summary>
        /// Whether the plugin is enabled or not.
        /// </summary>
        public static bool PluginEnabled { get; internal set; } = false;

        /// <summary>
        /// The plugin config, cast to get nwapi or exiled-specific config.
        /// </summary>
        public static Config Config = new UninitializedConfig();

        /// <summary>
        /// The display provider for the plugin.
        /// May be null (<see cref="XPSystem.API.Enums.DisplayMode.None"/> or uninitialized).
        /// </summary>
        public static IMessagingProvider MessagingProvider;

        /// <summary>
        /// The storage provider for the plugin.
        /// Change it using <see cref="SetStorageProvider(IStorageProvider)"/>.
        /// </summary>
        public static IStorageProvider StorageProvider { get; private set; }

        /// <summary>
        /// The xp display providers.
        /// </summary>
        public static readonly XPDisplayProviderCollection DisplayProviders = new();

        /// <summary>
        /// Gets a new serializer builder for the plugin.
        /// </summary>
        private static SerializerBuilder SerializerBuilder => new SerializerBuilder()
            .WithLoaderTypeConverters()
            .WithTypeConverter(new XPECFileYamlConverter())
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreFields();

        /// <summary>
        /// Gets the serializer of the plugin.
        /// </summary>
        public static ISerializer Serializer { get; } = SerializerBuilder.Build();

        /// <summary>
        /// Gets the value serializer of the plugin.
        /// </summary>
        public static IValueSerializer ValueSerializer { get; } = SerializerBuilder.BuildValueSerializer();

        /// <summary>
        /// Gets a new deserializer builder for the plugin.
        /// </summary>
        private static DeserializerBuilder DeserializerBuilder => new DeserializerBuilder()
            .WithLoaderTypeConverters()
            .WithTypeConverter(new XPECFileYamlConverter())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner),
                deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties();

        /// <summary>
        /// Gets the deserializer for the plugin.
        /// </summary>
        public static IDeserializer Deserializer { get; } = DeserializerBuilder.Build();

        /// <summary>
        /// Whether xp gain is currently paused.
        /// </summary>
        public static bool XPGainPaused = false;

        /// <summary>
        /// Prints a debug message, if debug is enabled.
        /// Not too different from your loader's LogDebug.
        /// </summary>
        public static Action<string> LogDebug = LoaderSpecific.LogDebug; 

        /// <summary>
        /// Prints a info message.
        /// Not too different from your loader's LogInfo.
        /// </summary>
        public static Action<string> LogInfo = LoaderSpecific.LogInfo;

        /// <summary>
        /// Prints a warning.
        /// Not too different from your loader's LogWarn.
        /// </summary>
        public static Action<string> LogWarn = LoaderSpecific.LogWarn;

        /// <summary>
        /// Prints a error message.
        /// Not too different from your loader's LogError.
        /// </summary>
        public static Action<string> LogError = LoaderSpecific.LogError;
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

            if (provider == null)
            {
                LogDebug("Disposed storage provider. No data will be read or saved!");
                return;
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
                LogError("No data will be read or saved!");
                return;
            }

            StorageProvider = provider;
            LogInfo("Set storage provider to " + provider.GetType().Name);
        }

        /// <summary>
        /// Sets the storage provider for the plugin.
        /// </summary>
        /// <param name="typeName">The full type name of the type of the storage provider to set.</param>
        public static void SetStorageProvider(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                LogError("Storage provider type name is empty!");
                return;
            }

            if (Main.TryCreate(typeName, out var e, out IStorageProvider provider))
            {
                SetStorageProvider(provider);
                return;
            }

            LogError("Could not instantiate storage provider (are you implementing IStorageProvider and have parameterless constructor?): " + e);
            LogError("No data will be read or saved!");
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
        /// <param name="player">The player to update the nickname of.</param>
        /// <param name="playerInfo">The player's <see cref="PlayerInfoWrapper"/>. Optional, only pass if you already have it, saves barely any time.</param>
        public static void UpdateNickname(XPPlayer player, PlayerInfoWrapper playerInfo = null)
        {
            EnsureStorageProviderValid();

            playerInfo ??= StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.PlayerId);
            playerInfo.PlayerInfo.Nickname = player.DisplayedName;
            StorageProvider.SetPlayerInfo(playerInfo);
            LogDebug("Updated nick of " + player.PlayerId + " to " + player.DisplayedName);
        }
#endif

        /// <summary>
        /// Adds XP to a player.
        /// </summary>
        /// <param name="player">The player to add XP to.</param>
        /// <param name="amount">The amount of XP to add.</param>
        /// <param name="force">Whether to force the addition of XP, even if <see cref="XPGainPaused"/> or the player has <see cref="XPPlayer.DNT"/> enabled.</param>
        /// <param name="playerInfo">The player's <see cref="PlayerInfoWrapper"/>. Optional, only pass if you already have it, saves barely any time.</param>
        /// <returns>Whether or not the XP was added.</returns>
        public static bool AddXP(XPPlayer player, int amount, bool force = false, PlayerInfoWrapper playerInfo = null)
        {
            if (amount == 0)
                return false;

            EnsureStorageProviderValid();

            if (!force && (XPGainPaused || player.DNT))
                return false;

            playerInfo ??= StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.PlayerId);
            int prevLevel = playerInfo.Level;

            playerInfo.PlayerInfo.XP += amount;
            StorageProvider.SetPlayerInfo(playerInfo);

            if (player.IsConnected && playerInfo.Level != prevLevel)
                HandleLevelUp(player, playerInfo);

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
        /// </summary>
        /// <param name="player">The player to display the message to.</param>
        /// <param name="message">The message to display.</param>
        /// <remarks>If <see cref="MessagingProvider"/> is null, it will not be displayed.</remarks>
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
        /// Respects role overrides, DNT, and <see cref="XPGainPaused"/>.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="key">The key of the <see cref="XPECFile"/>.</param>
        /// <param name="subkeys">The subkeys of the <see cref="XPECItem"/>.</param>
        /// <returns>Whether or not the XP was added and the message was sent (can be forced with
        /// <see cref="AddXP(XPSystem.API.XPPlayer,int,bool,XPSystem.API.StorageProviders.PlayerInfoWrapper)"/>
        /// and <see cref="DisplayMessage"/>).</returns>
        /// <remarks>Uses <see cref="XPECManager.GetItem(string, RoleTypeId, object[])"/>.</remarks>
        public static bool AddXPAndDisplayMessage(XPPlayer player, string key, params object[] subkeys)
        {
            return AddXPAndDisplayMessage(player, XPECManager.GetItem(key, player.Role, subkeys));
        }

        /// <summary>
        /// Tries to add XP to a player and display it's corresponding message.
        /// Respects <see cref="XPECLimitedDictFile{T}"/> limits, role overrides, DNT, and <see cref="XPGainPaused"/>.
        /// </summary>
        /// <inheritdoc cref="AddXPAndDisplayMessage(XPPlayer, string, object[])"/>
        /// <remarks>Uses <see cref="XPECManager.GetItem(string, RoleTypeId, object[])"/>.</remarks>
        public static bool TryAddXPAndDisplayMessage(XPPlayer player, string key, params object[] subkeys)
        {
            var file = XPECManager.GetFile(key, player.Role);
            if (file == null)
                return false;

            var item = file.Get(subkeys);
            if (item == null)
                return false;

            if (file is IXPECLimitedFile limitedFile)
            {
                if (!XPECLimitTracker.CanUse(item, limitedFile, player, true))
                    return false;
            }

            return AddXPAndDisplayMessage(player, item);
        }

        /// <summary>
        /// Adds XP to a player and displays it's corresponding message.
        /// Respects role overrides, DNT, and <see cref="XPGainPaused"/>.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="xpecItem">The <see cref="XPECItem"/> containing the amount and the message.</param>
        /// <returns>Whether or not the XP was added and the message was sent (can be forced with
        /// <see cref="AddXP(XPSystem.API.XPPlayer,int,bool,XPSystem.API.StorageProviders.PlayerInfoWrapper)"/>
        /// and <see cref="DisplayMessage"/>).</returns>
        public static bool AddXPAndDisplayMessage(XPPlayer player, XPECItem xpecItem)
        {
            if (xpecItem == null || xpecItem.Amount == 0 || player.DNT || XPGainPaused)
                return false;

            var playerInfo = StorageProvider.GetPlayerInfoAndCreateOfNotExist(player.PlayerId);

            AddXP(player, xpecItem.Amount, playerInfo: playerInfo);

            string message = xpecItem.Translation;
            if (Config.UseAddedXPTemplate)
            {
                message = Config.AddedXPTemplate
                    .Replace("%message%", message)
                    .Replace("%currentxp%", playerInfo.XP.ToString())
                    .Replace("%currentlevel%", playerInfo.Level.ToString())
                    .Replace("%neededxp%", playerInfo.NeededXP.ToString())
                    .Replace("%nextlevel%", (playerInfo.Level + 1).ToString());
            }

            DisplayMessage(player, message);
            return true;
        }
#endregion
#region Misc
        /// <summary>
        /// Handles a player leveling up.
        /// </summary>
        /// <param name="player">The player that leveled up.</param>
        /// <param name="wrapper">The <see cref="PlayerInfoWrapper"/> belonging to the player.</param>
        public static void HandleLevelUp(XPPlayer player, PlayerInfoWrapper wrapper)
        {
            DisplayProviders.RefreshOf(player);

            if (Config.ShowAddedLVL)
            {
                player.DisplayMessage(Config.AddedLVLMessage.Replace("%level%",
                    wrapper.Level.ToString()));
            }
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
        public static string FormatLeaderboard(IEnumerable<PlayerInfoWrapper> players)
        {
            var sb = StringBuilderPool.Shared.Rent();

            foreach (var playerInfo in players)
            {
#if STORENICKS
                sb.AppendLine(string.IsNullOrWhiteSpace(playerInfo.Nickname)
                    ? $"{playerInfo.Player.ToString()} - {playerInfo.XP} ({playerInfo.Level})"
                    : $"{playerInfo.Nickname} - {playerInfo.XP} ({playerInfo.Level})");
#else
                sb.AppendLine($"{playerInfo.Player.ToString()} - {playerInfo.XP} ({playerInfo.Level})");
#endif
            }

            return StringBuilderPool.Shared.ToStringReturn(sb);
        }

        /// <summary>
        /// Formats a type into a string, with it's generic arguments.
        /// </summary>
        /// <param name="type">The type to format.</param>
        /// <returns>The type, formatted into a string.</returns>
        public static string FormatType(Type type)
        {
            var sb = StringBuilderPool.Shared.Rent();
            sb.Append(type.FullName);
            if (type.IsGenericType)
            {
                sb.Append("<");
                foreach (var arg in type.GetGenericArguments())
                {
                    sb.Append(FormatType(arg));
                    sb.Append(", ");
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append(">");
            }

            return StringBuilderPool.Shared.ToStringReturn(sb);
        }
#endregion
    }
}
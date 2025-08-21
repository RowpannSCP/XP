// ReSharper disable MemberCanBePrivate.Global
namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using NorthwoodLib.Pools;
    using PlayerRoles;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.Enums;
    using XPSystem.API.Exceptions;
    using XPSystem.API.Player;
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool PluginEnabled { get; internal set; } = false;

        /// <summary>
        /// The plugin config, cast to get nwapi or exiled-specific config.
        /// </summary>
        public static Config Config = new UninitializedConfig();

        /// <summary>
        /// The display provider for the plugin.
        /// Null if (<see cref="XPSystem.API.Enums.DisplayMode.None"/> or uninitialized).
        /// </summary>
        public static IMessagingProvider? MessagingProvider;

        /// <summary>
        /// The storage provider for the plugin.
        /// Change it using <see cref="SetStorageProvider(IStorageProvider)"/>.
        /// </summary>
        public static IStorageProvider? StorageProvider { get; private set; }

        /// <summary>
        /// The XP display providers.
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
        public static bool XPGainPaused { get; set; } = false;

        /// <summary>
        /// Gets invoke when a player levels up.
        /// </summary>
        public static event Action<XPPlayer, int, int> PlayerLevelUp = delegate { };

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
        public static void SetStorageProvider(IStorageProvider? provider)
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
                StorageProvider = null;
                LogInfo("Storage provider set to null, no data will be read or saved!");
                return;
            }

            try
            {
                if (provider is StorageProvider storageProvider)
                    storageProvider.LoadConfig();

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

            if (Main.TryCreate(typeName, out Exception? e, out IStorageProvider? provider))
            {
                SetStorageProvider(provider);
                return;
            }

            LogError("Could not instantiate storage provider (are you implementing IStorageProvider and have parameterless constructor?): " + e);
            LogError("No data will be read or saved!");
        }

        /// <summary>
        /// Ensures that the storage provider is valid.
        /// </summary>
        /// <exception cref="StorageProviderInvalidException">Thrown if the storage provider has not been successfully set.</exception>
        public static void EnsureStorageProviderValid()
        {
            if (StorageProvider == null)
                throw new StorageProviderInvalidException();
        }

        /// <summary>
        /// Gets the player info of a player.
        /// Will create a new one if it doesn't exist.
        /// </summary>
        /// <param name="playerId">The <see cref="IPlayerId"/> of the player to get the info of.</param>
        /// <returns>The <see cref="PlayerInfoWrapper"/> belonging to the player.</returns>
        public static PlayerInfoWrapper GetPlayerInfo(IPlayerId playerId)
        {
            EnsureStorageProviderValid();
            return StorageProvider!.GetPlayerInfoAndCreateOfNotExist(playerId);
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
        public static void UpdateNickname(XPPlayer player, PlayerInfoWrapper? playerInfo = null)
        {
            playerInfo ??= GetPlayerInfo(player);
            playerInfo.PlayerInfo.Nickname = player.DisplayedName;

            StorageProvider!.SetPlayerInfo(playerInfo);
            LogDebug("Updated nick of " + player.PlayerId + " to " + player.DisplayedName);
        }
#endif

        /// <summary>
        /// Adds XP to a player.
        /// </summary>
        /// <param name="player">The player to add XP to.</param>
        /// <param name="amount">The amount of XP to add.</param>
        /// <param name="force">Whether to force the addition of XP, even if <see cref="XPGainPaused"/>.</param>
        /// <param name="playerInfo">The player's <see cref="PlayerInfoWrapper"/>. Optional, only pass if you already have it, saves barely any time.</param>
        /// <returns>Whether or not the player leveled up.</returns>
        public static bool AddXP(XPPlayer player, int amount, bool force = false, PlayerInfoWrapper? playerInfo = null)
        {
            if (amount == 0)
                return false;

            if (!force && XPGainPaused)
                return false;

            return AddXP(playerInfo ?? GetPlayerInfo(player.PlayerId), amount, player);
        }

        /// <summary>
        /// The method that actually adds XP to a player.
        /// Beware: No checks!
        /// </summary>
        internal static bool AddXP(PlayerInfoWrapper playerInfo, int amount, XPPlayer? player = null)
        {
            if (player == null)
                XPPlayer.TryGetXP(playerInfo.Player, out player);

            int prevLevel = playerInfo.Level;
            float floatAmount = amount;
            bool connected = player != null;

            if (amount > 0 || Config.XPMultiplierForXPLoss)
            {
                if (player?.XPMultiplier != null)
                    floatAmount *= player.XPMultiplier;

                if (connected || Config.GlobalXPMultiplierForNonOnline)
                    floatAmount *= Config.GlobalXPMultiplier;
            }

            amount = (int)floatAmount;

            playerInfo.PlayerInfo.XP += amount;
            StorageProvider!.SetPlayerInfo(playerInfo);

            if (playerInfo.Level == prevLevel)
                return false;

            if (connected)
                HandleLevelUp(player!, playerInfo, prevLevel);

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
        public static void DisplayMessage(BaseXPPlayer player, string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            MessagingProvider?.DisplayMessage(player, Config.TextPrefix + message + Config.TextSuffix, Config.DisplayDuration);
        }
#endregion
#region Mixed
        /// <summary>
        /// Tries to add XP to a player and display it's corresponding message.
        /// Respects <see cref="XPECLimitedDictFile{T}"/> limits, role overrides, and <see cref="XPGainPaused"/>.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="key">The key of the <see cref="XPECFile"/>.</param>
        /// <param name="subkeys">The subkeys of the <see cref="XPECItem"/>.</param>
        /// <returns>Whether or not the XP was added.</returns>
        /// <remarks>Uses <see cref="XPECManager.GetItem(string, RoleTypeId, object[])"/>.</remarks>
        public static bool TryAddXPAndDisplayMessage(XPPlayer? player, string key, params object?[] subkeys)
        {
            if (player is not { IsConnected: true })
                return false;

            XPECFile? file = XPECManager.GetFile(key, player.Role);
            XPECItem? item = file?.Get(subkeys);
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
        /// Respects role overrides and <see cref="XPGainPaused"/>.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="xpecItem">The <see cref="XPECItem"/> containing the amount and the message.</param>
        /// <returns>Whether or not the XP was added.</returns>
        public static bool AddXPAndDisplayMessage(XPPlayer player, XPECItem? xpecItem)
        {
            if (xpecItem == null)
                return false;

            return AddXPAndDisplayMessage(player, xpecItem.Amount, xpecItem.Translation);
        }

        /// <summary>
        /// Adds XP to a player and displays it's corresponding message.
        /// </summary>
        /// <param name="player">The player to affect.</param>
        /// <param name="amount">The amount of XP to add.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="force">Whether to force the addition of XP, even if amount = 0 or <see cref="XPGainPaused"/>.</param>
        /// <returns>Whether or not XP was added.</returns>
        public static bool AddXPAndDisplayMessage(XPPlayer player, int amount, string? message, bool force = false)
        {
            if (amount == 0 && !force || XPGainPaused)
                return false;

            PlayerInfoWrapper playerInfo = GetPlayerInfo(player.PlayerId);
            bool levelup = AddXP(player, amount, force: true, playerInfo: playerInfo);

            if (levelup && !Config.ShowXPOnLevelUp)
                return true;

            if (!string.IsNullOrWhiteSpace(message))
            {
                if (Config.UseAddedXPTemplate)
                    message = FormatMessage(message!, playerInfo);
                DisplayMessage(player, message);
            }

            return true;
        }

        /// <summary>
        /// Formats a message according to the <see cref="Config.AddedXPTemplate"/> for a player.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="playerInfo">The <see cref="PlayerInfoWrapper"/> of the player to format the message for.</param>
        /// <returns>The formatted message.</returns>
        public static string FormatMessage(string message, PlayerInfoWrapper playerInfo)
        {
            message = Config.AddedXPTemplate
                    .Replace("%message%", message)
                    .Replace("%currentlevel%", playerInfo.Level.ToString())
                    .Replace("%nextlevel%", (playerInfo.Level + 1).ToString());

            message = message
                .Replace("%currentxp%",
                    (Config.UseTotalXP
                        ? playerInfo.XP
                        : playerInfo.XP - playerInfo.NeededXPCurrent)
                    .ToString())
                .Replace("%neededxp%",
                    (Config.UseTotalXP
                        ? playerInfo.NeededXPNext
                        : playerInfo.NeededXPNext - playerInfo.NeededXPCurrent)
                    .ToString());

            if (Config.AddedXPTemplate.Contains("%progressbarfilled%") ||
                Config.AddedXPTemplate.Contains("%progressbarremaining%"))
            {
                if (string.IsNullOrEmpty(Config.AddedXPProgressBarChars) || Config.AddedXPProgressBarChars.Length < 2)
                {
                    LogWarn("Template contains progress bar, but no/not enough characters are set for it!");
                    message = message
                        .Replace("%progressbarfilled%", string.Empty)
                        .Replace("%progressbarremaining%", string.Empty);
                }
                else
                {
                    char filledChar = Config.AddedXPProgressBarChars[0];
                    char remainingChar = Config.AddedXPProgressBarChars[1];
                    double fillPercentage = (double)(playerInfo.XP - playerInfo.NeededXPCurrent) /
                                            (playerInfo.NeededXPNext - playerInfo.NeededXPCurrent);
                    int fill = (int)(Config.AddedXPProgressBarLength * fillPercentage);

                    message = message
                        .Replace("%progressbarfilled%",
                            new string(filledChar, fill))
                        .Replace("%progressbarremaining%",
                            new string(remainingChar, Config.AddedXPProgressBarLength - fill));
                }
            }

            return message;
        }
#endregion
#region Misc
        /// <summary>
        /// Handles a player leveling up.
        /// </summary>
        /// <param name="player">The player that leveled up.</param>
        /// <param name="wrapper">The <see cref="PlayerInfoWrapper"/> belonging to the player.</param>
        /// <param name="prevLevel">The previous level the player had.</param>
        public static void HandleLevelUp(XPPlayer player, PlayerInfoWrapper wrapper, int prevLevel)
        {
            DisplayProviders.RefreshOf(player);

            if (Config.ShowAddedLVL)
            {
                player.DisplayMessage(Config.AddedLVLMessage.Replace("%level%",
                    wrapper.Level.ToString()));
            }

            PlayerLevelUp.Invoke(player, wrapper.Level, prevLevel);
        }

        /// <summary>
        /// Attempts to create a <see cref="IPlayerId"/> from an id and an <see cref="AuthType"/>.
        /// </summary>
        /// <param name="id">The id value.</param>
        /// <param name="authType">The <see cref="AuthType"/> of the id.</param>
        /// <returns>If successful, the <see cref="IPlayerId"/> created. Otherwise, null.</returns>
        public static IPlayerId? CreateUserId(object id, AuthType authType)
        {
            bool EnsureIs<T>([NotNullWhen(true)] out T? obj)
            {
                if (id is T t)
                {
                    obj = t;
                    return true;
                }

                object converted = Convert.ChangeType(id, typeof(T));
                if (converted is T t2)
                {
                    obj = t2;
                    return true;
                }

                obj = default;
                return false;
            }

            switch (authType)
            {
                case AuthType.Steam:
                case AuthType.Discord:
                    if (EnsureIs(out ulong ulongId))
                        return new NumberPlayerId(ulongId, authType);

                    LogDebug("UserId creating failed (not ulong)");
                    return null;
                case AuthType.Northwood:
                    if (EnsureIs(out string? stringId))
                        return new StringPlayerId(stringId, authType);

                    LogDebug("UserId creating failed (not string)");
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Attempts to parse a string into a <see cref="IPlayerId"/>.
        /// </summary>
        /// <param name="string">The string to parse.</param>
        /// <param name="playerId">The equivalent <see cref="IPlayerId"/>.</param>
        /// <returns>Whether or not the parsing was successful.</returns>
        public static bool TryParseUserId(string? @string, [NotNullWhen(true)] out IPlayerId? playerId)
        {
            playerId = null;
            if (@string == null)
                return false;

            string[] split = @string.Split('@');
            if (split.Length != 2)
            {
                LogDebug("Failed to parse UserId (length != 2)");
                return false;
            }

            string authTypeString = split[1];
            if (!Enum.TryParse(authTypeString, true, out AuthType authType))
            {
                LogDebug("Failed to parse UserId (unknown authType):" + authTypeString);
                return false;
            }

            playerId = CreateUserId(split[0], authType);
            return playerId != null;
        }

        /// <summary>
        /// Formats the leaderboard entries into a string.
        /// </summary>
        /// <param name="players">The player data to format.</param>
        /// <returns>The formatted leaderboard, as a string..</returns>
        public static string FormatLeaderboard(IEnumerable<PlayerInfoWrapper> players)
        {
            StringBuilder sb = StringBuilderPool.Shared.Rent();

            foreach (PlayerInfoWrapper playerInfo in players)
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
            StringBuilder sb = StringBuilderPool.Shared.Rent();
            sb.Append(type.FullName);
            if (type.IsGenericType)
            {
                sb.Append("<");
                foreach (Type arg in type.GetGenericArguments())
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
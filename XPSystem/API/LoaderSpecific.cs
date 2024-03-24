namespace XPSystem.API
{
    using System.IO;
    using CommandSystem;
    using Exiled.Loader.Features.Configs.CustomConverters;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Things that change depending on the loader.
    /// </summary>
    public static class LoaderSpecific
    {
        /// <summary>
        /// Gets the path for XPSystem's configs.
        /// </summary>
        public static string ConfigPath =>
#if EXILED
            Path.Combine(Exiled.API.Features.Paths.Configs, "XPSystem");
#else
            Path.Combine(PluginAPI.Helpers.Paths.LocalPlugins.Plugins, "XPSystem");
#endif

        public static string LegacyDefaultDatabasePath =>
            Path.Combine(
#if EXILED
                Exiled.API.Features.Paths.Configs, 
#else
                Path.Combine(PluginAPI.Helpers.Paths.LocalPlugins.Plugins, "XPSystem"),
#endif
                "Players.db");

        /// <summary>
        /// Gets a <see cref="ReferenceHub"/> from a string.
        /// </summary>
        /// <param name="data">The player's nickname, user ID, or player ID.</param>
        /// <returns>The <see cref="ReferenceHub"/> if found, otherwise null.</returns>
        public static ReferenceHub GetHub(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;
#if EXILED
            return Exiled.API.Features.Player.Get(data).ReferenceHub;
#else
            if (uint.TryParse(data, out var networkId))
                return PluginAPI.Core.Player.Get(networkId).ReferenceHub;
            if (int.TryParse(data, out var playerId))
                return PluginAPI.Core.Player.Get(playerId).ReferenceHub;
            return PluginAPI.Core.Player.GetByName(data).ReferenceHub;
#endif
        }

        /// <summary>
        /// Checks if a sender has a permission.
        /// </summary>
        /// <param name="sender">The sender to check.</param>
        /// <param name="permission">The permission to check for.</param>
        /// <returns>Whether or not the sender has the permission.</returns>
        public static bool CheckPermission(ICommandSender sender, string permission)
        {
#if EXILED
            return Exiled.Permissions.Extensions.Permissions.CheckPermission(sender, permission);
#else
            return NWAPIPermissionSystem.PermissionHandler.CheckPermission(sender, permission);
#endif

        }

        /// <summary>
        /// Checks if a players has a permission.
        /// </summary>
        /// <param name="hub">The player to check.</param>
        /// <param name="permission">The permission to check for.</param>
        /// <returns>Whether or not the player has the permission.</returns>
        public static bool CheckPermission(ReferenceHub hub, string permission)
        {
#if EXILED
            return Exiled.Permissions.Extensions.Permissions.CheckPermission(
                Exiled.API.Features.Player.Get(hub)
                , permission);
#else
            return NWAPIPermissionSystem.PermissionHandler
                .CheckPermission(hub.authManager.UserId, permission);
#endif
        }

        public static void LogDebug(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Debug(message);
#else
            if (XPAPI.Config.Debug)
                PluginAPI.Core.Log.Debug(message);
#endif
        }

        public static void LogInfo(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Info(message);
#else
            PluginAPI.Core.Log.Info(message);
#endif
        }

        public static void LogWarn(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Warn(message);
#else
            PluginAPI.Core.Log.Warning(message);
#endif
        }

        public static void LogError(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Error(message);
#else
            PluginAPI.Core.Log.Error(message);
#endif
        }

        /// <summary>
        /// Adds loader-specific type converters to this <see cref="SerializerBuilder"/>.
        /// </summary>
        public static SerializerBuilder WithLoaderTypeConverters(this SerializerBuilder serializerBuilder) =>
#if EXILED
            serializerBuilder.WithTypeConverter(new VectorsConverter())
                .WithTypeConverter(new ColorConverter())
                .WithTypeConverter(new AttachmentIdentifiersConverter());
#else
            serializerBuilder;
#endif

        /// <summary>
        /// Adds loader-specific type converters to this <see cref="DeserializerBuilder"/>.
        /// </summary>
        public static DeserializerBuilder WithLoaderTypeConverters(this DeserializerBuilder deserializerBuilder) =>
#if EXILED
            deserializerBuilder.WithTypeConverter(new VectorsConverter())
                .WithTypeConverter(new ColorConverter())
                .WithTypeConverter(new AttachmentIdentifiersConverter());
#else
            deserializerBuilder;
#endif
    }
}
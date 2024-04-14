namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CommandSystem;
    using Mirror;
    using PlayerRoles;
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
            serializerBuilder.WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.VectorsConverter())
                .WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.ColorConverter())
                .WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.AttachmentIdentifiersConverter());
#else
            serializerBuilder;
#endif

        /// <summary>
        /// Adds loader-specific type converters to this <see cref="DeserializerBuilder"/>.
        /// </summary>
        public static DeserializerBuilder WithLoaderTypeConverters(this DeserializerBuilder deserializerBuilder) =>
#if EXILED
            deserializerBuilder.WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.VectorsConverter())
                .WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.ColorConverter())
                .WithTypeConverter(new Exiled.Loader.Features.Configs.CustomConverters.AttachmentIdentifiersConverter());
#else
            deserializerBuilder;
#endif

#region Exiled Mirror Dicts (Source: https://github.com/Exiled-Team/EXILED/blob/dev/Exiled.API/Extensions/MirrorExtensions.cs)
#if !EXILED
        private static readonly Dictionary<Type, MethodInfo> WriterExtensionsValue = new();
        private static readonly Dictionary<string, ulong> SyncVarDirtyBitsValue = new();
        private static readonly ReadOnlyDictionary<Type, MethodInfo> ReadOnlyWriterExtensionsValue = new(WriterExtensionsValue);
        private static readonly ReadOnlyDictionary<string, ulong> ReadOnlySyncVarDirtyBitsValue = new(SyncVarDirtyBitsValue);
        private static MethodInfo setDirtyBitsMethodInfoValue;
#endif

        /// <summary>
        /// Gets <see cref="MethodInfo"/> corresponding to <see cref="Type"/>.
        /// </summary>
        public static ReadOnlyDictionary<Type, MethodInfo> WriterExtensions
        {
            get
            {
#if EXILED
                return Exiled.API.Extensions.MirrorExtensions.WriterExtensions;
#else
                if (WriterExtensionsValue.Count == 0)
                {
                    foreach (var method in typeof(NetworkWriterExtensions)
                                 .GetMethods()
                                 .Where(x => !x.IsGenericMethod
                                             && x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null
                                             && (x.GetParameters()?.Length == 2)))
                    {
                        WriterExtensionsValue.Add(
                            method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType,
                            method);
                    }

                    var nonoword = Assembly.GetAssembly(typeof(RoleTypeId)).GetType("Mirror.GeneratedNetworkCode");
                    foreach (var method in nonoword.GetMethods()
                                 .Where(x => !x.IsGenericMethod
                                             && (x.GetParameters()?.Length == 2)
                                             && (x.ReturnType == typeof(void))))
                    {
                        WriterExtensionsValue.Add(
                            method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType,
                            method);
                    }

                    foreach (var serializer in typeof(ServerConsole).Assembly
                                 .GetTypes()
                                 .Where(x => x.Name.EndsWith("Serializer")))
                    {
                        foreach (var method in serializer.
                                     GetMethods()
                                     .Where(x => (x.ReturnType == typeof(void))
                                     && x.Name.StartsWith("Write")))
                        {
                            WriterExtensionsValue.Add(
                                method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter))
                                    .ParameterType, method);
                        }
                    }
                }

                return ReadOnlyWriterExtensionsValue;
#endif
            }
        }

        /// <summary>
        /// Gets a all DirtyBit <see cref="ulong"/> from something(idk) (format:classname.methodname).
        /// </summary>
        public static ReadOnlyDictionary<string, ulong> SyncVarDirtyBits
        {
            get
            {
#if EXILED
                return Exiled.API.Extensions.MirrorExtensions.SyncVarDirtyBits;
#else
                if (SyncVarDirtyBitsValue.Count == 0)
                {
                    foreach (var property in typeof(ServerConsole).Assembly.GetTypes()
                        .SelectMany(x => x.GetProperties())
                        .Where(m => m.Name.StartsWith("Network")))
                    {
                        var setMethod = property.GetSetMethod();

                        if (setMethod is null)
                            continue;

                        var methodBody = setMethod.GetMethodBody();

                        if (methodBody is null)
                            continue;

                        byte[] bytecodes = methodBody.GetILAsByteArray();

                        string key = $"{property.ReflectedType!.Name}.{property.Name}";
                        if (!SyncVarDirtyBitsValue.ContainsKey(key))
                            SyncVarDirtyBitsValue.Add(key, bytecodes[bytecodes.LastIndexOf((byte)OpCodes.Ldc_I8.Value) + 1]);
                    }
                }

                return ReadOnlySyncVarDirtyBitsValue;
#endif
            }
        }

        /// <summary>
        /// Gets a <see cref="NetworkBehaviour.SetSyncVarDirtyBit(ulong)"/>'s <see cref="MethodInfo"/>.
        /// </summary>
        public static MethodInfo SetDirtyBitsMethodInfo =>
#if EXILED
            Exiled.API.Extensions.MirrorExtensions.SetDirtyBitsMethodInfo;
#else
            setDirtyBitsMethodInfoValue ??= typeof(NetworkBehaviour).GetMethod(nameof(NetworkBehaviour.SetSyncVarDirtyBit));
#endif
#endregion
    }
}
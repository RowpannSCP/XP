namespace XPSystem.API
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CommandSystem;
    using InventorySystem;
    using InventorySystem.Items;
    using Mirror;
    using PlayerRoles;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Things that change depending on the loader.
    /// </summary>
    public static class LoaderSpecific
    {
        /// <summary>
        /// Gets the path for the plugin configs.
        /// </summary>
        public static string ConfigPath =>
#if EXILED
            Exiled.API.Features.Paths.Configs;
#else
            LabApi.Loader.Features.Paths.PathManager.Configs.FullName;
#endif

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
            return Exiled.API.Features.Player.Get(data)?.ReferenceHub;
#else
            if (uint.TryParse(data, out uint networkId)
                && LabApi.Features.Wrappers.Player.TryGet(networkId, out var player))
                return player.ReferenceHub;

            if (int.TryParse(data, out int playerId)
                && LabApi.Features.Wrappers.Player.TryGet(playerId, out player))
                return player.ReferenceHub;

            if (LabApi.Features.Wrappers.Player.TryGet(data, out player))
                return player.ReferenceHub;

            LabApi.Features.Wrappers.Player.TryGetPlayersByName(data, out var list);
            return list.FirstOrDefault()?.ReferenceHub;
#endif
        }

        /// <summary>
        /// Gets the <see cref="ItemCategory"/> of an <see cref="ItemType"/>.
        /// </summary>
        /// <param name="type">The <see cref="ItemType"/> to get the category of.</param>
        /// <returns>The <see cref="ItemCategory"/> of the <see cref="ItemType"/>.</returns>
        public static ItemCategory GetCategory(ItemType type)
        {
#if EXILED
            return Exiled.API.Extensions.ItemExtensions.GetCategory(type);
#else
            InventoryItemLoader.TryGetItem(type, out ItemBase item);
            return item.Category;
#endif
        }

        /// <summary>
        /// Gets the corresponding <see cref="RoundSummary.LeadingTeam"/> from a <see cref="Team"/>.
        /// </summary>
        /// <param name="team">The <see cref="Team"/> to get the leading team from.</param>
        /// <returns>The corresponding <see cref="RoundSummary.LeadingTeam"/>.</returns>
        public static RoundSummary.LeadingTeam GetLeadingTeam(this Team team) =>
#if EXILED
            (RoundSummary.LeadingTeam)Exiled.API.Extensions.RoleExtensions.GetLeadingTeam(team);
#else
            team switch
        {
            Team.ClassD or Team.ChaosInsurgency => RoundSummary.LeadingTeam.ChaosInsurgency,
            Team.FoundationForces or Team.Scientists => RoundSummary.LeadingTeam.FacilityForces,
            Team.SCPs => RoundSummary.LeadingTeam.Anomalies,
            _ => RoundSummary.LeadingTeam.Draw,
        };
#endif

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
            return LabApi.Features.Permissions.PermissionsExtensions.HasPermissions(sender, permission);
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
            return LabApi.Features.Permissions.PermissionsManager
                .HasPermissions(LabApi.Features.Wrappers.Player.Get(hub), permission);
#endif
        }

        /// <summary>
        /// Checks whether or not a player is a NPC.
        /// </summary>
        /// <param name="hub">The player to check.</param>
        /// <returns>Whether or not the player is a NPC.</returns>
        public static bool CheckNPC(ReferenceHub hub) =>
#if EXILED
            Exiled.API.Features.Player.Get(hub)?.IsNPC == true;
#else
            // ReSharper disable once ConstantConditionalAccessQualifier
            LabApi.Features.Wrappers.Player.Get(hub)?.IsNpc == true;
#endif

        public static void LogDebug(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Debug(message);
#else
            if (XPAPI.Config.Debug)
                LabApi.Features.Console.Logger.Debug(message);
#endif
        }

        public static void LogInfo(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Info(message);
#else
            LabApi.Features.Console.Logger.Info(message);
#endif
        }

        public static void LogWarn(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Warn(message);
#else
            LabApi.Features.Console.Logger.Warn(message);
#endif
        }

        public static void LogError(string message)
        {
#if EXILED
            Exiled.API.Features.Log.Error(message);
#else
            LabApi.Features.Console.Logger.Error(message);
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
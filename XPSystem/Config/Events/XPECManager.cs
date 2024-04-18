namespace XPSystem.Config.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.Config.Events.Types;
    using static API.LoaderSpecific;

    /// <summary>
    /// XP Event Config Manager.
    /// </summary>
    public static class XPECManager
    {
        public static readonly XPECFileCollection NeededFiles = new XPECFileCollection()
        {
            [""] = new XPECItemFile()
            {
                
            }
        }.AsReadonly();

        public static XPECFileCollection Default;
        public static readonly Dictionary<RoleTypeId, XPECFileCollection> Overrides = new();

        private static readonly RoleTypeId[] _skipRoles = new[]
        {
            RoleTypeId.None,
            RoleTypeId.Filmmaker,
            RoleTypeId.Overwatch
        };

        public static bool LogKeys = false;
        public static Dictionary<string, int> KeyUsage = new();

        /// <summary>
        /// Gets the override <see cref="XPECFileCollection"/> for the specified role.
        /// </summary>
        /// <param name="role">The role to get the collection for.</param>
        /// <returns>The override collection for the role.</returns>
        public static XPECFileCollection GetCollection(RoleTypeId role = RoleTypeId.None)
        {
            if (role == RoleTypeId.None || !Overrides.TryGetValue(role, out var collection))
                collection = Default;

            return collection;
        }

        /// <summary>
        /// Gets the <see cref="XPECFile"/> with the specified key.
        /// </summary>
        /// <param name="key">The key of the file.</param>
        /// <param name="role">The role to check for overrides from.</param>
        /// <returns></returns>
        public static XPECFile GetFile(string key, RoleTypeId role = RoleTypeId.None)
        {
            if (role != RoleTypeId.None && Overrides.TryGetValue(role, out var collection))
            {
                if (collection.Files.TryGetValue(key, out var file))
                    return file;
            }

            return Default.Files.TryGetValue(key, out var defaultFile)
                ? defaultFile
                : null;
        }

        /// <summary>
        /// Gets the <see cref="XPECFile"/> with the specified key and casts it to the specified type.
        /// </summary>
        /// <param name="key">The key of the file.</param>
        /// <param name="role">The role to check for overrides for.</param>
        /// <typeparam name="T">The type to cast the file to.</typeparam>
        /// <returns>The file, if found, otherwise null.</returns>
        /// <exception cref="InvalidCastException">Thrown if the file is not of the specified type.</exception>
        public static T GetFile<T>(string key, RoleTypeId role = RoleTypeId.None) where T : XPECFile
        {
            var file = GetFile(key, role);
            return file switch
            {
                null => null,
                T casted => casted,
                _ => throw new InvalidCastException(
                    $"File {key} has wrong type (was: {file.GetType().FormatType()}, should: {typeof(T).FormatType()})!")
            };
        }

        /// <summary>
        /// Gets the <see cref="XPECItem"/> with the specified key and subkeys.
        /// </summary>
        /// <param name="key">The key of the file.</param>
        /// <param name="role">The role to check for overrides for.</param>
        /// <param name="subkeys">The (optional) subkeys of the item.</param>
        /// <returns>The item, if found, otherwise null.</returns>
        public static XPECItem GetItem(string key, RoleTypeId role = RoleTypeId.None, params object[] subkeys)
        {
            var file = GetFile(key, role);
            var item = file?.Get(subkeys);

            if (item != null && LogKeys)
            {
                string fullKey = $"{key}/{string.Join("/", subkeys ?? Array.Empty<object>())}";
                KeyUsage[fullKey] = KeyUsage.TryGetValue(fullKey, out int count)
                        ? count + 1
                        : 1;
            }

            return item;
        }

        public static void Load(string dir)
        {
            var values = Enum.GetValues(typeof(RoleTypeId))
                .Cast<RoleTypeId>()
                .Where(x => !_skipRoles.Contains(x));

            Default = LoadInternal(Path.Combine(dir, "default"), true);
            foreach (var role in values)
            {
                Overrides[role] = LoadInternal(Path.Combine(dir, role.ToString()));
            }
        }

        private static XPECFileCollection LoadInternal(string dir, bool isNeeded = false)
        {
            var collection = new XPECFileCollection();
            string dirFormatted = dir.Replace("\\", "/");

            foreach (string file in Directory.GetFiles(dir, "*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    string data = File.ReadAllText(file);
                    var deserialized = XPAPI.Deserializer.Deserialize<XPECFile>(data);
                    string key = file
                        .Replace("\\", "/")
                        .Replace(dirFormatted, "")
                        .Replace(".yml", "");

                    collection.Files.Add(key, deserialized);
                }
                catch (Exception e)
                {
                    LogError($"Failed to load XPEC file {file}: {e}");
                }
            }

            if (isNeeded)
            {
                foreach (var needed in NeededFiles)
                {
                    if (collection.Files.TryGetValue(needed.Key, out var found))
                    {
                        if (!found.IsEqualType(needed.Value))
                        {
                            LogError(
                                $"XPEC file {needed.Key} has wrong type (was: {found.GetType().FormatType()}, should: {needed.Value.GetType().FormatType()})!");
                        }

                        continue;
                    }

                    collection.Files.Add(needed);
                    File.WriteAllText(GetFilePath(needed.Key, dir), XPAPI.Serializer.Serialize(needed));

                    LogInfo($"Created missing XPEC file {needed.Key}");
                }
            }

            return collection;
        }

        private static string GetFilePath(string key, string dir) =>
            Path.Combine(
                dir,
                key.Replace('/', Path.DirectorySeparatorChar) + ".yml"
                );
    }
}
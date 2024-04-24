namespace XPSystem.Config.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.Config.Events.Types;
    using static API.XPAPI;

    /// <summary>
    /// XP Event Config Manager.
    /// </summary>
    public static class XPECManager
    {
        public static readonly XPECFileCollection NeededFiles = new XPECFileCollection()
        {
            #error
            ["kills"] = new XPECDictFile<RoleTypeId>()
            {
                Default = new XPECItem()
                {
                    Amount = 30,
                    Translation = "You got a kill!"
                },
                Items = new ()
                {
                    [RoleTypeId.Scp049] = new ()
                    {
                        Amount = 60,
                        Translation = "You killed SCP 049!"
                    }
                }
            },
            #error
            ["spawn"] = new XPECDictFile<RoleTypeId>()
            {
                Default = new XPECItem()
                {
                    Amount = 5,
                    Translation = "You spawned!"
                },
                Items = new ()
                {
                    [RoleTypeId.Scp173] = new ()
                    {
                        Amount = 10,
                        Translation = "You spawned as 173!"
                    }
                }
            },
            #error
            ["upgrade"] = new XPECDictFile<ItemCategory>()
            {
                Default = new XPECItem()
                {
                    Amount = 5,
                    Translation = "You upgraded an item!"
                },
                Items = new ()
                {
                    [ItemCategory.MicroHID] = new ()
                    {
                        Amount = 50,
                        Translation = "You upgraded the micro!"
                    }
                }
            },
            #error
            ["pickup"] = new XPECDictFile<ItemCategory>()
            {
                Default = new XPECItem()
                {
                    Amount = 1,
                    Translation = "You picked up an item!"
                },
                Items = new ()
                {
                    [ItemCategory.MicroHID] = new ()
                    {
                        Amount = 30,
                        Translation = "You picked up the micro!"
                    }
                }
            },
            #error
            ["drop"] = new XPECDictFile<ItemType>()
            {
                Default = new XPECItem()
                {
                    Amount = 1,
                    Translation = "You dropped an item!"
                },
                Items = new ()
                {
                    [ItemType.MicroHID] = new ()
                    {
                        Amount = 10,
                        Translation = "You dropped the micro!"
                    }
                }
            },
            #error
            ["use"] = new XPECDictFile<ItemType>()
            {
                Default = new XPECItem()
                {
                    Amount = 1,
                    Translation = "You used an item!"
                },
                Items = new ()
                {
                    [ItemType.Medkit] = new ()
                    {
                        Amount = 5,
                        Translation = "You used a medkit!"
                    }
                }
            },
            #error
            ["escape"] = new XPECItemFile()
            {
                Item = new ()
                {
                    Amount = 50,
                    Translation = "You escaped!"
                }
            },
            #error
            ["win"] = new XPECItemFile()
            {
                Item = new ()
                {
                    Amount = 100,
                    Translation = "Your team won!"
                }
            },
#if EXILED
            #error
            ["door"] = new XPECDictFile<Exiled.API.Enums.DoorType>()
            {
                Default = new XPECItem()
                {
                    Amount = 0,
                    Translation = "You opened a door!"
                },
                Items = new ()
                {
                    [Exiled.API.Enums.DoorType.GateA] = new ()
                    {
                        Amount = 5,
                        Translation = "You opened gate A!"
                    }
                }
            },
            #error
            ["throw"] = new XPECDictFile<Exiled.API.Enums.ProjectileType>()
            {
                Default = new XPECItem()
                {
                    Amount = 3,
                    Translation = "You threw a grenade!"
                },
                Items = new ()
                {
                    [Exiled.API.Enums.ProjectileType.FragGrenade] = new ()
                    {
                        Amount = 5,
                        Translation = "You threw a frag grenade!"
                    }
                }
            },
#endif
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
                    var deserialized = Deserializer.Deserialize<XPECFile>(data);
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
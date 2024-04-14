namespace XPSystem.Config.Events
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using PlayerRoles;
    using XPSystem.API;
    using XPSystem.Config.Events.Models;
    using static API.LoaderSpecific;

    /// <summary>
    /// XP Event Config Manager.
    /// </summary>
    public static class XPECManager
    {
        public static readonly IReadOnlyCollection<IXPECFile> NeededFiles = new List<IXPECFile>
        {
            new XPECFile<RoleTypeId>()
            {
                Key = "",
                Default = new XPECItem()
                {
                    Amount = 0,
                    Translation = ""
                },
            }
        }.AsReadOnly();

        public static XPECFileCollection Default;
        public static readonly Dictionary<RoleTypeId, XPECFileCollection> Overrides = new();

        private static readonly RoleTypeId[] _skipRoles = new[]
        {
            RoleTypeId.None,
            RoleTypeId.Filmmaker,
            RoleTypeId.Overwatch
        };

        public static void Load(string dir)
        {
            var values = Enum.GetValues(typeof(RoleTypeId))
                .Cast<RoleTypeId>()
                .Where(x => !_skipRoles.Contains(x))
                .Select(x => x.ToString())
                .Append("default");

            foreach (var value in values)
            {
                
            }
        }

        private static XPECFileCollection LoadInternal(string dir)
        {
            Files.Clear();
            List<IXPECFile> read = new();
            var dirFormatted = dir.Replace("\\", "/");

            foreach (var file in Directory.GetFiles(dir, "*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    var data = File.ReadAllText(file);
                    var deserialized = XPAPI.Deserializer.Deserialize<IXPECFile>(data);
                    deserialized.Key = file
                        .Replace("\\", "/")
                        .Replace(dirFormatted, "")
                        .Replace(".yml", "");

                    read.Add(deserialized);
                }
                catch (Exception e)
                {
                    LogError($"Failed to load XPEC file {file}: {e}");
                }
            }

            foreach (var needed in NeededFiles)
            {
                var found = read.FirstOrDefault(x => x.Key == needed.Key);
                if (found != null)
                {
                    if (found.GetSubkeyType() != needed.GetSubkeyType())
                        LogError($"XPEC file {needed.Key} has wrong subkey type!");

                    continue;
                }

                read.Add(needed);
                File.WriteAllText(GetFilePath(needed, dir), XPAPI.Serializer.Serialize(needed));

                LogInfo($"Created missing XPEC file {needed.Key}");
            }

            Files.AddRange(read);
        }

        private static string GetFilePath(IXPECFile file, string dir) =>
            Path.Combine(
                dir,
                file.Key.Replace('/', Path.DirectorySeparatorChar) + ".yml"
                );
    }
}
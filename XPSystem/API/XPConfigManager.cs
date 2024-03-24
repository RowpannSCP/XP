namespace XPSystem.API.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Exiled.Loader.Features.Configs;
    using XPSystem.Config;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;
    using static LoaderSpecific;
    using ConfigsDict = System.Collections.Generic.Dictionary<string, XPSystem.Config.IXPConfigCollection>;

    public static class XPConfigManager
    {
        /// <summary>
        /// Gets or sets the serializer for configs and translations.
        /// </summary>
        public static ISerializer Serializer { get; set; } = new SerializerBuilder()
            .WithLoaderTypeConverters()
            .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreFields()
            .Build();

        /// <summary>
        /// Gets or sets the deserializer for configs and translations.
        /// </summary>
        public static IDeserializer Deserializer { get; set; } = new DeserializerBuilder()
            .WithLoaderTypeConverters()
            .WithTypeConverter(new XPConfigCollectionYamlTypeConverter())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), 
                deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

        /// <summary>
        /// The XP config for the plugin.
        /// </summary>
        public static ConfigsDict XPConfigs = new()
        {
            { "", new XPConfigCollection<string>("") }
        };

        /// <summary>
        /// Loads <see cref="ConfigsDict"/> and writes missing entries from and into the file.
        /// </summary>
        /// <param name="translationsFS">The file's <see cref="FileStream"/>.</param>
        public static void LoadConfigs(FileStream translationsFS)
        {
            bool changed = false;
            ConfigsDict readDict;

            using (var sr = new StreamReader(translationsFS))
            {
                try
                {
                    readDict = Deserializer.Deserialize<ConfigsDict>(sr.ReadToEnd());
                    var usedKeys = new List<string>();

                    foreach (var kvp in XPConfigs)
                    {
                        if (!readDict.TryGetValue(kvp.Key, out var collection))
                        {
                            changed = true;
                            readDict.Add(kvp.Key, kvp.Value);
                            LogWarn($"Missing config with key: {kvp.Key}, setting to fallback (default).");
                            continue;
                        }

                        if (collection == null)
                        {
                            changed = true;
                            readDict[kvp.Key] = kvp.Value;
                            LogWarn($"Config with key: {kvp.Key} is null, setting to fallback (default).");
                            continue;
                        }

                        if (!kvp.Value.CheckType(collection))
                        {
                            changed = true;
                            readDict[kvp.Key] = kvp.Value;
                            LogWarn($"Config with key: {kvp.Key} has wrong type, setting to fallback (default).");
                            continue;
                        }

                        kvp.Value.SetValues(collection);
                        usedKeys.Add(kvp.Key);
                    }

                    int applied = 0;
                    foreach (var kvp in readDict)
                    {
                        if (usedKeys.Contains(kvp.Key))
                            continue;
                        XPConfigs.Add(kvp.Key, kvp.Value);
                        applied++;
                    }

                    LogDebug($"Applied {applied} configs not used by the main plugin.");
                }
                catch (Exception e)
                {
                    LogError("Error reading configs, defaults will be used: " + e);
                }
            }

            using (var sw = new StreamWriter(translationsFS))
            {
                if (!XPAPI.Config.WriteConfig)
                    return;
                if (!changed)
                    return;

                LogWarn("Changes have been made, writing to file.");
                sw.Write(Serializer.Serialize(XPConfigs));
            }
        }

        /// <summary>
        /// Get a <see cref="XPConfigElement"/> from the <see cref="XPConfigs"/> dict,
        /// Will return <c>null</c> if it doesn't exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="key2">The secondary key, or <c>null</c>, to get the default value.</param>
        /// <typeparam name="T">The secondary key type.</typeparam>
        public static XPConfigElement GetConfig<T>(string key, T key2)
        {
            if (!XPConfigs.TryGetValue(key, out var translationCollection))
                return null;

            if (EqualityComparer<T>.Default.Equals(key2, default) ||
                !translationCollection.TryGetConfig(key2, out XPConfigElement value))
                return null;

            return value;
        }
    }
}
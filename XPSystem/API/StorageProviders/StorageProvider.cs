namespace XPSystem.API.StorageProviders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using XPSystem.API.Config;
    using XPSystem.API.StorageProviders.Models;
    using XPSystem.EventHandlers;
    using static LoaderSpecific;

    /// <summary>
    /// <see cref="IStorageProvider"/> but it already implements some methods and caching.
    /// </summary>
    public abstract class StorageProvider : IStorageProvider
    {
        /// <summary>
        /// The expected type for parameter with the given key.
        /// Optional, leave empty to disable.
        /// <example><c>Key: "enableFeature", value: typeof(bool)</c> <br/>
        /// Would allow you to use <code>if (GetParameter&lt;bool&gt;("enableFeature")) {}</code></example>
        /// <remarks>Uses <see cref="TryConvertToType"/> to check if conversion is possible.</remarks>
        /// </summary>
        protected virtual Dictionary<string, Type> ExpectedParametersTypes { get; } = new()
        {
        };

        /// <summary>
        /// Tried to convert the given value to the given type.
        /// Used by <see cref="ExpectedParametersTypes"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="type">The type to convert to.</param>
        /// <param name="converted">The converted value.</param>
        /// <returns>Whether or not the given value could be converted to the given <see cref="Type"/>.</returns>
        protected virtual bool TryConvertToType(string value, Type type, out object converted)
        {
            try
            {
                converted = Convert.ChangeType(value, type);
                return true;
            }
            catch
            {
                converted = null;
                return false;
            }
        }

        private readonly Dictionary<string, object> _convertedParameters = new();
        public Dictionary<string, string> LoadParameters(FileStream fs)
        {
            string content;
            using (var sr = new StreamReader(fs))
            {
                content = sr.ReadToEnd();
            }

            var required = GetRequiredDefaults();
            if (string.IsNullOrWhiteSpace(content))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(XPConfigManager.Serializer.Serialize(required));
                }

                return new Dictionary<string, string>();
            }

            var deserialized = XPConfigManager.Deserializer.Deserialize<Dictionary<string, string>>(content);
            foreach (var kvp in deserialized.ToArray()) // prevent Collection was modified; enumeration operation may not execute. error
            {
                if (ExpectedParametersTypes.TryGetValue(kvp.Key, out var type))
                {
                    if (!TryConvertToType(kvp.Value, type, out var converted))
                    {
                        LogError($"Could not convert parameter {kvp.Key} to type {type.Name}. Using default value.");

                        try
                        {
                            converted = Activator.CreateInstance(type);
                            deserialized[kvp.Key] = converted.ToString();
                        }
                        catch (Exception e)
                        {
                            LogError($"Could not reset parameter {kvp.Key} to default value: {e}. Using null.");
                            converted = null;
                        }
                    }

                    _convertedParameters.Add(kvp.Key, converted);
                }
            }

            foreach (var kvp in required)
            {
                if (!deserialized.ContainsKey(kvp.Key))
                    deserialized.Add(kvp.Key, kvp.Value);
            }

            using (var sw = new StreamWriter(fs))
            {
                sw.Write(XPConfigManager.Serializer.Serialize(deserialized));
            }

            return deserialized;
        }

        /// <summary>
        /// Gets the parameter with the given key.
        /// <remarks>Only works for parameters that you have registered in <see cref="ExpectedParametersTypes"/>. <br/>
        /// Will return null if <see cref="T"/> is a reference type and it could not be converted or reset (possibly due to incorrect config <b>and</b> unfortunate type selection).</remarks>
        /// </summary>
        /// <param name="key">The key of the parameter.</param>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <returns>The parameter with the given key in the correct type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no converted parameter with the given key is found.</exception>
        public T GetParameter<T>(string key)
        {
            if (_convertedParameters.TryGetValue(key, out var value))
                return (T)value;

            throw new KeyNotFoundException($"No converted parameter with key {key} found. Did you register it in ExpectedParametersTypes?");
        }

        protected abstract Dictionary<string, string> GetRequiredDefaults();
        public readonly PlayerInfoCache PlayerInfoCache = new();

        public void Initialize(Dictionary<string, string> parameters)
        {
            UnifiedEventHandlers.PlayerJoined += PlayerInfoCache.EnsureInCache;
            InitializeNoCache(parameters);
        }
        protected abstract void InitializeNoCache(Dictionary<string, string> parameters);

        public virtual void Dispose()
        {
            UnifiedEventHandlers.PlayerJoined -= PlayerInfoCache.EnsureInCache;
            PlayerInfoCache.Clear();
            DisposeNoCache();
        }
        protected abstract void DisposeNoCache();

        public virtual bool TryGetPlayerInfo(PlayerId playerId, out PlayerInfo playerInfo)
        {
            if (PlayerInfoCache.TryGet(playerId, out playerInfo))
                return true;

            return TryGetPlayerInfoNoCache(playerId, out playerInfo);
        }
        protected abstract bool TryGetPlayerInfoNoCache(PlayerId playerId, out PlayerInfo playerInfo);

        public virtual PlayerInfo GetPlayerInfoAndCreateOfNotExist(PlayerId playerId)
        {
            if (PlayerInfoCache.TryGet(playerId, out var playerInfo))
                return playerInfo;

            return GetPlayerInfoAndCreateOfNotExistNoCache(playerId);
        }
        protected abstract PlayerInfo GetPlayerInfoAndCreateOfNotExistNoCache(PlayerId playerId);

        public abstract IEnumerable<PlayerInfo> GetTopPlayers(int count);

        public virtual void SetPlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfoCache.Update(playerInfo);
            SetPlayerInfoNoCache(playerInfo);
        }
        protected abstract void SetPlayerInfoNoCache(PlayerInfo playerInfo);

        public virtual bool DeletePlayerInfo(PlayerId playerId)
        {
            PlayerInfoCache.Remove(playerId);
            return DeletePlayerInfoNoCache(playerId);
        }
        protected abstract bool DeletePlayerInfoNoCache(PlayerId playerId);

        public virtual void DeleteAllPlayerInfo()
        {
            PlayerInfoCache.Clear();
            DeleteAllPlayerInfoNoCache();
        }
        protected abstract void DeleteAllPlayerInfoNoCache();
    }
}
namespace XPSystem.API.DisplayProviders
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using XPSystem.API.StorageProviders;
    using static XPAPI;

    /// <summary>
    /// Represents a collection of <see cref="IXPDisplayProvider"/>s.
    /// Only one provider of a type can be added.
    /// </summary>
    public class XPDisplayProviderCollection : IEnumerable<IXPDisplayProvider>
    {
        private readonly List<IXPDisplayProvider> _providers = new();
        public IReadOnlyList<IXPDisplayProvider> Providers => _providers.AsReadOnly();

        /// <summary>
        /// Refreshes all displays.
        /// </summary>
        public void Refresh()
        {
            foreach (var provider in _providers)
                provider.RefreshAll();

            LogDebug("Refreshed all displays");
        }

        /// <summary>
        /// Refreshes all displays for the specified player.
        /// Called when the person joins the server.
        /// </summary>
        /// <param name="player">The player to refresh the displays for.</param>
        public void RefreshTo(XPPlayer player)
        {
            foreach (var provider in _providers)
                provider.RefreshTo(player);

            LogDebug($"Refreshed displays to {player.Nickname}");
        }

        /// <summary>
        /// Refreshes all displays of the specified player.
        /// Called when the person levels up.
        /// </summary>
        /// <param name="player">The player to refresh the displays of.</param>
        /// <param name="playerInfo">The player's info to refresh the displays using.</param>
        public void RefreshOf(XPPlayer player, PlayerInfoWrapper playerInfo = null)
        {
            playerInfo ??= player.GetPlayerInfo();

            foreach (var provider in _providers)
                provider.RefreshOf(player, playerInfo);

            LogDebug($"Refreshed displays of {player.Nickname}");
        }

        /// <summary>
        /// Adds a provider.
        /// </summary>
        /// <param name="provider">The provider to add.</param>
        public void Add(IXPDisplayProvider provider)
        {
            if (Providers.Any(x => x.GetType() == provider.GetType()))
            {
                LogWarn($"Attempted to add duplicate display provider of type {provider.GetType().Name}.");
                return;
            }

            _providers.Add(provider);
        }

        /// <summary>
        /// Disables all providers.
        /// </summary>
        public void DisableAll()
        {
            foreach (var provider in _providers)
            {
                try
                {
                    provider.Disable();
                }
                catch (Exception e)
                {
                    LogError($"Error while disabling display provider {provider.GetType().Name}: {e}");
                }
            }
        }

        /// <summary>
        /// Enables all enabled (in the config) providers.
        /// </summary>
        public void Enable()
        {
            foreach (var provider in _providers)
            {
                if (provider.ConfigPropertyInternal?.Enabled == true)
                {
                    try
                    {
                        provider.Enable();
                    }
                    catch (Exception e)
                    {
                        LogError($"Error while enabling xp display provider {provider.GetType().Name}: {e}");
                    }
                }
            }
        }

        /// <summary>
        /// Reads and writes (if not exists) the configs of providers from the specified folder.
        /// </summary>
        /// <param name="folder">The folder to read and write the configs from.</param>
        public void LoadConfigs(string folder)
        {
            foreach (var provider in Providers)
            {
                string name = provider.GetType().Name;
                string file = Path.Combine(folder, name + ".yml");

                if (File.Exists(file))
                {
                    try
                    {
                        provider.ConfigPropertyInternal = (IXPDisplayProviderConfig)
                            Deserializer.Deserialize(File.ReadAllText(file), provider.ConfigTypeInternal);
                    }
                    catch (Exception e)
                    {
                        LogError($"Error loading xpdisplayprovider config for {name}: {e}");
                    }
                }
                else
                {
                    var obj = provider.ConfigPropertyInternal;

                    File.WriteAllText(file, XPAPI.Serializer.Serialize(obj));
                    provider.ConfigPropertyInternal = obj;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => _providers.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<IXPDisplayProvider> IEnumerable<IXPDisplayProvider>.GetEnumerator() => _providers.GetEnumerator();
    }
}
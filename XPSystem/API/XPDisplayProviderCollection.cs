namespace XPSystem.API
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using static LoaderSpecific;

    /// <summary>
    /// Represents a collection of <see cref="IXPDisplayProvider"/>s.
    /// Only one provider of a type can be added.
    /// </summary>
    public class XPDisplayProviderCollection : IEnumerable
    {
        private readonly List<IXPDisplayProvider> _providers = new();
        public IReadOnlyList<IXPDisplayProvider> Providers => _providers.AsReadOnly();

        /// <summary>
        /// Refreshes all displays.
        /// </summary>
        public void Refresh()
        {
            foreach (var provider in _providers)
            {
                provider.RefreshAll();
            }
        }

        /// <summary>
        /// Refreshes all displays for the specified player.
        /// </summary>
        /// <param name="player">The player to refresh the displays for.</param>
        public void Refresh(XPPlayer player)
        {
            foreach (var provider in _providers)
            {
                provider.Refresh(player);
            }
        }

        /// <summary>
        /// Adds and enables a provider.
        /// </summary>
        /// <param name="provider"></param>
        public void Add(IXPDisplayProvider provider)
        {
            if (Providers.Any(x => x.GetType() == provider.GetType()))
            {
                LogWarn($"Attempted to add duplicate display provider of type {provider.GetType().Name}.");
                return;
            }

            try
            {
                provider.Enable();
            }
            catch (Exception e)
            {
                LogError($"Failed to enable display provider {provider.GetType().Name}: {e}");
                return;
            }

            _providers.Add(provider);
        }

        /// <summary>
        /// Removes and disables a provider.
        /// </summary>
        /// <param name="providerType">The type of the provider to remove.</param>
        /// <returns>Whether a provider was removed.</returns>
        public bool Remove(Type providerType)
        {
            var provider = _providers.FirstOrDefault(x => x.GetType() == providerType);
            if (provider == null)
                return false;

            _providers.Remove(provider);

            try
            {
                provider.Disable();
            }
            catch (Exception e)
            {
                LogError($"Error while disabling display provider {provider.GetType().Name}: {e}");
            }

            return true;
        }

        /// <summary>
        /// Removes and disables all providers.
        /// </summary>
        public void RemoveAll()
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

            _providers.Clear();
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => _providers.GetEnumerator();
    }
}
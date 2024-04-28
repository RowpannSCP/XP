namespace XPSystem.API
{
    using System;
    using XPSystem.API.StorageProviders;

    /// <summary>
    /// Base class for xp display providers, shows the xp/lvl of a player to others.
    /// </summary>
    public abstract class XPDisplayProvider<T> : IXPDisplayProvider where T : IXPDisplayProviderConfig, new()
    {
        public abstract void Refresh(XPPlayer player, PlayerInfoWrapper playerInfo);

        public virtual void Enable() => RefreshAll();
        public virtual void Disable() => RefreshAll();

        public virtual void RefreshAll()
        {
            foreach (var kvp in XPPlayer.Players)
                Refresh(kvp.Value, kvp.Value.GetPlayerInfo());
        }

        protected T Config { get; set; }

        /// <summary>
        /// Ignore this, used by loader.
        /// </summary>
        IXPDisplayProviderConfig IXPDisplayProvider.ConfigPropertyInternal
        {
            get => Config ?? new T();
            set => Config = (T)value;
        }

        /// <summary>
        /// Ignore this, used by loader.
        /// Type of <see cref="IXPDisplayProvider.ConfigPropertyInternal"/>.
        /// </summary>
        public Type ConfigTypeInternal { get; } = typeof(T);
    }
}
namespace XPSystem.API
{
    using System;

    /// <summary>
    /// Base class for xp display providers, shows the xp/lvl of a player to others.
    /// </summary>
    public abstract class XPDisplayProvider<T> : IXPDisplayProvider where T : IXPDisplayProviderConfig, new()
    {
        public abstract void Enable();
        public abstract void Disable();
        public abstract void Refresh(XPPlayer player);
        public abstract void RefreshAll();

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
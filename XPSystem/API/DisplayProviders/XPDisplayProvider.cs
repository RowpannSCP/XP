namespace XPSystem.API
{
    using System;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.StorageProviders;

    /// <summary>
    /// Base class for xp display providers, shows the xp/lvl of a player to others.
    /// </summary>
    public abstract class XPDisplayProvider<T> : IXPDisplayProvider where T : IXPDisplayProviderConfig, new()
    {
        /// <summary>
        /// Gets the patch category.
        /// Null to disable patching.
        /// </summary>
        protected virtual string PatchCategory => null;

        /// <summary>
        /// Enables the display provider.
        /// </summary>
        public virtual void Enable()
        {
            if (PatchCategory != null)
            {
                // unused
                #warning when exiled updates harmony, update this too
            }

            RefreshAll();
        }

        /// <summary>
        /// Disables the display provider.
        /// </summary>
        public virtual void Disable()
        {
            if (PatchCategory != null)
            {
                // see above
            }

            RefreshAll();
        }

        /// <summary>
        /// Whether or not changes, to players or other, have been made.
        /// </summary>
        protected virtual bool HasSet { get; set; } = false;

        /// <summary>
        /// <see cref="RefreshTo"/> implementation when enabled.
        /// </summary>
        protected abstract void RefreshToEnabled(XPPlayer player);

        /// <summary>
        /// <see cref="RefreshTo"/> implementation when disabled.
        /// </summary>
        protected abstract void RefreshToDisabled(XPPlayer player);

        /// See <see cref="XPDisplayProviderCollection.RefreshTo"/>.
        public virtual void RefreshTo(XPPlayer player)
        {
            if (!Config.Enabled && !HasSet)
                return;

            if (Config.Enabled)
            {
                RefreshToEnabled(player);
                HasSet = true;
            }
            else
            {
                RefreshToDisabled(player);
            }
        }

        /// <summary>
        /// <see cref="RefreshOf"/> implementation when enabled.
        /// </summary>
        protected abstract void RefreshOfEnabled(XPPlayer player, PlayerInfoWrapper playerInfo);

        /// <summary>
        /// <see cref="RefreshOf"/> implementation when disabled.
        /// </summary>
        protected abstract void RefreshOfDisabled(XPPlayer player);

        /// See <see cref="XPDisplayProviderCollection.RefreshTo"/>.
        public virtual void RefreshOf(XPPlayer player, PlayerInfoWrapper playerInfo = null)
        {
            if (!Config.Enabled && !HasSet)
                return;

            if (Config.Enabled)
            {
                RefreshOfEnabled(player, playerInfo ?? player.GetPlayerInfo());
                HasSet = true;
            }
            else
            {
                RefreshOfDisabled(player);
            }
        }

        /// <summary>
        /// Refreshes the displays of all players.
        /// </summary>
        public virtual void RefreshAll()
        {
            if (!Config.Enabled && !HasSet)
                return;

            foreach (var kvp in XPPlayer.Players)
                RefreshOf(kvp.Value, kvp.Value.GetPlayerInfo());

            HasSet = Config.Enabled;
        }

        /// <summary>
        /// The config instance.
        /// </summary>
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
namespace XPSystem.API
{
    using System;
    using System.IO;
    using XPSystem.API.DisplayProviders;
    using XPSystem.API.Player;
    using XPSystem.API.StorageProviders;
    using static XPAPI;

    /// <summary>
    /// Base class for XP display providers, shows the XP/level of a player to others.
    /// </summary>
    public abstract class XPDisplayProvider<T> : ConfigXPDisplayProvider where T : IXPDisplayProviderConfig, new()
    {
        /// <summary>
        /// Gets the patch category.
        /// Null to disable patching.
        /// </summary>
        protected virtual string? PatchCategory => null;

        /// <summary>
        /// Enables the display provider.
        /// </summary>
        public override void Enable()
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
        public override void Disable()
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
        protected virtual void RefreshToEnabled(BaseXPPlayer player) {}

        /// <summary>
        /// <see cref="RefreshTo"/> implementation when disabled.
        /// </summary>
        protected virtual void RefreshToDisabled(BaseXPPlayer player) {}

        /// See <see cref="XPDisplayProviderCollection.RefreshTo"/>.
        public override void RefreshTo(BaseXPPlayer player)
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
        protected virtual void RefreshOfEnabled(BaseXPPlayer player, PlayerInfoWrapper? playerInfo) {}

        /// <summary>
        /// <see cref="RefreshOf"/> implementation when disabled.
        /// </summary>
        protected virtual void RefreshOfDisabled(BaseXPPlayer player) {}

        /// See <see cref="XPDisplayProviderCollection.RefreshTo"/>.
        public override void RefreshOf(BaseXPPlayer player, PlayerInfoWrapper? playerInfo = null)
        {
            if (!Config.Enabled && !HasSet)
                return;

            if (Config.Enabled)
            {
                if (player is XPPlayer xpPlayer)
                    playerInfo ??= xpPlayer.GetPlayerInfo();
                RefreshOfEnabled(player, playerInfo);
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
        public override void RefreshAll()
        {
            if (!Config.Enabled && !HasSet)
                return;

            foreach (BaseXPPlayer player in XPPlayer.PlayersRealConnected)
                RefreshOf(player, player is XPPlayer xpPlayer ? xpPlayer.GetPlayerInfo() : null);

            HasSet = Config.Enabled;
        }

        /// <summary>
        /// The config instance.
        /// </summary>
        public T Config { get; private set; } = default!;

        internal override IXPDisplayProviderConfig ConfigPropertyInternal => Config;
        /// <summary>
        /// Ignore, used by loader.
        /// </summary>
        internal override void LoadConfig(string folder)
        {
            string name = GetType().Name;
            string file = Path.Combine(folder, name + ".yml");

            if (File.Exists(file))
            {
                try
                {
                    Config = Deserializer.Deserialize<T>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    LogError($"Error loading display provider config for {name}: {e}");
                }
            }
            else
            {
                Config = new T();
                File.WriteAllText(file, Serializer.Serialize(Config));
            }
        }
    }

    public abstract class ConfigXPDisplayProvider : IXPDisplayProvider
    {
        public abstract void Enable();
        public abstract void Disable();
        public abstract void RefreshTo(BaseXPPlayer player);
        public abstract void RefreshOf(BaseXPPlayer player, PlayerInfoWrapper? playerInfo = null);
        public abstract void RefreshAll();

        internal abstract IXPDisplayProviderConfig ConfigPropertyInternal { get; }
        internal abstract void LoadConfig(string folder);
    }
}
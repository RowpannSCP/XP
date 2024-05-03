namespace XPSystem.API.StorageProviders
{
    using XPSystem.API.StorageProviders.Models;
    using static XPAPI;
    using PlayerInfo = Models.PlayerInfo;

    /// <summary>
    /// Wrapper for <see cref="PlayerInfo"/>.
    /// Adds level caching.
    /// </summary>
    public class PlayerInfoWrapper
    {
        /// <summary>
        /// The <see cref="PlayerInfo"/> being wrapped.
        /// </summary>
        public readonly PlayerInfo PlayerInfo;

        /// <summary>
        /// Gets the <see cref="PlayerId"/> of the player the <see cref="PlayerInfo"/> belongs to.
        /// </summary>
        public PlayerId Player => PlayerInfo.Player;

        /// <summary>
        /// Gets the stored nickname of the player.
        /// </summary>
        public string Nickname => PlayerInfo.Nickname;

        /// <summary>
        /// Gets or sets the amount of XP of the player has.
        /// </summary>
        public int XP
        {
            get => PlayerInfo.XP;
            set
            {
                EnsureStorageProviderValid();

                float floatAmount = value;
                bool playerOnline = XPPlayer.TryGet(Player, out var xpPlayer);

                if (value > 0 || Config.XPMultiplerForXPLoss)
                {
                    if (playerOnline)
                        floatAmount *= xpPlayer.XPMultiplier;

                    if (playerOnline || Config.GlobalXPMultiplierForNonOnline)
                        floatAmount *= GlobalXPMultiplier;
                }

                value = (int)floatAmount;

                int prevLevel = Level;
                PlayerInfo.XP = value;
                int newLevel = Level;

                if (prevLevel != newLevel && playerOnline)
                    HandleLevelUp(xpPlayer, this);

                XPAPI.StorageProvider.SetPlayerInfo(this);
            }
        }

        private int _neededXP;
        private int _lastCalculatedNeededXP;
        private int _lastCalculatedNeededReload = -1;

        /// <summary>
        /// Gets the amount of XP needed for the next level.
        /// </summary>
        public int NeededXP
        {
            get
            {
                if (_lastCalculatedNeededXP != XP || _lastCalculatedNeededReload != Main.Reload)
                {
                    _lastCalculatedNeededXP = XP;
                    _lastCalculatedNeededReload = Main.Reload;
                    _neededXP = LevelCalculator.GetXP(Level + 1);
                }

                return _neededXP;
            }
        }

        private int _level;
        private int _lastCalculatedLevelXP;

        /// <summary>
        /// Gets or sets (not recommended) the level of the player.
        /// </summary>
        public int Level
        {
            get
            {
                if (_lastCalculatedLevelXP != XP)
                {
                    _lastCalculatedLevelXP = XP;
                    _level = LevelCalculator.GetLevel(XP);
                }

                return _level;
            }
            set
            {
                XP = LevelCalculator.GetXP(value);
                _level = value;
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="PlayerInfoWrapper"/> with the given <see cref="PlayerInfo"/>
        /// </summary>
        /// <param name="playerInfo">The <see cref="PlayerInfo"/> to wrap.</param>
        public PlayerInfoWrapper(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        /// <summary>
        /// Gets the <see cref="PlayerInfo"/> from the wrapper.
        /// </summary>
        /// <param name="wrapper">The wrapper to get the <see cref="PlayerInfo"/> from.</param>
        /// <returns>The <see cref="PlayerInfo"/> being wrapped.</returns>
        public static implicit operator PlayerInfo(PlayerInfoWrapper wrapper) => wrapper.PlayerInfo;

        /// <summary>
        /// Creates a new <see cref="PlayerInfoWrapper"/> with the given <see cref="PlayerInfo"/>.
        /// </summary>
        /// <param name="playerInfo">The <see cref="PlayerInfo"/> to wrap.</param>
        /// <returns>The new <see cref="PlayerInfoWrapper"/>.</returns>
        public static implicit operator PlayerInfoWrapper(PlayerInfo playerInfo) => new(playerInfo);
    }
}
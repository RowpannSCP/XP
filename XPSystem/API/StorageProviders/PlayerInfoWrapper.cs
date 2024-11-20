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
        /// Gets the <see cref="IPlayerId{T}"/> of the player the <see cref="PlayerInfo"/> belongs to.
        /// </summary>
        public IPlayerId<object> Player => PlayerInfo.Player;

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
            set => AddXP(this, value - XP);
        }

        private int _neededXPNext;
        private int _lastCalculatedNeededNext;
        private int _lastCalculatedNeededNextReload = -1;

        /// <summary>
        /// Gets the amount of XP needed for the next level.
        /// </summary>
        public int NeededXPNext
        {
            get
            {
                if (_lastCalculatedNeededNext != XP || _lastCalculatedNeededNextReload != Main.Reload)
                {
                    _lastCalculatedNeededNext = XP;
                    _lastCalculatedNeededNextReload = Main.Reload;
                    _neededXPNext = LevelCalculator.GetXP(Level + 1);
                }

                return _neededXPNext;
            }
        }
        
        private int _neededXPCurrent;
        private int _lastCalculatedNeededCurrent;
        private int _lastCalculatedNeededCurrentReload = -1;
        
        /// <summary>
        /// Gets the amount of XP needed for the current level.
        /// </summary>
        public int NeededXPCurrent
        {
            get
            {
                if (_lastCalculatedNeededCurrent != XP || _lastCalculatedNeededCurrentReload != Main.Reload)
                {
                    _lastCalculatedNeededCurrent = XP;
                    _lastCalculatedNeededCurrentReload = Main.Reload;
                    _neededXPCurrent = LevelCalculator.GetXP(Level);
                }

                return _neededXPCurrent;
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
                int xp =  LevelCalculator.GetXP(value);

                _level = value;
                _lastCalculatedLevelXP = xp;
                XP = xp;
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
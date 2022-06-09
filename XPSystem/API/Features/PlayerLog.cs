using Exiled.API.Features;
using XPSystem.API.Serialization;
using Badge = XPSystem.API.Features.Badge;

namespace XPSystem
{
    // unserializable version of playerlog meant for regular use
    public class PlayerLog : PlayerLogSerializable
    {
        private string customRank;
        private string rank;
        public Player Player { get; set; }        
        public PlayerLog(PlayerLogSerializable log)
        {
            XP = log.XP;
            LVL = log.LVL;
        }
        public PlayerLog(Player player)
        {
            XP = 0;
            LVL = 0;
            Player = player;
        }
        public void AddXP(int xp)
        {
            XP += xp;
            int lvlsGained = XP / Main.Instance.Config.XPPerLevel;
            if (lvlsGained > 0)
            {
                LVL += lvlsGained;
                XP -= lvlsGained * Main.Instance.Config.XPPerLevel;
                if (Main.Instance.Config.ShowAddedLVL)
                {
                    Player.ShowHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", LVL.ToString()));
                }
                ApplyRank();
            }
            else if (Main.Instance.Config.ShowAddedXP)
            {
                Player.ShowHint($"+ <color=green>{xp}</color> XP");
            }
        }

        public void ApplyRank()
        {
            Player.RankName = customRank ?? Player.Group?.BadgeText ?? "";
        }

        public string EvaluateRank(string badgeText) // more internal, used for transpiler
        {
            if (badgeText == rank) // the settext method gets called twice because questionable NW networking, prevent duplication
            {
                return badgeText;
            }

            Badge badge = GetLVLBadge();
            string new_rank = Main.Instance.Config.BadgeStructure
                .Replace("%lvl%", LVL.ToString())
                .Replace("%badge%", badge.Name)
                .Replace("%oldbadge%", badgeText);

            rank = new_rank;
            customRank = badgeText;

            string raColor = Player.Group?.BadgeColor;
            Player.RankColor = Main.Instance.Config.OverrideColor && raColor != null ? raColor : badge.Color;

            return rank;
        }

        Badge GetLVLBadge()
        {
            Badge biggestLvl = new Badge
            {
                Name = "UNDEFINED",
                Color = "red"
            };
            foreach (var pair in Main.Instance.Config.LevelsBadge)
            {
                if (LVL < pair.Key)
                {
                    break;
                }
                biggestLvl = pair.Value;
            }
            return biggestLvl;
        }        
    }
}

using System.Collections.Generic;
using AdvancedHints;
using AdvancedHints.Enums;
using Exiled.API.Features;
using XPSystem.API.Serialization;

using Badge = XPSystem.API.Features.Badge;

namespace XPSystem.API
{
    public static class Extensions
    {
        public static PlayerLog GetLog(this Player ply)
        {
            if (!API.TryGetLog(ply.UserId, out var log))
            {
                log = new PlayerLog()
                {
                    ID = ply.UserId,
                    LVL = 0,
                    XP = 0,
                };
                Main.Instance.db.GetCollection<PlayerLog>("Players").Insert(log);
            }
            return log;
        }

        public static void UpdateLog(this PlayerLog log)
        {
            Main.Instance.db.GetCollection<PlayerLog>("Players").Update(log);
        }

        public static void AddXP(this PlayerLog log, int amount)
        {
            log.XP += amount;
            Player ply = Player.Get(log.ID);
            int lvlsGained = log.XP / Main.Instance.Config.XPPerLevel;
            if (lvlsGained > 0)
            {
                log.LVL += lvlsGained;
                log.XP -= lvlsGained * Main.Instance.Config.XPPerLevel;
                if (Main.Instance.Config.ShowAddedLVL && ply != null)
                {
                    ply.ShowManagedHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", log.LVL.ToString()), 3f, true, DisplayLocation.Top);
                }

                ply.RankName = "";
            }
            else if (Main.Instance.Config.ShowAddedXP && ply != null)
            {
                ply.ShowManagedHint($"+ <color=green>{amount}</color> XP", 3f, true, DisplayLocation.Top);
            }
            log.UpdateLog();
        }
    }
}
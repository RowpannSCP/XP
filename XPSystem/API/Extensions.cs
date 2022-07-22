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
        private static Config _cfg = Main.Instance.Config;
        public static PlayerLog GetLog(this Player ply)
        {
            PlayerLog toInsert = null;
            if (!API.TryGetLog(ply.UserId, out var log))
            {
                toInsert = new PlayerLog()
                {
                    ID = ply.UserId,
                    LVL = 0,
                    XP = 0,
                };
                Main.Instance.db.GetCollection<PlayerLog>("Players").Insert(toInsert);
            }

            if (log is null)
                return toInsert;
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
                    ply.ShowCustomHint(Main.Instance.Config.AddedLVLHint
                        .Replace("%level%", log.LVL.ToString()));
                }

                ply.RankName = "";
            }
            else if (Main.Instance.Config.ShowAddedXP && ply != null)
            {
                ply.ShowCustomHint($"+ <color=green>{amount}</color> XP");
            }
            log.UpdateLog();
        }

        public static void ShowCustomHint(this Player ply, string text)
        {
            ply.ShowManagedHint($"<voffset={_cfg.VOffest}em><space={_cfg.HintSpace}em><size={_cfg.HintSize}%>{text}</size></voffset></voffset>", _cfg.HintDuration, _cfg.OverrideQuene, _cfg.HintLocation);
        }
    }
}
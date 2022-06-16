using System.Collections.Generic;
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
            log.UpdateLog();
        }
    }
}
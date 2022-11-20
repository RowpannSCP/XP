using XPSystem.API.Serialization;

namespace XPSystem.API
{
    using System.Linq;
    using Exiled.API.Features;
    using Badge = Features.Badge;

    public static class API
    {
        public static bool TryGetLog(string id, out PlayerLog log)
        {
            log = Main.Instance.db.GetCollection<PlayerLog>("Players")?.FindById(id);
            return log != null;
        }

        public static void UpdateBadge(Player ply, string i = null)
        {
            if(i != null && i.Contains("\n"))
                return;
            var log = ply.GetLog();
            Badge badge = Main.Instance.Config.DNTBadge;
            if(!ply.DoNotTrack)
            {
                foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderBy(kvp => kvp.Key))
                {
                    if (log.LVL > kvp.Key && Main.Instance.Config.LevelsBadge.OrderByDescending(kvp2 => kvp2.Key).ElementAt(0).Key != kvp.Key)
                        continue;
                    badge = kvp.Value;
                    break;
                }
            }

            bool hasGroup = ply.Group == null || string.IsNullOrEmpty(ply.RankName);
            Log.Debug($"i is null {i == null}");
            Log.Debug($"Using i: {hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)}");
            string text = hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)
                ? Main.Instance.Config.BadgeStructureNoBadge
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                : Main.Instance.Config.BadgeStructure
                    .Replace("%lvl%", log.LVL.ToString())
                    .Replace("%badge%", badge.Name)
                    .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.Group?.BadgeText : i);
            text += "\n";
            string color = badge.Color;
            if (hasGroup || !Main.Instance.Config.OverrideColor)
            {
                color = ply.Group?.BadgeColor ?? color;
            }


            ply.RankName = text;
            ply.RankColor = color;
        }
    }
}
using XPSystem.API.Serialization;

namespace XPSystem.API
{
    using System.Linq;
    using Badge = Features.Badge;

    public static class API
    {
        public static bool TryGetLog(string id, out PlayerLog log)
        {
            log = Main.Instance.db.GetCollection<PlayerLog>("Players")?.FindById(id);
            return log != null;
        }

        public static void UpdateBadge(ReferenceHub ply, string i = null)
        {
            if(i != null && i.Contains("\n"))
                return;
            if (ply == null || ply.characterClassManager.UserId == null)
            {
                Main.LogWarn("Not updating role: player null");
                return;
            }
            var log = ply.GetLog();
            Badge badge = Main.Instance.Config.DNTBadge;
            if(!ply.serverRoles.DoNotTrack)
            {
                foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderBy(kvp => kvp.Key))
                {
                    if (log.LVL > kvp.Key && Main.Instance.Config.LevelsBadge.OrderByDescending(kvp2 => kvp2.Key).ElementAt(0).Key != kvp.Key)
                        continue;
                    badge = kvp.Value;
                    break;
                }
            }

            bool hasGroup = ply.serverRoles.Group == null || string.IsNullOrEmpty(ply.serverRoles.Network_myText);
            Main.DebugProgress($"i is null {i == null}");
            Main.DebugProgress($"Using i: {hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)}");
            string text = hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)
                ? Main.Instance.Config.BadgeStructureNoBadge
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                : Main.Instance.Config.BadgeStructure
                    .Replace("%lvl%", log.LVL.ToString())
                    .Replace("%badge%", badge.Name)
                    .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.serverRoles.Group?.BadgeText : i);
            text += "\n";
            string color = badge.Color;
            if (hasGroup || !Main.Instance.Config.OverrideColor)
            {
                color = ply.serverRoles.Group?.BadgeColor ?? color;
            }

            ply.serverRoles.SetText(text);
            ply.serverRoles.SetColor(color);
        }
    }
}
using XPSystem.API.Serialization;

namespace XPSystem.API
{
    using System.Linq;
    using LiteDB;
    using Badge = Features.Badge;

    public static class API
    {
        public static ILiteCollection<PlayerLog> PlayerLogCollection => Main.Instance.db.GetCollection<PlayerLog>("Players");

        public static bool TryGetLog(string id, out PlayerLog log)
        {
            log = PlayerLogCollection?.FindById(id);
            return log != null;
        }

        public static void UpdateBadge(ReferenceHub ply, string i = null, bool hide = false)
        {
            if(i != null && i.Contains("\n"))
                return;
            if (ply == null || ply.authManager.UserId == null)
            {
                Main.LogWarn("Not updating role: player null");
                return;
            }
            if (ply.serverRoles.HiddenBadge != null)
                hide = true;
            if (Main.Instance.Config.VSRComplaint && ply.serverRoles.GlobalSet && !Main.Instance.Config.BadgeStructure.Contains("%oldbadge%"))
            {
                Main.DebugProgress("Not hiding gbadge");
                return;
            }

            var log = ply.GetLog();
            Badge badge = null;
            if(!ply.authManager.DoNotTrack)
            {
                if (Main.Instance.Config.BadgeKeyIsRequiredLevel)
                {
                    foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderByDescending(kvp => kvp.Key))
                    {
                        if (log.LVL < kvp.Key)
                            continue;
                        badge = kvp.Value;
                        break;
                    }
                }
                else
                {
                    var lastBadge = Main.Instance.Config.LevelsBadge.OrderByDescending(kvp2 => kvp2.Key).ElementAt(0);
                    foreach (var kvp in Main.Instance.Config.LevelsBadge.OrderBy(kvp => kvp.Key))
                    {
                        if (log.LVL > kvp.Key && kvp.Key != lastBadge.Key)
                            continue;
                        badge = kvp.Value;
                        break;
                    }
                }
            }
            else
            {
                badge = Main.Instance.Config.DNTBadge;
            }

            if (badge == null)
            {
                return;
            }

            bool hasGroup = ply.serverRoles.Group != null || !string.IsNullOrEmpty(ply.serverRoles.Network_myText);
            Main.DebugProgress($"i is null {i == null}");
            Main.DebugProgress($"Using i: {hasGroup && !string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)}");
            string text;
            if (hasGroup)
            {
                if (hide)
                {
                    text = string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)
                        ? Main.Instance.Config.BadgeStructure
                            .Replace("%lvl%", log.LVL.ToString())
                            .Replace("%badge%", badge.Name)
                            .Replace("%oldbadge%", string.Empty)
                        : Main.Instance.Config.BadgeStructureNoBadge
                            .Replace("%lvl%", log.LVL.ToString())
                            .Replace("%badge%", badge.Name);
                    ply.serverRoles.HiddenBadge = Main.Instance.Config.BadgeStructure
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                        .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.serverRoles.Group?.BadgeText : i);
                }
                else
                {
                    text = Main.Instance.Config.BadgeStructure
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                        .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.serverRoles.Group?.BadgeText : i);
                }
            }
            else
            {
                text = string.IsNullOrEmpty(Main.Instance.Config.BadgeStructureNoBadge)
                    ? Main.Instance.Config.BadgeStructure
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name)
                        .Replace("%oldbadge%", string.IsNullOrWhiteSpace(i) ? ply.serverRoles.Group?.BadgeText : i)
                    : Main.Instance.Config.BadgeStructureNoBadge
                        .Replace("%lvl%", log.LVL.ToString())
                        .Replace("%badge%", badge.Name);
            }

            text += "\n";
            string color = badge.Color;
            if (hasGroup || !Main.Instance.Config.OverrideColor || hide)
            {
                color = ply.serverRoles.Group?.BadgeColor ?? color;
            }

            ply.serverRoles.SetText(text);
            ply.serverRoles.SetColor(color);
        }
    }
}
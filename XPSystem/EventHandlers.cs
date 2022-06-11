using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System.Linq;
using XPSystem.API;
using XPSystem.API.Serialization;
using XPSystem.Component;

namespace XPSystem
{
    class EventHandlers
    {
        public void OnJoined(VerifiedEventArgs ev)
        {
            if (ev.Player.DoNotTrack)
            {
                ev.Player.OpenReportWindow(Main.Instance.Config.DNTHint);
                return;
            }

            ev.Player.ReferenceHub.gameObject.AddComponent<XPComponent>();
            ev.Player.RankName = "";
        }

        public void OnKill(DyingEventArgs ev)
        {
            if (ev.Target == null || ev.Target.DoNotTrack)
            {
                return;
            }
            Player killer = ev.Handler.Type == DamageType.PocketDimension ? Player.Get(RoleType.Scp106).FirstOrDefault() : ev.Killer;
            if (killer == null)
            {
                return;
            }
            if (Main.Instance.Config.KillXP.TryGetValue(killer.Role, out var killxpdict) && killxpdict.TryGetValue(ev.Target.Role, out int xp))
            {
                ev.Killer.GetXPComponent().AddXP(xp);
            }
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            ev.Player.GetXPComponent().AddXP(Main.Instance.Config.EscapeXP[ev.Player.Role]);
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Side team;
            switch (ev.LeadingTeam)
            {
                case LeadingTeam.FacilityForces:
                    team = Side.Mtf;
                    break;
                case LeadingTeam.ChaosInsurgency:
                    team = Side.ChaosInsurgency;
                    break;
                case LeadingTeam.Anomalies:
                    team = Side.Scp;
                    break;
                case LeadingTeam.Draw:
                    team = Side.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var player in Player.Get(team))
            {
                player.GetXPComponent().AddXP(Main.Instance.Config.TeamWinXP);
            }
        }

        public void OnLeaving(DestroyingEventArgs ev)
        {
            
        }
    }
}

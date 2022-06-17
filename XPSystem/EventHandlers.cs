using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using System.Linq;
using MEC;
using XPSystem.API;
using XPSystem.API.Serialization;

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

            ev.Player.GetLog();
            Timing.CallDelayed(0.5f, () =>
            {
                ev.Player.RankName = "";
            });
        }

        public void OnKill(DyingEventArgs ev)
        {
            if (ev.Killer == null || ev.Killer.DoNotTrack)
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
                var log = ev.Killer.GetLog();
                log.AddXP(xp);
                log.UpdateLog();
            }
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            var log = ev.Player.GetLog();
            log.AddXP(Main.Instance.Config.EscapeXP[ev.Player.Role]);
            log.UpdateLog();
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
                var log = player.GetLog();
                if (log is null)
                    return;
                log.AddXP(Main.Instance.Config.TeamWinXP);
                log.UpdateLog();
            }
        }

        public void OnLeaving(DestroyingEventArgs ev)
        {
            
        }
    }
}

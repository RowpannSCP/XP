namespace XPSystem.EventHandlers.LoaderSpecific
{
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;
    using XPSystem.API;

#if EXILED
    public class ExiledEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified += PlayerJoined;
            Exiled.Events.Handlers.Server.RoundEnded += RoundEnded;
        }

        public override void UnregisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified -= PlayerJoined;
            Exiled.Events.Handlers.Server.RoundEnded -= RoundEnded;
        }

        private void PlayerJoined(VerifiedEventArgs ev)
        {
            OnPlayerJoined(new XPPlayer(ev.Player.ReferenceHub));
        }

        protected void RoundEnded(RoundEndedEventArgs ev)
        {
            OnRoundEnded();
        }
    }
#endif
}
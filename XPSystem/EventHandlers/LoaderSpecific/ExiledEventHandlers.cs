namespace XPSystem.EventHandlers.LoaderSpecific
{
    using Exiled.Events.EventArgs.Player;
    using XPSystem.API;

#if EXILED
    public class ExiledEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified += OnPlayerJoined;
        }

        public override void UnregisterEvents(Main plugin)
        {
            Exiled.Events.Handlers.Player.Verified -= OnPlayerJoined;
        }

        private void OnPlayerJoined(VerifiedEventArgs ev)
        {
            OnPlayerJoined(new XPPlayer(ev.Player.ReferenceHub));
        }
    }
#endif
}
namespace XPSystem.EventHandlers.LoaderSpecific
{
    using PluginAPI.Core;
    using PluginAPI.Core.Attributes;
    using PluginAPI.Enums;
    using PluginAPI.Events;
    using XPSystem.API;

#if !EXILED
    public class NWAPIEventHandlers : UnifiedEventHandlers
    {
        public override void RegisterEvents(Main plugin)
        {
            EventManager.RegisterEvents(plugin, this);
        }

        public override void UnregisterEvents(Main plugin)
        {
            EventManager.UnregisterEvents(plugin, this);
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        private void OnPlayerJoined(Player player)
        {
            OnPlayerJoined(new XPPlayer(player.ReferenceHub));
        }
    }
#endif
}
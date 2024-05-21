namespace XPSystem
{
    using XPSystem.API;
    using XPSystem.API.Enums;

    public class MessagingProviders
    {
        public static IMessagingProvider Get(DisplayMode displayMode) =>
            displayMode switch
            {
                DisplayMode.Hint => new HintMessagingProvider(),
                DisplayMode.Broadcast => new BroadcastMessagingProvider(),
                DisplayMode.Console => new ConsoleMessagingProvider(),
                _ => null
            };

        public class HintMessagingProvider : IMessagingProvider
        {
            public void DisplayMessage(XPPlayer player, string message, float duration)
            {
                player.ShowHint(message, duration);
            }
        }

        public class BroadcastMessagingProvider : IMessagingProvider
        {
            public void DisplayMessage(XPPlayer player, string message, float duration)
            {
                player.ShowBroadcast(message, (ushort)duration);
            }
        }

        public class ConsoleMessagingProvider : IMessagingProvider
        {
            public void DisplayMessage(XPPlayer player, string message, float duration)
            {
                player.SendConsoleMessage(message, "green");
            }
        }
    }
}
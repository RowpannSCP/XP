namespace XPSystem
{
    using System;
    using System.Collections.Generic;
    using MEC;
    using XPSystem.API;
    using XPSystem.API.Enums;
    using XPSystem.API.Player;

    public class MessagingProviders
    {
        public static IMessagingProvider? Get(DisplayMode displayMode) =>
            displayMode switch
            {
                DisplayMode.Hint => new HintMessagingProvider(),
                DisplayMode.Broadcast => new BroadcastMessagingProvider(),
                DisplayMode.Console => new ConsoleMessagingProvider(),
                _ => null
            };

        public class HintMessagingProvider : IMessagingProvider
        {
            private static readonly Dictionary<BaseXPPlayer, PendingHint> PendingHints = new();

            public void DisplayMessage(BaseXPPlayer player, string message, float duration)
            {
                float window = Math.Max(0f, XPAPI.Config.HintCoalesceWindow);
                if (window <= 0f)
                {
                    ShowHint(player, message, duration);
                    return;
                }

                if (PendingHints.TryGetValue(player, out PendingHint pendingHint))
                {
                    pendingHint.Messages.Add(message);
                    pendingHint.Duration = Math.Max(pendingHint.Duration, duration);
                    return;
                }

                pendingHint = new PendingHint(message, duration);
                PendingHints[player] = pendingHint;

                Timing.CallDelayed(window, () =>
                {
                    if (!PendingHints.TryGetValue(player, out PendingHint hint))
                        return;

                    PendingHints.Remove(player);
                    if (!player.IsConnected)
                        return;

                    ShowHint(player, string.Join(XPAPI.Config.AddedXPLevelSeparator, hint.Messages), hint.Duration);
                });
            }

            private static void ShowHint(BaseXPPlayer player, string message, float duration)
            {
                if (!string.IsNullOrWhiteSpace(XPAPI.Config.HintVerticalOffset))
                    message = $"<voffset={XPAPI.Config.HintVerticalOffset}>{message}</voffset>";

                player.ShowHint(message, duration);
            }

            private class PendingHint
            {
                public List<string> Messages { get; } = new();
                public float Duration { get; set; }

                public PendingHint(string message, float duration)
                {
                    Messages.Add(message);
                    Duration = duration;
                }
            }
        }

        public class BroadcastMessagingProvider : IMessagingProvider
        {
            public void DisplayMessage(BaseXPPlayer player, string message, float duration)
            {
                player.ShowBroadcast(message, (ushort)duration);
            }
        }

        public class ConsoleMessagingProvider : IMessagingProvider
        {
            public void DisplayMessage(BaseXPPlayer player, string message, float duration)
            {
                player.SendConsoleMessage(message, "green");
            }
        }
    }
}

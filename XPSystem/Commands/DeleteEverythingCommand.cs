namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.Serialization;

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class DeleteEverythingCommand : ICommand
    {
        private static DateTime _lastUsed = DateTime.MinValue;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is ServerConsoleSender)
            {
                if ((DateTime.Now - _lastUsed).TotalSeconds < 10)
                {
                    API.PlayerLogCollection.DeleteAll();
#if EXILED
                    Main.Instance.Handlers.AlreadyGainedPlayers.Clear();
#endif
                    Main.Instance.Handlers.AlreadyGainedPlayers2.Clear();
                    Main.Instance.Handlers.AlreadyGainedPlayers3.Clear();
                    Main.Instance.Handlers.AlreadyGainedPlayers5.Clear();
                    Main.Instance.Handlers.AlreadyGainedPlayers6.Clear();

                    if (Main.EnabledNick)
                    {
                        foreach (var hub in ReferenceHub.AllHubs)
                        {
                            if (hub == ReferenceHub.HostHub)
                                continue;
                            hub.nicknameSync.DisplayName = hub.nicknameSync.Network_myNickSync;
                        }
                    }

                    if (Main.EnabledRank)
                    {
                        foreach (var hub in ReferenceHub.AllHubs)
                        {
                            if (hub == ReferenceHub.HostHub)
                                continue;
                            API.UpdateBadge(hub, hub.serverRoles.Group?.BadgeText);
                        }
                    }

                    response = "Everything deleted.";
                    return true;
                }
                _lastUsed = DateTime.Now;
                response = "Are you sure you want to delete everything from the database? Run again with 10 seconds confirm.";
                return true;
            }

            response = "You must be the server console to use this command.";
            return false;
        }

        public string Command { get; } = "xp_deleteeverything";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Deletes everything from the database.";
    }
}
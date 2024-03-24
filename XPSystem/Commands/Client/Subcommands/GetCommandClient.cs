namespace XPSystem.Commands.Client.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class GetCommandClient : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGet(sender, out var player))
            {
                response = "This command is player only.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            var playerId = player.GetPlayerId();
            var log = XPAPI.StorageProvider.GetPlayerInfoAndCreateOfNotExist(playerId);

            response = $"LVL: {log.GetLevel()} | XP: {log.XP}";
            return true;
        }

        public string Command { get; } = "get";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Get your level and XP.";
    }
}
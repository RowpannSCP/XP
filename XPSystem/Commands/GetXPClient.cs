namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using RemoteAdmin;
    using XPSystem.API;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class GetXPClient : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Main.Instance.Config.EnableGetXPCommand)
            {
                response = "This command is disabled.";
                return false;
            }
            if (sender is PlayerCommandSender player)
            {
                var log = player.ReferenceHub.GetLog();
                response = $"LVL: {log.LVL} | XP: {log.XP}";
                return true;
            }
            response = "This command is only for players.";
            return false;
        }

        public string Command { get; } = "getxp";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Gets your XP and LVL";
    }
}
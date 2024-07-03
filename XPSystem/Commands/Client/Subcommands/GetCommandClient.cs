namespace XPSystem.Commands.Client.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class GetCommandClient : SanitizedInputCommand, IAliasableCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!XPPlayer.TryGet(sender, out var player))
            {
                response = "This command is player only.";
                return false;
            }

            var log = player.GetPlayerInfo();

            response = $"LVL: {log.Level} | XP: {log.XP}";
            return true;
        }

        public string CommandOverride { get; set; } = "get";
        public override string Command => CommandOverride;
        public override string[] Aliases { get; } = Array.Empty<string>();
        public override string Description { get; } = "Get your level and XP.";
    }
}
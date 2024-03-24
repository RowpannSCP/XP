namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;

    public class ShowMessageCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!
        }

        public string Command { get; } = "showmessage";
        public string[] Aliases { get; } = { "sm" };
        public string Description { get; } = "Messaging and translation debug tool.";
    }
}
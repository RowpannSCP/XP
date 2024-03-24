namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class RefreshCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.refresh"))
            {
                response = "You don't have permission (xps.refresh) to use this command.";
                return false;
            }

            XPAPI.DisplayProviders.Refresh();

            response = "Refreshed all xp displays.";
            return true;
        }

        public string Command { get; } = "refresh";
        public string[] Aliases { get; } = new[] { "r" };
        public string Description { get; } = "Refreshes all xp displays.";
    }
}
namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class RefreshCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
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

        public override string Command { get; } = "refresh";
        public override string[] Aliases { get; } = new[] { "r" };
        public override string Description { get; } = "Refreshes all xp displays.";
    }
}
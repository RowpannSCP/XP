namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.Config.Events;

    public class ShowMessageCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.showmessage"))
            {
                response = "You do not have permission (xps.showmessage) to run this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: showmessage <key> (<subkey>)";
                return false;
            }

            var file = XPECManager.GetXPECFile(arguments.At(0));
            if (file == null)
            {
                response = "No such XPEC file.";
                return false;
            }

            object subkey = null;
            if (arguments.Count > 1)
            {
                var subkeyString = arguments.At(1);
                subkey = Convert.ChangeType(subkeyString, file.GetSubkeyType());
                if (subkey == null)
                {
                    response = "Subkey conversion failed.";
                    return false;
                }
            }

            var item = file.GetItem(subkey);
            if (item == null)
            {
                response = "Item null.";
                return false;
            }

            response = $"Amount: {item.Amount}, Translation: {item.Translation}";
            return true;
        }

        public string Command { get; } = "showmessage";
        public string[] Aliases { get; } = { "sm" };
        public string Description { get; } = "Messaging and translation debug tool.";
    }
}
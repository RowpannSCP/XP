namespace XPSystem.Commands.Console.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;

    public class DeleteEverythingCommand : SanitizedInputCommand
    {
        private static DateTime _lastUsed = DateTime.MinValue;

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not ServerConsoleSender)
            {
                response = "You must be the server console to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            if ((DateTime.Now - _lastUsed).TotalSeconds < 10)
            {
                XPAPI.StorageProvider.DeleteAllPlayerInfo();

                response = "Everything deleted.";
                return true;
            }

            _lastUsed = DateTime.Now;
            response = "Are you sure you want to delete everything from the database? Run again with 10 seconds confirm.";
            return true;
        }

        public override string Command { get; } = "deleteeverything";
        public override string[] Aliases { get; } = Array.Empty<string>();
        public override string Description { get; } = "Deletes the data of all players.";
    }
}
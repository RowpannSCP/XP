namespace XPSystem.Commands.Console.Subcommands
{
    using System;
    using System.IO;
    using CommandSystem;
    using LiteDB;
    using XPSystem.API;
    using XPSystem.API.Legacy;
    using static API.XPAPI;

    public class MigrateCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string path;
            if (arguments.Count < 1)
            {
                path = LoaderSpecific.LegacyDefaultDatabasePath;
                LogInfo("No file provided, using default.");
            }
            else
            {
                path = string.Join(" ", arguments);
            }

            if (!File.Exists(path))
            {
                response = $"File not found: {path}.";
                return false;
            }

            int affected = -1;
            try
            {
                using (var db = new LiteDatabase(path))
                {
                    affected = LiteDBMigrator.ImportLegacyDB(db);
                }
            }
            catch (Exception e)
            {
                response = $"Could not migrate the database: {e}";
                return false;
            }

            response = $"Migrated {affected} entries.";
            return true;
        }

        public string Command { get; } = "migrate";
        public string[] Aliases { get; } = Array.Empty<string>();
        public string Description { get; } = "Imports data from the old XPSystem database.";
    }
}
namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using CommandSystem;
    using XPSystem.API;
    using XPSystem.API.StorageProviders;

    public class ClearCacheCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.admin.clearcache"))
            {
                response = "You do not have permission (xps.clearcache) to use this command.";
                return false;
            }

            XPAPI.EnsureStorageProviderValid();

            if (XPAPI.StorageProvider is not StorageProvider storageProvider)
            {
                response = "Storage provider is not a predefined cached storage provider, and cannot be cleared with this command.";
                return false;
            }

            storageProvider.ClearCache();

            response = "Cache cleared.";
            return true;
        }

        public override string Command { get; } = "clearcache";
        public override string Description { get; } = "Clears the storage providers cache.";
    }
}
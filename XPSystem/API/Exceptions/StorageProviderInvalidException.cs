namespace XPSystem.API.Exceptions
{
    using System;

    public class StorageProviderInvalidException : Exception
    {
        public StorageProviderInvalidException() : base("No storage provider has been set successfully.")
        {
        }
    }
}
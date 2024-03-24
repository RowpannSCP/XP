namespace XPSystem.API.Exceptions
{
    using System;

    public class StorageProviderInvalidException : Exception
    {
        public StorageProviderInvalidException(string message) : base(message)
        {
        }
    }
}
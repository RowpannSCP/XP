namespace XPSystem.API.Exceptions
{
    using System;

    public class InvalidPlayerIdException : Exception
    {
        public InvalidPlayerIdException() : base("The PlayerId for the specified player is invalid! Are you trying to modify the xp for the host?")
        {
        }
    }
}
namespace XPSystem.API.Exceptions
{
    using System;

    public class InvalidPlayerIdException : Exception
    {
        public InvalidPlayerIdException() : base("The PlayerId for the specified player is invalid! Are you trying to add xp to a the host?")
        {
        }
    }
}
namespace XPSystem.Commands
{
    using System;
    using CommandSystem;

    /// <summary>
    /// Command that sanitizes the response.
    /// Just <see cref="ICommand"/> with <see cref="SanitizeResponse"/> set to <see langword="true"/>.
    /// <remarks>Also empty default aliases :DDDD</remarks>
    /// </summary>
    public abstract class SanitizedInputCommand : ICommand
    {
        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);
        public abstract string Command { get; }
        public abstract string Description { get; }

        public virtual string[] Aliases { get; } = Array.Empty<string>();
        public bool SanitizeResponse { get; } = true;
    }
}
namespace XPSystem.Commands
{
    using System;
    using CommandSystem;
    using MEC;
    using XPSystem.API;

    public class PauseXPCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionInternal("xps.pause"))
            {
                response = "You don't have permission (xps.pause) to use this command.";
                return false;
            }

            Main.Paused = !Main.Paused;
            if (Main.Paused)
            {
#if EXILED
                if (Extensions.HintCoroutineHandle.HasValue)
                    Timing.KillCoroutines(Extensions.HintCoroutineHandle.Value);
#endif
                response = "XP gain paused.";
                return true;
            }

#if EXILED
            Extensions.HintCoroutineHandle = Timing.RunCoroutine(Extensions.HintCoroutine());
#endif
            response = "XP gain unpaused.";
            return true;
        }

        public string Command { get; } = "pause";
        public string[] Aliases { get; } = { "pause_xp" };
        public string Description { get; } = "Pause XP gain for all players.";
    }
}
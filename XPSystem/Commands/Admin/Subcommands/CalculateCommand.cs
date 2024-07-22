namespace XPSystem.Commands.Admin.Subcommands
{
    using System;
    using System.Diagnostics;
    using CommandSystem;
    using XPSystem.API;

    public class CalculateCommand : SanitizedInputCommand
    {
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermissionLS("xps.calculate"))
            {
                response = "You do not have permission (xps.calculate) to run this command.";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: xp calculate <level|xp> <value>";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int value))
            {
                response = "Invalid value.";
                return false;
            }

            Stopwatch stopwatch = new();
            switch (arguments.At(0).ToLower())
            {
                case "l":
                case "lvl":
                case "level":
                    stopwatch.Start();
                    int xp;
                    try
                    {
                        xp = LevelCalculator.GetLevel(value, true);
                    }
                    catch (Exception e)
                    {
                        response = "Failed to calculate level (LevelFunction): " + e;
                        return false;
                    }

                    stopwatch.Stop();
                    response = $"{value} XP is level {xp} in {stopwatch.ElapsedMilliseconds}ms.";
                    return true;
                case "xp":
                    stopwatch.Start();
                    int level;
                    try
                    {
                        level = LevelCalculator.GetXP(value, true);
                    }
                    catch (Exception e)
                    {
                        response = "Failed to calculate XP (XPFunction): " + e;
                        return false;
                    }

                    stopwatch.Stop();
                    response = $"Level {value} needs {level} XP in {stopwatch.ElapsedMilliseconds}ms.";
                    return true;
                default:
                    response = "Usage: xp calculate <level|xp> <value>";
                    return false;
            }
        }

        public override string Command { get; } = "calculate";
        public override string[] Aliases { get; } = { "calc", "c" };
        public override string Description { get; } = "Calculate levels or XP values.";
    }
}
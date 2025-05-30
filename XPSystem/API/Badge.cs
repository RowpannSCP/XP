// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace XPSystem.API
{
    using System;

    public class Badge
    {
        public string Text { get; set; }
        public string Color { get; set; }

        public void ValidateColor()
        {
            bool valid =
                Color == "default" ||
                (Enum.TryParse(Color, true, out Misc.PlayerInfoColorTypes color) && Misc.AllowedColors.ContainsKey(color)) ||
                Misc.AllowedColors.ContainsValue(Color);

            if (!valid)
                XPAPI.LogWarn("Badge color may be invalid: " + Color);
        }
    }
}
namespace XPSystem.Config.Models
{
    using System;
    using XPSystem.API;

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
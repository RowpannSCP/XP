namespace XPSystem.API.Enums
{
    using System.Collections.Generic;

    /// <summary>
    /// All colors allowed for server roles and custominfo.
    /// </summary>
    public enum ApprovedColor
    {
        Pink,
        Red,
        Brown,
        Silver,
        LightGreen,
        Crimson,
        Cyan,
        Aqua,
        DeepPink,
        Tomato,
        Yellow,
        Magenta,
        BlueGreen,
        Orange,
        Lime,
        Green,
        Emerald,
        Carmine,
        Nickel,
        Mint,
        ArmyGreen,
        Pumpkin,
        Black,
        White
    }
    
    public static class ApprovedColorExtensions
    {
        /// <summary>
        /// Returns the hex value of the <see cref="ApprovedColor"/>.
        /// </summary>
        public static string ToHex(this ApprovedColor color)
        {
            return ToHexDict[color];
        }

        public static readonly Dictionary<ApprovedColor, string> ToHexDict = new()
        {
            {
                ApprovedColor.Pink,
                "#FF96DE"
            },
            {
                ApprovedColor.Red,
                "#C50000"
            },
            {
                ApprovedColor.Brown,
                "#944710"
            },
            {
                ApprovedColor.Silver,
                "#A0A0A0"
            },
            {
                ApprovedColor.LightGreen,
                "#32CD32"
            },
            {
                ApprovedColor.Crimson,
                "#DC143C"
            },
            {
                ApprovedColor.Cyan,
                "#00B7EB"
            },
            {
                ApprovedColor.Aqua,
                "#00FFFF"
            },
            {
                ApprovedColor.DeepPink,
                "#FF1493"
            },
            {
                ApprovedColor.Tomato,
                "#FF6448"
            },
            {
                ApprovedColor.Yellow,
                "#FAFF86"
            },
            {
                ApprovedColor.Magenta,
                "#FF0090"
            },
            {
                ApprovedColor.BlueGreen,
                "#4DFFB8"
            },
            {
                ApprovedColor.Orange,
                "#FF9966"
            },
            {
                ApprovedColor.Lime,
                "#BFFF00"
            },
            {
                ApprovedColor.Green,
                "#228B22"
            },
            {
                ApprovedColor.Emerald,
                "#50C878"
            },
            {
                ApprovedColor.Carmine,
                "#960018"
            },
            {
                ApprovedColor.Nickel,
                "#727472"
            },
            {
                ApprovedColor.Mint,
                "#98FB98"
            },
            {
                ApprovedColor.ArmyGreen,
                "#4B5320"
            },
            {
                ApprovedColor.Pumpkin,
                "#EE7600"
            },
            {
                ApprovedColor.Black,
                "#000000"
            },
            {
                ApprovedColor.White,
                "#FFFFFF"
            }
        };
    }
}
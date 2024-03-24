namespace XPSystem.Config
{
    public static class XPConfigExtensions
    {
        /// <summary>
        /// <see cref="string.Format(string, object[)"/> but cooler (not really).
        /// Its literally the same thing.
        /// </summary>
        public static string FormatTranslation(this string translation, params object[] args)
        {
            return string.Format(translation, args);
        }
    }
}
namespace HardwareCore
{
    public static class StringExtensions
    {
        public static string After(this string value, string delimiter)
        {
            var ix = value.IndexOf(delimiter);

            if(ix < 0 || ix == (value.Length - delimiter.Length))
            {
                return string.Empty;
            }

            return value.Substring(ix + delimiter.Length);
        }
        public static string Before(this string value, string delimiter)
        {
            var ix = value.IndexOf(delimiter);

            if(ix < 0)
            {
                return value;
            }

            return value.Substring(0, ix);
        }
    }
}
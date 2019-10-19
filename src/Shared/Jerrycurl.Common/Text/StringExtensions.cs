namespace Jerrycurl.Text
{
    internal static class StringExtensions
    {
        public static string ToCapitalCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}

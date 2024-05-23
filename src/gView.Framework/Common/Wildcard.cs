namespace gView.Framework.Common
{
    public class WildcardEx : global::System.Text.RegularExpressions.Regex
    {
        public WildcardEx(string pattern, global::System.Text.RegularExpressions.RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        {
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Escape(pattern).
                        Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}

using System.Text.RegularExpressions;

namespace BloggingCorner.Helper
{
    public static class RemoveHtmlHelper
    {
        public static string RemoveHelperTag(string input)
        {
            return Regex.Replace(input, "<.*>|&.*;", string.Empty);
        }
    }
}

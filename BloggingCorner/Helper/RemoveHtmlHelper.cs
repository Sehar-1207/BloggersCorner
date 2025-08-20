using System.Text.RegularExpressions;

namespace BloggingCorner.Helper
{
    public static class RemoveHtmlHelper
    {
        public static string RemoveHelperTag(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // remove HTML tags
            string withoutTags = Regex.Replace(input, "<.*?>", string.Empty);

            // remove HTML entities like &nbsp; &amp;
            string withoutEntities = Regex.Replace(withoutTags, "&[a-zA-Z0-9#]+;", string.Empty);

            return withoutEntities.Trim();
        }
    }
}

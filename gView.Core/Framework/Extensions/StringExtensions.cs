using System;

namespace gView.Framework.Extensions
{
    static public class StringExtensions
    {
        static public string XmlEncoded(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            return System.Web.HttpUtility.HtmlEncode(value);
        }
    }
}

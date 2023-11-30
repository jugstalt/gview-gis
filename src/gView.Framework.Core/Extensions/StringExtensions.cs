using System;

namespace gView.Framework.Core.Extensions
{
    static public class StringExtensions
    {
        static public string XmlEncoded(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return System.Web.HttpUtility.HtmlEncode(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class StringExtensions
    {
        static public string UrlEncodePassword(this string password)
        {
            if (password != null && password.IndexOfAny("+/=&".ToCharArray()) > 0)
            {
                password = System.Web.HttpUtility.UrlEncode(password);
            }

            return password;
        }

        static public string UrlEncodeWhereClause(this string whereClause)
        {
            if (String.IsNullOrWhiteSpace(whereClause))
            {
                return String.Empty;
            }

            return whereClause.Replace("%", "%25")
                      .Replace("+", "%2B")
                      .Replace("/", "%2F")
                      //.Replace(@"\", "%5C")   // Darf man nicht ersetzen!! Sonst geht beim Kunden der Filter für Usernamen nicht mehr!!!!!!
                      .Replace("&", "%26");
        }
    }
}

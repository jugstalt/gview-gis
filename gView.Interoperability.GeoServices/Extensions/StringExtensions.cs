using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class StringExtensions
    {
        public static string UrlEncodePassword(this string password)
        {
            if (password != null && password.IndexOfAny("+/=&".ToCharArray()) > 0)
            {
                password = System.Web.HttpUtility.UrlEncode(password);
            }

            return password;
        }
    }
}

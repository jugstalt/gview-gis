using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.OGC.Extensions
{
    static internal class StringExtensions
    {
        static public string ToValidXmlTag(this string tag)
        {
            tag = tag.Replace("#", "")
                     .Replace(".", "_")
                     .Replace("()", "");  //Shape.STLength()

            return tag;
        }
    }
}

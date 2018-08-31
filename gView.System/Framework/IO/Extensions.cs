using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.IO
{
    static public class Extensions
    {
        static public string ToPlattformPath(this string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}

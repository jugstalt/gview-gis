using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    static public class Extensions
    {
        static public string ServiceName(this string id)
        {
            if (id.Contains("@"))
                return id.Split('@')[1].Trim();

            return id;
        }

        static public string FolderName(this string id)
        {
            if (id.Contains("@"))
                return id.Split('@')[0].Trim();

            return String.Empty;
        }
    }
}

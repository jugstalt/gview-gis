using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.OGC.XML
{
    public class Globals
    {
        public static string TypeWithoutPrefix(string type)
        {
            if (type.Contains(":"))
            {
                return type.Substring(type.IndexOf(":") + 1, type.Length - type.IndexOf(":") - 1);
            }
            else
            {
                return type;
            }
        }

        public static int IntegerFeatureID(string id)
        {
            if (id.Contains("."))
            {
                id= id.Substring(id.LastIndexOf(".") + 1, id.Length - id.LastIndexOf(".") - 1);
            }
            int ID;
            if (!int.TryParse(id, out ID))
                ID = -1;
            return ID;
        }
    }
}

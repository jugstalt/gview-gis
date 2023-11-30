using System;

namespace gView.Framework.Common
{
    public class TypeHelper
    {
        static public bool Match(Type type, Type hasType)
        {
            if (type == null || hasType == null)
            {
                return false;
            }

            if (type.Equals(hasType))
            {
                return true;
            }

            foreach (Type inter in type.GetInterfaces())
            {
                if (inter.Equals(hasType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

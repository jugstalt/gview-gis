using System;

namespace gView.Framework.system
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

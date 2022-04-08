using gView.Framework.Data;
using System;

namespace gView.Framework.Extensions
{
    static public class FieldExtenstions
    {
        static public object TryConvertType(this IField field, object val)
        {
            if (field == null || val == null)
            {
                return null;
            }

            try
            {
                switch (field.type)
                {
                    case FieldType.biginteger:
                        return Convert.ToInt64(val);
                    case FieldType.integer:
                        return Convert.ToInt32(val);
                    case FieldType.smallinteger:
                        return Convert.ToInt16(val);
                    case FieldType.boolean:
                        return Convert.ToBoolean(val);
                        //case FieldType.Double:
                        //    return Convert.ToDouble(val);
                        //case FieldType.Float:
                        //    return Convert.ToSingle(val);
                }
            }
            catch
            {

            }

            return val;
        }
    }
}

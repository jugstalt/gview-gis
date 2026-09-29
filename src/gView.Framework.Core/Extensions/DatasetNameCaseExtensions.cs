using gView.Framework.Core.Data;
using System;

namespace gView.Framework.Core.Extensions
{
    static public class DatasetNameCaseExtensions
    {
        /// <summary>
        /// Name of a new feature class/table: upper/classNameUpper => upper case, lower/classNameLower => lower case.
        /// fieldNames* don't touch the class name.
        /// </summary>
        static public string ToClassName(this DatasetNameCase nameCase, string name)
        {
            if (name == null)
            {
                return null;
            }

            switch (nameCase)
            {
                case DatasetNameCase.upper:
                case DatasetNameCase.classNameUpper:
                    return name.ToUpper();
                case DatasetNameCase.lower:
                case DatasetNameCase.classNameLower:
                    return name.ToLower();
                default:
                    return name;
            }
        }

        /// <summary>
        /// Name of a new field/column: upper/fieldNamesUpper => upper case, lower/fieldNamesLower => lower case.
        /// className* don't touch field names.
        /// </summary>
        static public string ToFieldName(this DatasetNameCase nameCase, string name)
        {
            if (name == null)
            {
                return null;
            }

            switch (nameCase)
            {
                case DatasetNameCase.upper:
                case DatasetNameCase.fieldNamesUpper:
                    return name.ToUpper();
                case DatasetNameCase.lower:
                case DatasetNameCase.fieldNamesLower:
                    return name.ToLower();
                default:
                    return name;
            }
        }

        /// <summary>
        /// The <see cref="UseDatasetNameCaseAttribute"/> of a dataset type (inherited from base classes),
        /// <see cref="DatasetNameCase.ignore"/> if there is none.
        /// </summary>
        static public DatasetNameCase GetDatasetNameCase(this Type type)
        {
            if (type == null)
            {
                return DatasetNameCase.ignore;
            }

            var attribute = (UseDatasetNameCaseAttribute)Attribute.GetCustomAttribute(type, typeof(UseDatasetNameCaseAttribute), true);

            return attribute?.Value ?? DatasetNameCase.ignore;
        }
    }
}

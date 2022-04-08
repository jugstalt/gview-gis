using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Interoperability.GeoServices.Extensions
{
    static public class FieldsExtensions
    {
        static public IEnumerable<string> FieldsNames(this string fieldsString)
        {
            return (fieldsString ?? String.Empty)
                        .Split(',')
                        .Select(f => f.Trim());
        }

        static public IEnumerable<string> CheckAllowedFunctions(this IEnumerable<string> fieldNames, ITableClass tableClass)
        {
            foreach (var fieldName in fieldNames)
            {
                if (fieldName.IsFieldFunction())
                {
                    if (tableClass.Fields.FindField(fieldName) == null)
                    {
                        throw new Exception($"Forbidden field function detected: { fieldName }");
                    }
                }
            }

            return fieldNames;
        }

        static public bool IsFieldFunction(this string fieldName) => !String.IsNullOrEmpty(fieldName) && (fieldName.Contains("(") || fieldName.Contains(")") || fieldName.Contains(" ") || fieldName.Contains("."));
    }
}

using gView.Framework.Core.Data;
using gView.Framework.Data.Extensions;
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

        /// <summary>
        /// Checks the outFields of a request: only "*" and fields of the table are allowed
        /// (field functions only if the table defines a field with exactly this name).
        /// Returns the field names with the table's spelling.
        /// </summary>
        static public IEnumerable<string> CheckAllowedFunctions(this IEnumerable<string> fieldNames, ITableClass tableClass, bool isFeatureServer)
        {
            if (isFeatureServer && fieldNames.Count() == 1 && fieldNames.First() == "*")
            {
                fieldNames = tableClass.Fields.ToEnumerable()
                        .Select(f => f.name)
                        .Where(n => !n.IsFieldFunction())  // FeatureServer Query do not return FieldFunctions link STArea, STLength, ...
                        .ToArray();
            }

            return tableClass.ProjectRequestFieldNames(fieldNames);
        }

        static public bool IsFieldFunction(this string fieldName) => !String.IsNullOrEmpty(fieldName) && (fieldName.Contains("(") || fieldName.Contains(")") || fieldName.Contains(" ") || fieldName.Contains("."));
    }
}

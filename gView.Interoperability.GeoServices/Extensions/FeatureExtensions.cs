using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class FeatureExtensions
    {
        static public Dictionary<string, object> AttributesDictionary(this IRow row)
        {
            var dict = new Dictionary<string, object>();

            if(row?.Fields!=null)
            {
                foreach(var fieldValue in row.Fields)
                {
                    dict[fieldValue.Name] = fieldValue.Value;
                }
            }

            return dict;
        }
    }
}

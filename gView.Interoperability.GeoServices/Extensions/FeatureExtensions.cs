using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class FeatureExtensions
    {
        static public Dictionary<string, object> AttributesDictionary(this IRow row)
        {
            var dict = new Dictionary<string, object>();

            if (row?.Fields != null)
            {
                foreach (var fieldValue in row.Fields)
                {
                    dict[fieldValue.Name] = fieldValue.Value;
                }
            }

            return dict;
        }

        static public bool IsGridResponse(this Dictionary<string, object> attributes)
        {
            if (attributes?.Keys != null)
            {
                int bandNumber = 0;
                var countBands = attributes.Keys.Where(k => k.StartsWith("band") && int.TryParse(k.Substring(4), out bandNumber)).Count();

                return countBands == 1 && attributes.Keys.Contains("band1") && attributes.Keys.Contains("Bands");
            }

            return false;
        }

        static public Dictionary<string, object> AppendRasterAttributes(this Dictionary<string, object> attributes)
        {
            if (attributes.IsGridResponse())
            {
                var result = new Dictionary<string, object>()
                {
                    { "Stretched value","" },
                    { "Pixel Value", attributes["band1"] }
                };

                foreach (var key in attributes.Keys)
                {
                    result[key] = attributes[key];
                }

                return result;
            }

            return attributes;
        }
    }
}

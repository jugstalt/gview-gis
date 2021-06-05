using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Json.Features.Geometry;
using Newtonsoft.Json;
using System;

namespace gView.Interoperability.GeoServices.Extensions
{
    static public class JsonGeometryExtensions
    {
        static public JsonGeometry ToJsonGeometry(this string geometryString)
        {
            geometryString = geometryString.Trim();

            if (geometryString.StartsWith("{") || geometryString.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<JsonGeometry>(geometryString);
            }

            var coords = geometryString.Split(',');
            if (coords.Length == 2)
            {
                return new JsonGeometry()
                {
                    X = coords[0].ToDouble(),
                    Y = coords[1].ToDouble()
                };
            }
            else if(coords.Length == 4)
            {
                return new JsonGeometry()
                {
                    XMin = coords[0].ToDouble(),
                    YMin = coords[1].ToDouble(),
                    XMax = coords[2].ToDouble(),
                    YMax = coords[3].ToDouble(),
                };
            }
            else
            {
                throw new Exception($"Unkown geometry { geometryString }");
            }
        }

    }
}

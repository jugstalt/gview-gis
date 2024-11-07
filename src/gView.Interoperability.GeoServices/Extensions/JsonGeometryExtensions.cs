using gView.Framework.Common;
using gView.Framework.Common.Json;
using gView.Interoperability.GeoServices.Rest.DTOs.Features.Geometry;
using System;

namespace gView.Interoperability.GeoServices.Extensions
{
    static public class JsonGeometryExtensions
    {
        static public JsonGeometryDTO ToJsonGeometry(this string geometryString)
        {
            geometryString = geometryString.Trim();

            if (geometryString.StartsWith("{") || geometryString.StartsWith("["))
            {
                return JSerializer.Deserialize<JsonGeometryDTO>(geometryString);
            }

            var coords = geometryString.Split(',');
            if (coords.Length == 2)
            {
                return new JsonGeometryDTO()
                {
                    X = coords[0].ToDouble(),
                    Y = coords[1].ToDouble()
                };
            }
            else if (coords.Length == 4)
            {
                return new JsonGeometryDTO()
                {
                    XMin = coords[0].ToDouble(),
                    YMin = coords[1].ToDouble(),
                    XMax = coords[2].ToDouble(),
                    YMax = coords[3].ToDouble(),
                };
            }
            else
            {
                throw new Exception($"Unkown geometry {geometryString}");
            }
        }

    }
}

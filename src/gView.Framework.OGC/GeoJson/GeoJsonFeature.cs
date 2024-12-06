using gView.Framework.Common.Json;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.OGC.GeoJson
{
    public class GeoJsonFeature
    {
        [JsonPropertyName("oid")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Oid { get; set; }

        [JsonPropertyName("type")]
        public string Type { get { return "Feature"; } set { } }

        [JsonPropertyName("geometry")]
        public JsonGeometry Geometry { get; set; }

        [JsonPropertyName("properties")]
        public object Properties { get; set; }

        public IGeometry ToGeometry()
        {
            if (this.Geometry?.Coordinates == null)
            {
                return null;
            }

            switch (this.Geometry.Type.ToString().ToLower())
            {
                case "point":
                    double[] coordinates = JSerializer.Deserialize<double[]>(this.Geometry.Coordinates.ToString());
                    return new Point(coordinates[0], coordinates[1]);
                case "linestring":
                    double[][] lineString = JSerializer.Deserialize<double[][]>(this.Geometry.Coordinates.ToString());
                    Polyline line = new Polyline();
                    Path path = new Path();
                    line.AddPath(path);

                    for (int i = 0, to = lineString.GetLength(0); i < to; i++)
                    {
                        path.AddPoint(new Point(lineString[i][0], lineString[i][1]));
                    }

                    return line;
                case "multilinestring":
                    double[][][] paths = this.Geometry.Coordinates is double[][][]?
                        (double[][][])this.Geometry.Coordinates :
                        JSerializer.Deserialize<double[][][]>(this.Geometry.Coordinates.ToString());

                    Polyline multiLine = new Polyline();
                    for (int p = 0, to = paths.GetLength(0); p < to; p++)
                    {
                        Path multiLinePath = new();
                        multiLine.AddPath(multiLinePath);

                        var coords = paths[p];
                        for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                        {
                            multiLinePath.AddPoint(new Point(coords[i][0], coords[i][1]));
                        }
                    }
                    return multiLine;
                case "multipolygon":
                case "polygon":
                    object polygonString = this.Geometry.Coordinates;
                    if (!(polygonString is double[][]) && !(polygonString is double[][][]))
                    {
                        try
                        {
                            polygonString = JSerializer.Deserialize<double[][]>(this.Geometry.Coordinates.ToString());
                        }
                        catch
                        {
                            try
                            {
                                polygonString = JSerializer.Deserialize<double[][][]>(this.Geometry.Coordinates.ToString());
                            }
                            catch
                            {
                                polygonString = JSerializer.Deserialize<double[][][][]>(this.Geometry.Coordinates.ToString());
                            }
                        }
                    }
                    Polygon polygon = new Polygon();

                    if (polygonString is double[][])
                    {
                        Ring ring = new Ring();
                        polygon.AddRing(ring);

                        var coords = (double[][])polygonString;
                        for (int i = 0, to = coords.GetLength(0); i < to; i++)
                        {
                            ring.AddPoint(new Point(coords[i][0], coords[i][1]));
                        }
                    }
                    else if (polygonString is double[][][])
                    {
                        var rings = (double[][][])polygonString;
                        for (int r = 0, to = rings.GetLength(0); r < to; r++)
                        {
                            Ring ring = new Ring();
                            polygon.AddRing(ring);

                            var coords = rings[r];
                            for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                            {
                                ring.AddPoint(new Point(coords[i][0], coords[i][1]));
                            }
                        }
                    }
                    else if (polygonString is double[][][][]) // Multipolygon
                    {
                        var subPolygons = (double[][][][])polygonString;

                        for (int p = 0, to_p = subPolygons.GetLength(0); p < to_p; p++)
                        {
                            var rings = subPolygons[p];
                            for (int r = 0, to_r = rings.GetLength(0); r < to_r; r++)
                            {
                                Ring ring = new Ring();
                                polygon.AddRing(ring);

                                var coords = rings[r];
                                for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                                {
                                    ring.AddPoint(new Point(coords[i][0], coords[i][1]));
                                }
                            }
                        }
                    }

                    return polygon;
            }

            return null;
        }

        public void FromShape(IGeometry shape)
        {
            if (shape is IPoint)
            {
                this.Geometry = new JsonGeometry()
                {
                    Type = "Point",
                    Coordinates = new double[] { ((Point)shape).X, ((Point)shape).Y }
                };
            }
            else if (shape is IPolyline)
            {
                var polyline = (IPolyline)shape;
                if (polyline.PathCount == 1)
                {
                    this.Geometry = new JsonGeometry()
                    {
                        Type = "LineString",
                        Coordinates = polyline[0]
                            .ToArray()
                            .Select(point => new double[] { point.X, point.Y })
                            .ToArray()
                    };
                }
                else if (polyline.PathCount > 1)
                {
                    this.Geometry = new JsonGeometry()
                    {
                        Type = "MultiLineString",
                        Coordinates = polyline
                            .ToArray()  // Paths
                            .Select(path => path
                                                .ToArray()  // Points
                                                .Select(point => new double[] { point.X, point.Y })
                                                .ToArray())
                            .ToArray()
                    };
                }
            }
            else if (shape is IPolygon)
            {
                var polygon = (IPolygon)shape;

                this.Geometry = new JsonGeometry()
                {
                    Type = "Polygon",
                    Coordinates = polygon
                        .ToArray()  // Rings
                        .Select(ring => ring
                                            .ToArray() // Points
                                            .Select(point => new double[] { point.X, point.Y })
                                            .ToArray())
                        .ToArray()
                };
            }
        }

        public T GetPropery<T>(string propertyName)
        {
            object val = this[propertyName];
            if (val == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(val, typeof(T));
        }

        public object this[string propertyName]
        {
            get
            {
                var properties = this.Properties;
                object result = null;

                foreach (var name in propertyName.Split('.'))
                {
                    if (properties is JsonElement jsonElement)
                    {
                        var val = jsonElement.GetProperty(name);

                        result = val.ValueKind switch
                        {
                            JsonValueKind.Object => val,
                            JsonValueKind.Array => val,
                            JsonValueKind.Number => val.ToString().Contains(".") ? val.GetDouble() : val.GetInt32(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            JsonValueKind.String => val.ToString(),
                            _ => val
                        };

                        properties = val;
                    }
                    else if (properties is IDictionary<string, object> dict)
                    {
                        if (dict.ContainsKey(propertyName))
                        {
                            result = dict[propertyName];
                        }
                    }
                }

                return result;
            }
        }

        public object GetFirstOrDefault(IEnumerable<string> propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var result = this[propertyName];
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public bool HasProperty(string propertyName)
        {
            return this[propertyName] != null;
        }

        public void PropertiesToDict()
        {
            if (this.Properties is JsonElement)
            {
                this.Properties = JSerializer.Deserialize<Dictionary<string, object>>(this.Properties.ToString());
            }
            else if (this.Properties == null)
            {
                this.Properties = new Dictionary<string, object>();
            }
            else
            {
                throw new Exception("Unknown properties type");
            }
        }

        public void SetProperty(string propertyName, object val)
        {
            if (this.Properties is JsonElement)
            {
                PropertiesToDict();
            }

            if (this.Properties is Dictionary<string, object>)
            {
                ((Dictionary<string, object>)this.Properties)[propertyName] = val;
            }
            else
            {
                throw new Exception("Properties is not a Dictionary<string, object>");
            }
        }

        public bool TrySetProperty(string propertyName, object val, bool ignorNull)
        {
            if (ignorNull && val == null)
            {
                return true;
            }

            try
            {
                SetProperty(propertyName, val);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Classes

        public class JsonGeometry
        {
            [JsonPropertyName("type")]
            virtual public string Type { get; set; }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            virtual public object Coordinates { get; set; }
        }

        public class JsonPointGeometry : JsonGeometry
        {
            [JsonPropertyName("type")]
            override public string Type { get { return "Point"; } set { } }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            override public /*double[]*/object Coordinates { get; set; }
        }

        public class JsonMultiPointGeometry : JsonGeometry
        {
            [JsonPropertyName("type")]
            override public string Type { get { return "MultiPoint"; } set { } }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            override public /*double[][]*/ object Coordinates { get { return CoordinatesArray.ToArray(); } set { } }

            [JsonIgnore]
            public List<double[]> CoordinatesArray = new List<double[]> { };
        }

        public class JsonCoordinatesListGeometry : JsonGeometry
        {
            [JsonIgnore]
            public List<JsonMultiPointGeometry> CoorinatesListArray = new List<JsonMultiPointGeometry>();
        }

        public class JsonLineStringGeometry : JsonMultiPointGeometry
        {
            [JsonPropertyName("type")]
            override public string Type { get { return "LineString"; } set { } }
        }

        public class JsonPolygonGeometry : JsonCoordinatesListGeometry
        {
            [JsonPropertyName("type")]
            override public string Type { get { return "Polygon"; } set { } }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public override object Coordinates
            {
                get
                {
                    List<object> rings = new List<object>();
                    foreach (var r in CoorinatesListArray)
                    {
                        var ring = new List<object>();
                        for (int c = 0; c < r.CoordinatesArray.Count; c++)
                        {
                            ring.Add(r.CoordinatesArray[c]);
                        }
                        rings.Add(ring);
                    }

                    return rings.ToArray();
                }

                set
                {
                }
            }
        }

        public class JsonMultiLineStringGeometry : JsonGeometry
        {
            public JsonMultiLineStringGeometry(JsonLineStringGeometry[] lineStrings)
            {
                this.Coordinates = lineStrings;
            }

            [JsonPropertyName("type")]
            override public string Type { get { return "MultiLineString"; } set { } }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            override public /*JsonLineStringGeometry[]*/ object Coordinates { get; set; }
        }

        public class JsonMultiPolygonGeometry : JsonGeometry
        {
            public JsonMultiPolygonGeometry(JsonPolygonGeometry[] polygons)
            {
                Coordinates = polygons;
            }

            [JsonPropertyName("type")]
            override public string Type { get { return "MultiPolygon"; } set { } }

            [JsonPropertyName("coordinates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            override public /*JsonPolygonGeometry[]*/ object Coordinates { get; set; }
        }

        #endregion
    }
}

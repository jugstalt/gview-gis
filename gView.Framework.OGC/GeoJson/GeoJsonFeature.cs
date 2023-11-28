using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.OGC.GeoJson
{
    public class GeoJsonFeature
    {
        [JsonProperty("oid", NullValueHandling = NullValueHandling.Ignore)]
        public string Oid { get; set; }

        [JsonProperty("type")]
        public string Type { get { return "Feature"; } set { } }

        [JsonProperty("geometry")]
        public JsonGeometry Geometry { get; set; }

        [JsonProperty("properties")]
        public object Properties { get; set; }

        public IGeometry ToGeometry()
        {
            if (this.Geometry?.coordinates == null)
            {
                return null;
            }

            switch (this.Geometry.type.ToString().ToLower())
            {
                case "point":
                    double[] coordinates = JsonConvert.DeserializeObject<double[]>(this.Geometry.coordinates.ToString());
                    return new Point(coordinates[0], coordinates[1]);
                case "linestring":
                    double[][] lineString = JsonConvert.DeserializeObject<double[][]>(this.Geometry.coordinates.ToString());
                    Polyline line = new Polyline();
                    Path path = new Path();
                    line.AddPath(path);

                    for (int i = 0, to = lineString.GetLength(0); i < to; i++)
                    {
                        path.AddPoint(new Point(lineString[i][0], lineString[i][1]));
                    }

                    return line;
                case "polygon":
                    object polygonString = null;
                    try
                    {
                        polygonString = JsonConvert.DeserializeObject<double[][]>(this.Geometry.coordinates.ToString());
                    }
                    catch
                    {
                        polygonString = JsonConvert.DeserializeObject<double[][][]>(this.Geometry.coordinates.ToString());
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

                            var coords = (double[][])rings[r];
                            for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                            {
                                ring.AddPoint(new Point(coords[i][0], coords[i][1]));
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
                    type = "Point",
                    coordinates = new double[] { ((Point)shape).X, ((Point)shape).Y }
                };
            }
            else if (shape is IPolyline)
            {
                var polyline = (IPolyline)shape;
                if (polyline.PathCount == 1)
                {
                    this.Geometry = new JsonGeometry()
                    {
                        type = "LineString",
                        coordinates = polyline[0]
                            .ToArray()
                            .Select(point => new double[] { point.X, point.Y })
                            .ToArray()
                    };
                }
                else if (polyline.PathCount > 1)
                {
                    this.Geometry = new JsonGeometry()
                    {
                        type = "MultiLineString",
                        coordinates = polyline
                            .ToArray()  // Paths
                            .Select(path =>
                                                path
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
                    type = "Polygon",
                    coordinates = polygon
                        .ToArray()  // Rings
                        .Select(ring =>
                                                ring
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
                    if (properties is Newtonsoft.Json.Linq.JObject)
                    {
                        object val = ((Newtonsoft.Json.Linq.JObject)properties)[name];

                        if (val is Newtonsoft.Json.Linq.JValue)
                        {
                            result = ((Newtonsoft.Json.Linq.JValue)val).Value;
                        }
                        else
                        {
                            result = val;
                        }

                        properties = val;
                    }
                    else if (properties is IDictionary<string, object>)
                    {
                        if (((IDictionary<string, object>)properties).ContainsKey(propertyName))
                        {
                            result = ((IDictionary<string, object>)properties)[propertyName];
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
            if (this.Properties is Newtonsoft.Json.Linq.JObject)
            {
                this.Properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(this.Properties.ToString());
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
            if (this.Properties is Newtonsoft.Json.Linq.JObject)
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
            virtual public string type { get; set; }

            [JsonProperty("coordinates", NullValueHandling = NullValueHandling.Ignore)]
            virtual public object coordinates { get; set; }
        }

        public class JsonPointGeometry : JsonGeometry
        {
            override public string type { get { return "Point"; } set { } }
            override public /*double[]*/object coordinates { get; set; }
        }

        public class JsonMultiPointGeometry : JsonGeometry
        {
            override public string type { get { return "MultiPoint"; } set { } }
            override public /*double[][]*/ object coordinates { get { return CoordinatesArray.ToArray(); } set { } }

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
            override public string type { get { return "LineString"; } set { } }
        }

        public class JsonPolygonGeometry : JsonCoordinatesListGeometry
        {
            override public string type { get { return "Polygon"; } set { } }

            public override object coordinates
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
                this.coordinates = lineStrings;
            }

            override public string type { get { return "MultiLineString"; } set { } }
            override public /*JsonLineStringGeometry[]*/ object coordinates { get; set; }
        }

        public class JsonMultiPolygonGeometry : JsonGeometry
        {
            public JsonMultiPolygonGeometry(JsonPolygonGeometry[] polygons)
            {
                coordinates = polygons;
            }

            override public string type { get { return "MultiLineString"; } set { } }
            override public /*JsonPolygonGeometry[]*/ object coordinates { get; set; }
        }

        #endregion
    }
}

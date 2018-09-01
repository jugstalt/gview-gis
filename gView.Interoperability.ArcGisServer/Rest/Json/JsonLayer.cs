using gView.Framework.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonLayer
    {
        public JsonLayer()
        {
            this.SubLayers = new JsonLayer[0];
        }

        [JsonProperty("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometryType")]
        public string GeometryType { get; set; }

        [JsonProperty("subLayers")]
        public JsonLayer[] SubLayers { get; set; }

        [JsonProperty("parentLayer")]
        public JsonLayer ParentLayer { get; set; }

        [JsonProperty("minScale")]
        public double MinScale { get; set; }

        [JsonProperty("maxScale")]
        public double MaxScale { get; set; }

        [JsonProperty("defaultVisibility")]
        public bool DefaultVisibility { get; set; }

        [JsonProperty("fields")]
        public JsonField[] Fields { get; set; }

        [JsonProperty("extent")]
        public JsonExtent Extent { get; set; }

        [JsonProperty("drawingInfo")]
        public JsonDrawingInfo DrawingInfo { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (this.ParentLayer != null)
                {
                    sb.Append(this.ParentLayer.FullName);
                    sb.Append("\\");
                }
                sb.Append(this.Name);

                return sb.ToString();
            }
        }

        [JsonIgnore]
        public string ParentFullName
        {
            get
            {
                if (this.ParentLayer != null)
                    return ParentLayer.FullName + @"\";

                return String.Empty;
            }
        }

        #region Static Members

        static public EsriGeometryType ToGeometryType(geometryType type)
        {
            switch (type)
            {
                case geometryType.Point:
                    return EsriGeometryType.esriGeometryPoint;
                case geometryType.Multipoint:
                    return EsriGeometryType.esriGeometryMultipoint;
                case geometryType.Polyline:
                    return EsriGeometryType.esriGeometryPolyline;
                case geometryType.Polygon:
                    return EsriGeometryType.esriGeometryPolygon;
                case geometryType.Aggregate:
                    return EsriGeometryType.esriGeometryBag;
                case geometryType.Envelope:
                    return EsriGeometryType.esriGeometryEnvelope;
                case geometryType.Unknown:
                case geometryType.Network:
                default:
                    return EsriGeometryType.esriGeometryNull;
            }
        }

        #endregion
    }
}

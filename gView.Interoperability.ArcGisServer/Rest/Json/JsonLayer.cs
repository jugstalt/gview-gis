using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public class JsonLayer
    {
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

        public string ParentFullName
        {
            get
            {
                if (this.ParentLayer != null)
                    return ParentLayer.FullName + @"\";

                return String.Empty;
            }
        }
    }
}

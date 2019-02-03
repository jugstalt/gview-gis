using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonDrawingInfo
    {

        public JsonRenderer Renderer
        {
            get;set;
        }

        #region Classes

        public class JsonRenderer
        {
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "field1")]
            public string Field1 { get; set; }
            [JsonProperty(PropertyName = "field2")]
            public string Field2 { get; set; }
            [JsonProperty(PropertyName = "field3")]
            public string Field3 { get; set; }

            [JsonProperty(PropertyName = "fieldDelimiter")]
            public string FieldDelimiter { get; set; }
        }

        #endregion
    }
}

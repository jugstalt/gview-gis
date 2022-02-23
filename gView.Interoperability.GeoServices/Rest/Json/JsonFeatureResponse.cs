using gView.Interoperability.GeoServices.Rest.Json.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureResponse : JsonStopWatch
    {
        [JsonProperty("displayFieldName")]
        public string DisplayFieldName { get; set; }

        [JsonProperty("fieldAliases")]
        public dynamic FieldAliases { get; set; } // object

        [JsonProperty("geometryType")]
        public string GeometryType { get; set; }

        [JsonProperty("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }

        [JsonProperty("fields")]
        public Field[] Fields { get; set; }

        [JsonProperty("features")]
        public JsonFeature[] Features { get; set; }

        [JsonProperty("exceededTransferLimit")]
        public bool ExceededTransferLimit { get; set; }

        #region Classes

        public class Field
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
            
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
            public int? Length { get; set; }

            //public class VType
            //{
            //    [JsonProperty("value")]
            //    public string Value { get; set; }
            //}
        }

        #endregion
    }
}

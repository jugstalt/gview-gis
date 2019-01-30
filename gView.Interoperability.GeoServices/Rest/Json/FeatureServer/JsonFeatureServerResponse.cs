using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.FeatureServer
{
    public class JsonFeatureServerResponse
    {
        [JsonProperty(PropertyName = "addResults", NullValueHandling = NullValueHandling.Ignore)]
        public JsonResponse[] AddResults { get; set; }

        [JsonProperty(PropertyName = "updateResults", NullValueHandling = NullValueHandling.Ignore)]
        public JsonResponse[] UpdateResults { get; set; }

        [JsonProperty(PropertyName = "deleteResults", NullValueHandling = NullValueHandling.Ignore)]
        public JsonResponse[] DeleteResults { get; set; }

        #region Classses

        public class JsonResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "objectId", NullValueHandling = NullValueHandling.Ignore)]
            public int? ObjectId { get; set; }

            [JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
            public JsonError Error { get; set; }
        }

        public class JsonError
        {
            [JsonProperty(PropertyName = "code")]
            public int Code { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }
        }

        #endregion
    }
}

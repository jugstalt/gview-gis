using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.FeatureServer
{
    public class JsonFeatureServerResponse
    {
        [JsonPropertyName("addResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonResponse[] AddResults { get; set; }

        [JsonPropertyName("updateResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonResponse[] UpdateResults { get; set; }

        [JsonPropertyName("deleteResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonResponse[] DeleteResults { get; set; }

        #region Classses

        public class JsonResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("objectId")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public int? ObjectId { get; set; }

            [JsonPropertyName("error")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public JsonError Error { get; set; }
        }

        public class JsonError
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

        #endregion
    }
}

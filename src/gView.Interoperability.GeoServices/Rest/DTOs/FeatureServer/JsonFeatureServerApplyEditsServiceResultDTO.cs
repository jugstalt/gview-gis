using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    /// <summary>
    /// One entry of the service level applyEdits response array
    /// (.../FeatureServer/applyEdits).
    /// </summary>
    public class JsonFeatureServerApplyEditsServiceResultDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("addResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonFeatureServerResponseDTO.JsonResponse[] AddResults { get; set; }

        [JsonPropertyName("updateResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonFeatureServerResponseDTO.JsonResponse[] UpdateResults { get; set; }

        [JsonPropertyName("deleteResults")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonFeatureServerResponseDTO.JsonResponse[] DeleteResults { get; set; }
    }
}

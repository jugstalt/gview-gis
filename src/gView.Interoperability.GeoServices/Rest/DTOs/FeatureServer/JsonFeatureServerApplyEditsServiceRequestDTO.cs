using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    /// <summary>
    /// Request body of the service level applyEdits operation
    /// (.../FeatureServer/applyEdits). This is the variant used by ArcGIS Pro.
    /// https://developers.arcgis.com/rest/services-reference/enterprise/apply-edits-feature-service/
    /// </summary>
    public class JsonFeatureServerApplyEditsServiceRequestDTO
    {
        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonPropertyName("edits")]
        public JsonEdits[] Edits { get; set; }

        [FormInput(FormInputAttribute.InputTypes.Text)]
        [JsonPropertyName("rollbackOnFailure")]
        public bool RollbackOnFailure { get; set; } = true;

        [FormInput(FormInputAttribute.InputTypes.Text)]
        [JsonPropertyName("useGlobalIds")]
        public bool UseGlobalIds { get; set; } = false;

        public class JsonEdits
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("adds")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public JsonFeatureDTO[] Adds { get; set; }

            [JsonPropertyName("updates")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public JsonFeatureDTO[] Updates { get; set; }

            /// <summary>
            /// Array of objectIds (numbers) or objects like { "objectId": 1 } / { "globalId": "..." }.
            /// </summary>
            [JsonPropertyName("deletes")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public JsonElement? Deletes { get; set; }
        }
    }
}

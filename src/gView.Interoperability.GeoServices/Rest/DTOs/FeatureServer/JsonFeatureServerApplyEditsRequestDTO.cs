using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    /// <summary>
    /// Request body of the layer level applyEdits operation
    /// (.../FeatureServer/{layerId}/applyEdits).
    /// https://developers.arcgis.com/rest/services-reference/enterprise/apply-edits-feature-service/
    /// </summary>
    public class JsonFeatureServerApplyEditsRequestDTO : JsonFeatureServerEditRequesDTO
    {
        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonPropertyName("adds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonFeatureDTO[] Adds { get; set; }

        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonPropertyName("updates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonFeatureDTO[] Updates { get; set; }

        /// <summary>
        /// The objectIds (or globalIds) of the features to be deleted.
        /// Accepted formats: "1,2,3", "[1,2,3]" or "[{\"objectId\":1}]".
        /// </summary>
        [FormInput(FormInputAttribute.InputTypes.TextBox)]
        [JsonPropertyName("deletes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Deletes { get; set; }

        [FormInput(FormInputAttribute.InputTypes.Text)]
        [JsonPropertyName("rollbackOnFailure")]
        public bool RollbackOnFailure { get; set; } = true;

        [FormInput(FormInputAttribute.InputTypes.Text)]
        [JsonPropertyName("useGlobalIds")]
        public bool UseGlobalIds { get; set; } = false;
    }
}

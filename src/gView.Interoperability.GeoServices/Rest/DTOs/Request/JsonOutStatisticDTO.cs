using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Request
{
    /// <summary>
    /// One element of the <c>outStatistics</c> query parameter.
    /// </summary>
    public class JsonOutStatisticDTO
    {
        [JsonPropertyName("statisticType")]
        public string StatisticType { get; set; }

        [JsonPropertyName("onStatisticField")]
        public string OnStatisticField { get; set; }

        [JsonPropertyName("outStatisticFieldName")]
        public string OutStatisticFieldName { get; set; }
    }
}

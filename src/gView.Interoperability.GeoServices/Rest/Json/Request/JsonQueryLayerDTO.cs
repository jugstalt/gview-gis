using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Request
{
    public class JsonQueryLayerDTO
    {
        //[JsonPropertyName("text")]
        //public string Text { get; set; }
        [JsonPropertyName("where")]
        public string Where { get; set; }

        [JsonPropertyName("geometry")]
        public string Geometry { get; set; }

        [JsonPropertyName("geometryType")]
        public string geometryType { get; set; }

        [JsonPropertyName("inSR")]
        public string InSRef { get; set; }

        [JsonPropertyName("relationParam")]
        public string relationParam { get; set; }

        [JsonPropertyName("objectIds")]
        public string ObjectIds { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("units")]
        public string units { get; set; }

        [JsonPropertyName("outFields")]
        public string OutFields { get; set; }

        [JsonPropertyName("returnGeometry")]
        public bool ReturnGeometry { get; set; }

        [JsonPropertyName("maxAllowableOffset")]
        public int MaxAllowableOffset { get; set; }

        [JsonPropertyName("geometryPrecision")]
        public int GeometryPrecision { get; set; }

        [JsonPropertyName("outSR")]
        public string OutSRef { get; set; }

        [JsonPropertyName("returnIdsOnly")]
        public bool ReturnIdsOnly { get; set; }

        [JsonPropertyName("returnCountOnly")]
        public bool ReturnCountOnly { get; set; }

        [JsonPropertyName("returnExtentOnly")]
        public bool ReturnExtentOnly { get; set; }

        [JsonPropertyName("orderByFields")]
        public string OrderByFields { get; set; }

        [JsonPropertyName("outStatistics")]
        public string OutStatistics { get; set; }

        [JsonPropertyName("groupByFieldsForStatistics")]
        public string GroupByFieldsForStatistics { get; set; }

        [JsonPropertyName("returnZ")]
        public bool ReturnZ { get; set; }

        [JsonPropertyName("returnM")]
        public bool ReturnM { get; set; }

        [JsonPropertyName("returnDistinctValues")]
        public bool ReturnDistinctValues { get; set; }

        [JsonPropertyName("returnTrueCurves")]
        public bool ReturnTrueCurves { get; set; }

        [JsonPropertyName("resultOffset")]
        public int ResultOffset { get; set; }

        [JsonPropertyName("resultRecordCount")]
        public int ResultRecordCount { get; set; }

        [JsonPropertyName("datumTransformation")]
        public string DatumTransformation { get; set; }

        [JsonPropertyName("rangeValues")]
        public string RangeValues { get; set; }

        [JsonPropertyName("quantizationParameters")]
        public string QuantizationParameters { get; set; }

        [JsonPropertyName("parameterValues")]
        public string ParameterValues { get; set; }

        [JsonPropertyName("historicMoment")]
        public long HistoricMoment { get; set; }

        [JsonPropertyName("layerId")]
        public int LayerId { get; set; }

        [JsonPropertyName("f")]
        [FormInput(Values = new string[] { "json", "pjson", "geojson" })]
        public string OutputFormat { get; set; }
    }
}

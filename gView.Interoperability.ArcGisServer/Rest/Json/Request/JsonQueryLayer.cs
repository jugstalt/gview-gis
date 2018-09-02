using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json.Request
{
    public class JsonQueryLayer
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "geometry")]
        public string Geometry { get; set; }

        [JsonProperty(PropertyName = "geometryType")]
        public string geometryType { get; set; }

        [JsonProperty(PropertyName = "inSR")]
        public string InSRef { get; set; }

        [JsonProperty(PropertyName = "relationParam")]
        public string relationParam { get; set; }

        [JsonProperty(PropertyName = "where")]
        public string Where { get; set; }

        [JsonProperty(PropertyName = "objectIds")]
        public string ObjectIds { get; set; }

        [JsonProperty(PropertyName = "time")]
        public long Time { get; set; }

        [JsonProperty(PropertyName = "distance")]
        public double Distance { get; set; }

        [JsonProperty(PropertyName = "units")]
        public string units { get; set; }

        [JsonProperty(PropertyName = "outFields")]
        public string OutFields { get; set; }

        [JsonProperty(PropertyName = "returnGeometry")]
        public bool ReturnGeometry { get; set; }

        [JsonProperty(PropertyName = "maxAllowableOffset")]
        public int MaxAllowableOffset { get; set; }

        [JsonProperty(PropertyName = "geometryPrecision")]
        public int GeometryPrecision { get; set; }

        [JsonProperty(PropertyName = "outSR")]
        public string OutSRef { get; set; }

        [JsonProperty(PropertyName = "returnIdsOnly")]
        public bool ReturnIdsOnly { get; set; }

        [JsonProperty(PropertyName = "returnCountOnly")]
        public bool ReturnCountOnly { get; set; }

        [JsonProperty(PropertyName = "returnExtentOnly")]
        public bool ReturnExtentOnly { get; set; }

        [JsonProperty(PropertyName = "orderByFields")]
        public string OrderByFields { get; set; }

        [JsonProperty(PropertyName = "outStatistics")]
        public string OutStatistics { get; set; }

        [JsonProperty(PropertyName = "groupByFieldsForStatistics")]
        public string GroupByFieldsForStatistics { get; set; }

        [JsonProperty(PropertyName = "returnZ")]
        public bool ReturnZ { get; set; }

        [JsonProperty(PropertyName = "returnM")]
        public bool ReturnM { get; set; }

        [JsonProperty(PropertyName = "returnDistinctValues")]
        public bool ReturnDistinctValues { get; set; }

        [JsonProperty(PropertyName = "returnTrueCurves")]
        public bool ReturnTrueCurves { get; set; }

        [JsonProperty(PropertyName = "resultOffset")]
        public int ResultOffset { get; set; }

        [JsonProperty(PropertyName = "resultRecordCount")]
        public int ResultRecordCount { get; set; }

        [JsonProperty(PropertyName = "datumTransformation")]
        public string DatumTransformation { get; set; }

        [JsonProperty(PropertyName = "rangeValues")]
        public string RangeValues { get; set; }

        [JsonProperty(PropertyName = "quantizationParameters")]
        public string QuantizationParameters { get; set; }

        [JsonProperty(PropertyName = "parameterValues")]
        public string ParameterValues { get; set; }

        [JsonProperty(PropertyName = "historicMoment")]
        public long HistoricMoment { get; set; }
    }
}

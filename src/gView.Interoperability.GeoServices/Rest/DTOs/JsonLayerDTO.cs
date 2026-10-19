using gView.Framework.Core.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    [ServiceMethod("Query", "query")]
    public class JsonLayerDTO
    {
        public JsonLayerDTO()
        {
            this.SubLayers = new JsonLayerLinkDTO[0];
            this.Capabilities = "Map,Query";
        }

        [JsonPropertyName("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("geometryType")]
        public string GeometryType { get; set; }

        [JsonPropertyName("hasZ")]
        public bool HasZ { get; set; }
        [JsonPropertyName("hasM")]
        public bool HasM { get; set; }

        [JsonPropertyName("subLayers")]
        public JsonLayerLinkDTO[] SubLayers { get; set; }

        [JsonPropertyName("parentLayer")]
        public JsonLayerLinkDTO ParentLayer { get; set; }

        [JsonPropertyName("minScale")]
        public double MinScale { get; set; }

        [JsonPropertyName("maxScale")]
        public double MaxScale { get; set; }

        [JsonPropertyName("defaultVisibility")]
        public bool DefaultVisibility { get; set; }

        [JsonPropertyName("fields")]
        public JsonFieldDTO[] Fields { get; set; }

        [JsonPropertyName("extent")]
        public JsonExtentDTO Extent { get; set; }

        [JsonPropertyName("drawingInfo")]
        public JsonDrawingInfoDTO DrawingInfo { get; set; }

        [JsonPropertyName("capabilities")]
        public string Capabilities { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("copyrightText")]
        public string CopyrightText { get; set; }

        [JsonPropertyName("supportsDynamicLegends")]
        public bool SupportsDynamicLegends { get; set; } = false;

        /// <summary>The query operation accepts <c>outStatistics</c> / <c>groupByFieldsForStatistics</c>.</summary>
        [JsonPropertyName("supportsStatistics")]
        public bool SupportsStatistics { get; set; }

        /// <summary>The query operation accepts <c>orderByFields</c>.</summary>
        [JsonPropertyName("supportsAdvancedQueries")]
        public bool SupportsAdvancedQueries { get; set; }

        [JsonPropertyName("advancedQueryCapabilities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AdvancedQueryCapabilitiesClass AdvancedQueryCapabilities { get; set; }

        #region Classes

        public class AdvancedQueryCapabilitiesClass
        {
            public AdvancedQueryCapabilitiesClass()
            {
                this.SupportsPagination = true;
                this.SupportsReturningQueryExtent = true;
                this.SupportsDistinct = true;
                this.SupportsOrderBy = true;
                this.SupportsQueryWithResultType = true;
                this.SupportsReturningGeometryCentroid = true;
                this.SupportsSqlExpression = true;
            }

            [JsonPropertyName("supportsPagination")]
            public bool SupportsPagination { get; set; }

            [JsonPropertyName("supportsTrueCurve")]
            public bool SupportsTrueCurve { get; set; }

            [JsonPropertyName("supportsQueryWithDistance")]
            public bool SupportsQueryWithDistance { get; set; }

            [JsonPropertyName("supportsReturningQueryExtent")]
            public bool SupportsReturningQueryExtent { get; set; }

            [JsonPropertyName("supportsStatistics")]
            public bool SupportsStatistics { get; set; }

            [JsonPropertyName("supportsHavingClause")]
            public bool SupportsHavingClause { get; set; }

            [JsonPropertyName("supportsCountDistinct")]
            public bool SupportsCountDistinct { get; set; }

            [JsonPropertyName("supportsOrderBy")]
            public bool SupportsOrderBy { get; set; }

            [JsonPropertyName("supportsDistinct")]
            public bool SupportsDistinct { get; set; }

            [JsonPropertyName("supportsQueryWithResultType")]
            public bool SupportsQueryWithResultType { get; set; }

            [JsonPropertyName("supportsReturningGeometryCentroid")]
            public bool SupportsReturningGeometryCentroid { get; set; }

            [JsonPropertyName("supportsSqlExpression")]
            public bool SupportsSqlExpression { get; set; }
        }

        #endregion

        //[JsonIgnore]
        //public string FullName
        //{
        //    get
        //    {
        //        StringBuilder sb = new StringBuilder();

        //        if (this.ParentLayer != null)
        //        {
        //            sb.Append(this.ParentLayer.FullName);
        //            sb.Append("\\");
        //        }
        //        sb.Append(this.Name);

        //        return sb.ToString();
        //    }
        //}

        //[JsonIgnore]
        //public string ParentFullName
        //{
        //    get
        //    {
        //        if (this.ParentLayer != null)
        //            return ParentLayer.FullName + @"/";

        //        return String.Empty;
        //    }
        //}

        public GeometryType GetGeometryType()
        {
            switch (GeometryType)
            {
                case "esriGeometryPoint":
                    return Framework.Core.Geometry.GeometryType.Point;
                case "esriGeometryMultipoint":
                    return Framework.Core.Geometry.GeometryType.Multipoint;
                case "esriGeometryPolyline":
                    return Framework.Core.Geometry.GeometryType.Polyline;
                case "esriGeometryPolygon":
                    return Framework.Core.Geometry.GeometryType.Polygon;
                case "esriGeometryBag":
                    return Framework.Core.Geometry.GeometryType.Aggregate;
                case "esriGeometryEnvelope":
                    return Framework.Core.Geometry.GeometryType.Envelope;
            }

            return Framework.Core.Geometry.GeometryType.Unknown;
        }

        #region Static Members

        static public EsriGeometryType ToGeometryType(GeometryType type)
        {
            switch (type)
            {
                case Framework.Core.Geometry.GeometryType.Point:
                    return EsriGeometryType.esriGeometryPoint;
                case Framework.Core.Geometry.GeometryType.Multipoint:
                    return EsriGeometryType.esriGeometryMultipoint;
                case Framework.Core.Geometry.GeometryType.Polyline:
                    return EsriGeometryType.esriGeometryPolyline;
                case Framework.Core.Geometry.GeometryType.Polygon:
                    return EsriGeometryType.esriGeometryPolygon;
                case Framework.Core.Geometry.GeometryType.Aggregate:
                    return EsriGeometryType.esriGeometryBag;
                case Framework.Core.Geometry.GeometryType.Envelope:
                    return EsriGeometryType.esriGeometryEnvelope;
                case Framework.Core.Geometry.GeometryType.Unknown:
                case Framework.Core.Geometry.GeometryType.Network:
                default:
                    return EsriGeometryType.esriGeometryNull;
            }
        }

        #endregion
    }

    public class JsonLayerLinkDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

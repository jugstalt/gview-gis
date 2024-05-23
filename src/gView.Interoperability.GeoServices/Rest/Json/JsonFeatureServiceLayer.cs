using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("Query", "query")]
    [ServiceMethod("Add Feature", "addfeatures")]
    [ServiceMethod("Update Feature", "updatefeatures")]
    [ServiceMethod("Delete Feature", "deletefeatures")]
    public class JsonFeatureServerLayer : JsonLayer
    {
        public JsonFeatureServerLayer()
            : base()
        {
            this.Relationships = new object[0];
            this.ArchivingInfo = new ArchivingInfoClass();
            this.AdvancedQueryCapabilities = new AdvancedQueryCapabilitiesClass();
            this.SupportsAdvancedQueries = true;
            this.SupportsValidateSQL = true;

            MaxRecordCount = 1000;
            MaxRecordCountFactor = 1;
            StandardMaxRecordCount = 32000;
        }

        [JsonPropertyName("gv_is_editable")]
        public bool IsEditable { get; set; }

        [JsonPropertyName("gv_edit_operations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] EditOperations { get; set; }

        [JsonPropertyName("parentLayer")]
        new public JsonLayerLink ParentLayer { get; set; }

        [JsonIgnore]
        public int ParentLayerId => ParentLayer == null ? -1 : ParentLayer.Id;

        [JsonPropertyName("editFieldsInfo")]
        public object EidtFiedsInfo { get; set; }

        [JsonPropertyName("ownershipBasedAccessControlForFeatures")]
        public object OwnershipBasedAccessControlForFeatures { get; set; }

        [JsonPropertyName("syncCanReturnChanges")]
        public bool SyncCanReturnChanges { get; set; }

        [JsonPropertyName("relationships")]
        public IEnumerable<object> Relationships { get; set; }

        [JsonPropertyName("supportsRollbackOnFailureParameter")]
        public bool SupportsRollbackOnFailureParameter { get; set; }

        [JsonPropertyName("archivingInfo")]
        public ArchivingInfoClass ArchivingInfo { get; set; }

        [JsonPropertyName("supportsStatistics")]
        public bool SupportsStatistics { get; set; }

        [JsonPropertyName("supportsAdvancedQueries")]
        public bool SupportsAdvancedQueries { get; set; }

        [JsonPropertyName("supportsValidateSQL")]
        public bool SupportsValidateSQL { get; set; }

        [JsonPropertyName("supportsCoordinatesQuantization")]
        public bool SupportsCoordinatesQuantization { get; set; }

        [JsonPropertyName("supportsCalculate")]
        public bool SupportsCalculate { get; set; }

        [JsonPropertyName("advancedQueryCapabilities")]
        public AdvancedQueryCapabilitiesClass AdvancedQueryCapabilities { get; set; }

        [JsonPropertyName("maxRecordCount")]
        public int MaxRecordCount { get; set; }

        [JsonPropertyName("standardMaxRecordCount")]
        public int StandardMaxRecordCount { get; set; }

        [JsonPropertyName("maxRecordCountFactor")]
        public int MaxRecordCountFactor { get; set; }

        [JsonPropertyName("supportedQueryFormats")]
        public string SupportedQueryFormats => "JSON, geoJSON";

        [JsonPropertyName("useStandardizedQueries")]
        public bool UseStandardizedQueries => true;

        #region Classes

        public class ArchivingInfoClass
        {

            public ArchivingInfoClass()
            {
                this.SupportsQueryWithHistoricMoment = false;
                this.StartArchivingMoment = -1;
            }

            [JsonPropertyName("supportsQueryWithHistoricMoment")]
            public bool SupportsQueryWithHistoricMoment { get; set; }

            [JsonPropertyName("startArchivingMoment")]
            public int StartArchivingMoment { get; set; }
        }

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
    }
}

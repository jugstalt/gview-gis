using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        [JsonProperty("gv_is_editable")]
        public bool IsEditable { get; set; }

        [JsonProperty("gv_edit_operations", NullValueHandling = NullValueHandling.Ignore)]
        public string[] EditOperations { get; set; }

        [JsonProperty("parentLayer")]
        new public JsonLayerLink ParentLayer { get; set; }

        [JsonIgnore]
        public int ParentLayerId => ParentLayer == null ? -1 : ParentLayer.Id;

        [JsonProperty("editFieldsInfo")]
        public object EidtFiedsInfo { get; set; }

        [JsonProperty("ownershipBasedAccessControlForFeatures")]
        public object OwnershipBasedAccessControlForFeatures { get; set; }

        [JsonProperty("syncCanReturnChanges")]
        public bool SyncCanReturnChanges { get; set; }

        [JsonProperty("relationships")]
        public IEnumerable<object> Relationships { get; set; }

        [JsonProperty("supportsRollbackOnFailureParameter")]
        public bool SupportsRollbackOnFailureParameter { get; set; }

        [JsonProperty("archivingInfo")]
        public ArchivingInfoClass ArchivingInfo { get; set; }

        [JsonProperty("supportsStatistics")]
        public bool SupportsStatistics { get; set; }

        [JsonProperty("supportsAdvancedQueries")]
        public bool SupportsAdvancedQueries { get; set; }

        [JsonProperty("supportsValidateSQL")]
        public bool SupportsValidateSQL { get; set; }

        [JsonProperty("supportsCoordinatesQuantization")]
        public bool SupportsCoordinatesQuantization { get; set; }

        [JsonProperty("supportsCalculate")]
        public bool SupportsCalculate { get; set; }

        [JsonProperty("advancedQueryCapabilities")]
        public AdvancedQueryCapabilitiesClass AdvancedQueryCapabilities { get; set; }

        [JsonProperty(PropertyName = "maxRecordCount")]
        public int MaxRecordCount { get; set; }

        [JsonProperty("standardMaxRecordCount")]
        public int StandardMaxRecordCount {get;set;}

        [JsonProperty("maxRecordCountFactor")]
        public int MaxRecordCountFactor { get; set; }

        [JsonProperty(PropertyName = "supportedQueryFormats")]
        public string SupportedQueryFormats => "JSON";

        [JsonProperty("useStandardizedQueries")]
        public bool UseStandardizedQueries => true;

        #region Classes

        public class ArchivingInfoClass
        {
            
            public ArchivingInfoClass()
            {
                this.SupportsQueryWithHistoricMoment = false;
                this.StartArchivingMoment = -1;
            }

            [JsonProperty("supportsQueryWithHistoricMoment")]
            public bool SupportsQueryWithHistoricMoment { get; set; }

            [JsonProperty("startArchivingMoment")]
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

            [JsonProperty("supportsPagination")]
            public bool SupportsPagination { get; set; }

            [JsonProperty("supportsTrueCurve")]
            public bool SupportsTrueCurve { get; set; }

            [JsonProperty("supportsQueryWithDistance")]
            public bool SupportsQueryWithDistance { get; set; }

            [JsonProperty("supportsReturningQueryExtent")]
            public bool SupportsReturningQueryExtent { get; set; }

            [JsonProperty("supportsStatistics")]
            public bool SupportsStatistics { get; set; }

            [JsonProperty("supportsHavingClause")]
            public bool SupportsHavingClause { get; set; }

            [JsonProperty("supportsCountDistinct")]
            public bool SupportsCountDistinct { get; set; }

            [JsonProperty("supportsOrderBy")]
            public bool SupportsOrderBy { get; set; }

            [JsonProperty("supportsDistinct")]
            public bool SupportsDistinct { get; set; }

            [JsonProperty("supportsQueryWithResultType")]
            public bool SupportsQueryWithResultType { get; set; }

            [JsonProperty("supportsReturningGeometryCentroid")]
            public bool SupportsReturningGeometryCentroid { get; set; }

            [JsonProperty("supportsSqlExpression")]
            public bool SupportsSqlExpression { get; set; }
        }

        #endregion
    }
}

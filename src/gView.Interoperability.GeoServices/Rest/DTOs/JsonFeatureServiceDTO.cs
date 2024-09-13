using System.Text.Json;
using System;
using System.Collections.Generic;
using static gView.Interoperability.GeoServices.Rest.DTOs.JsonMapServiceDTO;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    //[ServiceMethod("ExportMap", "export")]
    public class JsonFeatureServiceDTO
    {
        public JsonFeatureServiceDTO()
        {
            this.Layers = new JsonIdNameDTO[0];
            this.DocumentInfo = new JsonMapServiceDTO.DocumentInfoClass()
            {
                Title = String.Empty,
                Author = String.Empty,
                Comments = String.Empty,
                Category = String.Empty,
                Subject = String.Empty,
                Keywords = String.Empty
            };
            Capabilities = "Create,Query,Update,Delete";
            DatumTransformations = new JsonMapServiceDTO.DatumTransformationsClass[0];
            SupportsDatumTransformation = true;
            Units = "esriMeters";

            MaxRecordCount = 1000;
            MaxRecordCountFactor = 1;

            AllowGeometryUpdates = true;
            Tables = new object[0];
        }

        [JsonPropertyName("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonPropertyName("serviceDescription")]
        public string ServiceDescription { get; set; }

        [JsonPropertyName("copyrightText")]
        public string CopyrightText { get; set; }

        [JsonPropertyName("supportedQueryFormats")]
        public string SupportedQueryFormats => "JSON";

        [JsonPropertyName("layers")]
        public JsonIdNameDTO[] Layers { get; set; }

        [JsonPropertyName("fullExtent")]
        public JsonMapServiceDTO.Extent FullExtent { get; set; }

        [JsonPropertyName("initialExtent")]
        public JsonMapServiceDTO.Extent InitialExtend { get; set; }

        [JsonPropertyName("spatialReference")]
        public SpatialReference SpatialReferenceInstance { get; set; }

        [JsonPropertyName("units")]
        public string Units { get; set; }

        [JsonPropertyName("documentInfo")]
        public JsonMapServiceDTO.DocumentInfoClass DocumentInfo { get; set; }

        [JsonPropertyName("capabilities")]
        public string Capabilities { get; set; }

        [JsonPropertyName("datumTransformations")]
        public JsonMapServiceDTO.DatumTransformationsClass[] DatumTransformations { get; set; }

        [JsonPropertyName("supportsDatumTransformation")]
        public bool SupportsDatumTransformation { get; set; }

        [JsonPropertyName("maxRecordCount")]
        public int MaxRecordCount { get; set; }

        [JsonPropertyName("maxRecordCountFactor")]
        public int MaxRecordCountFactor { get; set; }

        [JsonPropertyName("hasVersionedData")]
        public bool HasVersionedData { get; set; }

        [JsonPropertyName("supportsDisconnectedEditing")]
        public bool SupportsDisconnectedEditing { get; set; }

        [JsonPropertyName("syncEnabled")]
        public bool SyncEnabled { get; set; }

        [JsonPropertyName("allowGeometryUpdates")]
        public bool AllowGeometryUpdates { get; set; }

        [JsonPropertyName("allowTrueCurvesUpdates")]
        public bool AllowTrueCurvesUpdates { get; set; }

        [JsonPropertyName("onlyAllowTrueCurveUpdatesByTrueCurveClients")]
        public bool OnlyAllowTrueCurveUpdatesByTrueCurveClients { get; set; }

        [JsonPropertyName("supportsApplyEditsWithGlobalIds")]
        public bool SupportsApplyEditsWithGlobalIds { get; set; }

        [JsonPropertyName("supportsTrueCurve")]
        public bool supportsTrueCurve { get; set; }

        [JsonPropertyName("tables")]
        public IEnumerable<object> Tables { get; set; }

        [JsonPropertyName("enableZDefaults")]
        public bool EnableZDefaults { get; set; }

        [JsonPropertyName("allowUpdateWithoutMValues")]
        public bool AllowUpdateWithoutMValues { get; set; }

        #region Classes

        #endregion
    }
}

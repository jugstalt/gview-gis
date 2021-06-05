using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static gView.Interoperability.GeoServices.Rest.Json.JsonMapService;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    //[ServiceMethod("ExportMap", "export")]
    public class JsonFeatureService
    {
        public JsonFeatureService()
        {
            this.Layers = new JsonIdName[0];
            this.DocumentInfo = new JsonMapService.DocumentInfoClass()
            {
                Title = String.Empty,
                Author = String.Empty,
                Comments = String.Empty,
                Category = String.Empty,
                Subject = String.Empty,
                Keywords = String.Empty
            };
            Capabilities = "Create,Query,Update,Delete";
            DatumTransformations = new JsonMapService.GeoTransformation[0];
            SupportsDatumTransformation = true;

            MaxRecordCount = 1000;
        }

        [JsonProperty(PropertyName = "currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty(PropertyName = "serviceDescription")]
        public string ServiceDescription { get; set; }

        [JsonProperty(PropertyName = "copyrightText")]
        public string CopyrightText { get; set; }

        [JsonProperty(PropertyName = "supportedQueryFormats")]
        public string SupportedQueryFormats => "JSON";

        [JsonProperty(PropertyName = "layers")]
        public JsonIdName[] Layers { get; set; }

        [JsonProperty(PropertyName = "fullExtent")]
        public JsonMapService.Extent FullExtent { get; set; }

        [JsonProperty(PropertyName = "initialExtent")]
        public JsonMapService.Extent InitialExtend { get; set; }

        [JsonProperty(PropertyName = "spatialReference")]
        public SpatialReference SpatialReferenceInstance { get; set; }

        [JsonProperty(PropertyName = "units")]
        public string Units { get; set; }

        [JsonProperty(PropertyName = "documentInfo")]
        public JsonMapService.DocumentInfoClass DocumentInfo { get; set; }

        [JsonProperty(PropertyName = "capabilities")]
        public string Capabilities { get; set; }

        [JsonProperty(PropertyName = "datumTransformations")]
        public JsonMapService.GeoTransformation[] DatumTransformations { get; set; }

        [JsonProperty(PropertyName = "supportsDatumTransformation")]
        public bool SupportsDatumTransformation{get;set;}

        [JsonProperty(PropertyName = "maxRecordCount")]
        public int MaxRecordCount { get; set; }

        #region Classes

        #endregion
    }
}

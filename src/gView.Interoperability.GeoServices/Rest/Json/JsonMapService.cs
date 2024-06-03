using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("ExportMap", "export")]
    [ServiceMethod("All Layers", "layers")]
    [ServiceMethod("Legend", "legend")]
    [ServiceMethod("Identify", "identify")]
    public class JsonMapService
    {
        public JsonMapService()
        {
            this.Layers = new Layer[0];
            this.Tables = new Table[0];
            this.DocumentInfo = new DocumentInfoClass()
            {
                Title = String.Empty,
                Author = String.Empty,
                Comments = String.Empty,
                Category = String.Empty,
                Subject = String.Empty,
                Keywords = String.Empty
            };
            SupportedImageFormats = "PNG32,PNG24,PNG,JPG";
            Capabilities = "Map,Query";
            SupportedQueryFormats = "JSON";
            DatumTransformations = new DatumTransformationsClass[0];
            SupportsDatumTransformation = true;
            Units = "esriMeters";

            MaxRecordCount = 1000;
            MaxImageWidth = MaxImageHeight = 4096;
            SupportedExtensions = String.Empty;

            this.MapName = "Layers";
        }

        [JsonPropertyName("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonPropertyName("mapName")]
        public string MapName { get; set; }

        [JsonPropertyName("serviceDescription")]
        public string ServiceDescription { get; set; }

        [JsonPropertyName("copyrightText")]
        public string CopyrightText { get; set; }

        [JsonPropertyName("supportsDynamicLayers")]
        public bool SupportsDynamicLayers => true;

        [JsonPropertyName("layers")]
        public Layer[] Layers { get; set; }

        [JsonPropertyName("tables")]
        public Table[] Tables { get; set; }

        [JsonPropertyName("singleFusedMapCache")]
        public bool SingleFusedMapCache => false;

        [JsonPropertyName("spatialReference")]
        public SpatialReference SpatialReferenceInstance { get; set; }

        [JsonPropertyName("fullExtent")]
        public Extent FullExtent { get; set; }

        [JsonPropertyName("initialExtent")]
        public JsonMapService.Extent InitialExtend { get; set; }

        [JsonPropertyName("minScale")]
        public int MinScale { get; set; }

        [JsonPropertyName("maxScale")]
        public int MaxScale { get; set; }

        [JsonPropertyName("units")]
        public string Units { get; set; }

        [JsonPropertyName("supportedImageFormatTypes")]
        public string SupportedImageFormats { get; set; }

        [JsonPropertyName("documentInfo")]
        public DocumentInfoClass DocumentInfo { get; set; }

        [JsonPropertyName("capabilities")]
        public string Capabilities { get; set; }

        [JsonPropertyName("supportedQueryFormats")]
        public string SupportedQueryFormats { get; set; }

        [JsonPropertyName("exportTilesAllowed")]
        public bool exportTilesAllowed => false;

        [JsonPropertyName("datumTransformations")]
        public DatumTransformationsClass[] DatumTransformations { get; set; }

        [JsonPropertyName("supportsDatumTransformation")]
        public bool SupportsDatumTransformation { get; set; }

        [JsonPropertyName("maxRecordCount")]
        public int MaxRecordCount { get; set; }

        [JsonPropertyName("maxImageHeight")]
        public int MaxImageHeight { get; set; }

        [JsonPropertyName("maxImageWidth")]
        public int MaxImageWidth { get; set; }

        [JsonPropertyName("supportedExtensions")]
        public string SupportedExtensions { get; set; }

        #region Classes

        [YamlGroupBy("ParentLayerId")]
        public class Layer : JsonIdName
        {
            public Layer()
            {
                this.ParentLayerId = -1;
            }


            [JsonPropertyName("parentLayerId")]
            public int ParentLayerId { get; set; }

            [JsonPropertyName("defaultVisibility")]
            public bool DefaultVisibility { get; set; }

            [JsonPropertyName("subLayerIds")]
            public int[] SubLayersIds { get; set; }

            [JsonPropertyName("minScale")]
            public double MinScale { get; set; }

            [JsonPropertyName("maxScale")]
            public double MaxScale { get; set; }

            [JsonPropertyName("type")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string LayerType { get; set; }

            [JsonPropertyName("geometryType")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string GeometryType { get; set; }
        }

        public class Table { }

        public class Extent
        {
            [JsonPropertyName("xmin")]
            public double XMin { get; set; }

            [JsonPropertyName("ymin")]
            public double YMin { get; set; }

            [JsonPropertyName("xmax")]
            public double XMax { get; set; }

            [JsonPropertyName("ymax")]
            public double YMax { get; set; }

            [JsonPropertyName("spatialReference")]
            public SpatialReference SpatialReference { get; set; }
        }

        public class SpatialReference
        {
            public SpatialReference()
            {

            }

            public SpatialReference(int wkid)
            {
                this.Wkid = this.LatestWkid = wkid;
                //this.Wkt = "PROJCS[\"Austria_Gauss_Krueger_M34_Nord_5Mio\",GEOGCS[\"GCS_BESSEL_AUT\",DATUM[\"D_BESSEL_AUT\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",-5000000.0],PARAMETER[\"central_meridian\",16.33333333],PARAMETER[\"scale_factor\",1.0],PARAMETER[\"latitude_of_origin\",0.0],UNIT[\"Meter\",1.0]]";
            }

            [JsonPropertyName("wkt")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Wkt { get; set; }

            [JsonPropertyName("wkid")]
            public int Wkid { get; set; }

            [JsonPropertyName("latestWkid")]
            public int LatestWkid { get; set; }
        }

        public class DocumentInfoClass
        {
            [JsonPropertyName("Title")]
            public string Title { get; set; }

            [JsonPropertyName("Author")]
            public string Author { get; set; }

            [JsonPropertyName("Comments")]
            public string Comments { get; set; }

            [JsonPropertyName("Subject")]
            public string Subject { get; set; }

            [JsonPropertyName("Category")]
            public string Category { get; set; }

            [JsonPropertyName("AntialiasingMode")]
            public string AntialiasingMode => "None";

            [JsonPropertyName("TextAntialiasingMode")]
            public string TextAliasingMode => "Force";

            [JsonPropertyName("Keywords")]
            public string Keywords { get; set; }
        }

        public class DatumTransformationsClass
        {
            [JsonPropertyName("geoTransforms")]
            public GeoTransformation[] GeoTransforms { get; set; }
        }

        public class GeoTransformation : SpatialReference
        {
            [JsonPropertyName("geoTransforms")]
            public bool TransformForward { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        #endregion
    }
}

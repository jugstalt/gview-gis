using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Request
{
    public class JsonExportMap
    {
        public JsonExportMap()
        {
            Dpi = 96D;
            Transparent = true;
            ImageFormat = "png";
        }

        public void InitForm(IServiceMap map)
        {
            if (map != null)
            {
                if (map.MapElements != null)
                {
                    Framework.Geometry.Envelope fullExtent = null;

                    foreach (var layer in map.MapElements)
                    {
                        IEnvelope envelope = null;
                        if (layer.Class is IFeatureClass && ((IFeatureClass)layer.Class).Envelope != null)
                        {
                            envelope = ((IFeatureClass)layer.Class).Envelope;
                        }
                        else if (layer.Class is IRasterClass && ((IRasterClass)layer.Class).Polygon != null)
                        {
                            envelope = ((IRasterClass)layer.Class).Polygon.Envelope;
                        }

                        if (envelope != null)
                        {
                            if (fullExtent == null)
                            {
                                fullExtent = new Framework.Geometry.Envelope(envelope);
                            }
                            else
                            {
                                fullExtent.Union(envelope);
                            }
                        }
                    }

                    if (fullExtent != null)
                    {
                        BBox = $"{fullExtent.MinX.ToDoubleString()},{fullExtent.MinY.ToDoubleString()},{fullExtent.MaxX.ToDoubleString()},{fullExtent.MaxY.ToDoubleString()}";
                    }
                }

                if (map.Display?.SpatialReference != null)
                {
                    BBoxSRef = map.Display.SpatialReference.Name.ToLower().Replace("epsg:", "");
                }

                Size = "800,600";
            }
        }

        [JsonPropertyName("bbox")]
        public string BBox { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("dpi")]
        public double Dpi { get; set; }

        [JsonPropertyName("imageSR")]
        public string ImageSRef { get; set; }

        [JsonPropertyName("bboxSR")]
        public string BBoxSRef { get; set; }

        [JsonPropertyName("format")]
        public string ImageFormat { get; set; }

        [JsonPropertyName("layerDefs")]
        public string LayerDefs { get; set; }

        [JsonPropertyName("layers")]
        public string Layers { get; set; }

        [JsonPropertyName("transparent")]
        public bool Transparent { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("layerTimeOptions")]
        public string LayerTimeOptions { get; set; }

        [JsonPropertyName("dynamicLayers")]
        public string DynamicLayers { get; set; }

        [JsonPropertyName("mapScale")]
        public double MapScale { get; set; }

        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }

        [JsonPropertyName("datumTransformations")]
        public string DatumTransformations { get; set; }

        [JsonPropertyName("mapRangeValues")]
        public string MapRageValues { get; set; }

        [JsonPropertyName("layerRangeValues")]
        public string LayerRangeValues { get; set; }

        [JsonPropertyName("layerParameterValues")]
        public string LayerParameterValues { get; set; }

        [JsonPropertyName("historicMoment")]
        public long HistoricMoment { get; set; }

        [JsonPropertyName("f")]
        [FormInput(Values = new string[] { "json", "pjson", "image" })]
        public string OutputFormat { get; set; }

        public string GetContentType()
        {
            switch (this.ImageFormat?.ToLower())
            {


                case "jpg":
                case "jpeg":
                    return "image/jpg";
                default:
                    return "image/png";
            }
        }
    }
}

using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

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
                        BBox = $"{fullExtent.minx.ToDoubleString()},{fullExtent.miny.ToDoubleString()},{fullExtent.maxx.ToDoubleString()},{fullExtent.maxy.ToDoubleString()}";
                    }
                }

                if (map.Display?.SpatialReference != null)
                {
                    BBoxSRef = map.Display.SpatialReference.Name.ToLower().Replace("epsg:", "");
                }

                Size = "800,600";
            }
        }

        [JsonProperty(PropertyName = "bbox")]
        public string BBox { get; set; }

        [JsonProperty(PropertyName = "size")]
        public string Size { get; set; }

        [JsonProperty(PropertyName = "dpi")]
        public double Dpi { get; set; }

        [JsonProperty(PropertyName = "imageSR")]
        public string ImageSRef { get; set; }

        [JsonProperty(PropertyName = "bboxSR")]
        public string BBoxSRef { get; set; }

        [JsonProperty(PropertyName = "format")]
        public string ImageFormat { get; set; }

        [JsonProperty(PropertyName = "layerDefs")]
        public string LayerDefs { get; set; }

        [JsonProperty(PropertyName = "layers")]
        public string Layers { get; set; }

        [JsonProperty(PropertyName = "transparent")]
        public bool Transparent { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string Time { get; set; }

        [JsonProperty(PropertyName = "layerTimeOptions")]
        public string LayerTimeOptions { get; set; }

        [JsonProperty(PropertyName = "dynamicLayers")]
        public string DynamicLayers { get; set; }

        [JsonProperty(PropertyName = "mapScale")]
        public double MapScale { get; set; }

        [JsonProperty(PropertyName = "rotation")]
        public double Rotation { get; set; }

        [JsonProperty(PropertyName = "datumTransformations")]
        public string DatumTransformations { get; set; }

        [JsonProperty(PropertyName = "mapRangeValues")]
        public string MapRageValues { get; set; }

        [JsonProperty(PropertyName = "layerRangeValues")]
        public string LayerRangeValues { get; set; }

        [JsonProperty(PropertyName = "layerParameterValues")]
        public string LayerParameterValues { get; set; }

        [JsonProperty(PropertyName = "historicMoment")]
        public long HistoricMoment { get; set; }

        [JsonProperty(PropertyName = "f")]
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

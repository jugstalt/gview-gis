using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Request
{
    public class JsonIdentifyDTO
    {
        public JsonIdentifyDTO()
        {
            this.ReturnGeometry = true;
            this.ReturnM = true;
            this.ReturnZ = true;
            this.PixelTolerance = PixelTolerance;
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
                        MapExtent = $"{fullExtent.MinX.ToDoubleString()},{fullExtent.MinY.ToDoubleString()},{fullExtent.MaxX.ToDoubleString()},{fullExtent.MaxY.ToDoubleString()}";
                        this.Geometry = $"{fullExtent.Center.X.ToDoubleString()},{fullExtent.Center.Y.ToDoubleString()}";
                    }
                }

                ImageDisplay = "800,600,96";
                if (map?.Display?.SpatialReference != null)
                {
                    this.SRef = map.Display.SpatialReference.EpsgCode.ToString();
                }

                this.geometryType = "esriGeometryPoint";
                this.PixelTolerance = 1;
            }
        }

        [JsonPropertyName("geometry")]
        public string Geometry { get; set; }

        [JsonPropertyName("geometryType")]
        public string geometryType { get; set; }

        [JsonPropertyName("sr")]
        public string SRef { get; set; }

        [JsonPropertyName("layerDefs")]
        public string LayerDefs { get; set; }

        [JsonPropertyName("layers")]
        public string Layers { get; set; }

        [JsonPropertyName("tolerance")]
        public int PixelTolerance { get; set; }

        [JsonPropertyName("mapExtent")]
        public string MapExtent { get; set; }  // <xmin>, <ymin>, <xmax>, <ymax>

        [JsonPropertyName("imageDisplay")]
        public string ImageDisplay { get; set; }  // <width>, <height>, <dpi>

        [JsonPropertyName("returnGeometry")]
        public bool ReturnGeometry { get; set; }

        [JsonPropertyName("returnZ")]
        public bool ReturnZ { get; set; }

        [JsonPropertyName("returnM")]
        public bool ReturnM { get; set; }

        [JsonPropertyName("f")]
        [FormInput(Values = new string[] { "json", "pjson" })]
        public string OutputFormat { get; set; }
    }
}

using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Request
{
    public class JsonIdentify
    {
        public JsonIdentify()
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
                        MapExtent = $"{fullExtent.minx.ToDoubleString()},{fullExtent.miny.ToDoubleString()},{fullExtent.maxx.ToDoubleString()},{fullExtent.maxy.ToDoubleString()}";
                    }
                }

                ImageDisplay = "800,600,96";
                if (map?.Display?.SpatialReference != null)
                {
                    this.SRef = map.Display.SpatialReference.EpsgCode.ToString();
                }

                this.geometryType = "esriGeometryPoint";
                this.Geometry = "400,300";
                this.PixelTolerance = 1;
            }
        }

        [JsonProperty(PropertyName = "geometry")]
        public string Geometry { get; set; }

        [JsonProperty(PropertyName = "geometryType")]
        public string geometryType { get; set; }

        [JsonProperty(PropertyName = "sr")]
        public string SRef { get; set; }

        [JsonProperty(PropertyName = "layerDefs")]
        public string LayerDefs { get; set; }

        [JsonProperty(PropertyName = "layers")]
        public string Layers { get; set; }

        [JsonProperty(PropertyName = "tolerance")]
        public int PixelTolerance { get; set; }

        [JsonProperty(PropertyName = "mapExtent")]
        public string MapExtent { get; set; }  // <xmin>, <ymin>, <xmax>, <ymax>

        [JsonProperty(PropertyName = "imageDisplay")]
        public string ImageDisplay { get; set; }  // <width>, <height>, <dpi>

        [JsonProperty(PropertyName = "returnGeometry")]
        public bool ReturnGeometry { get; set; }

        [JsonProperty(PropertyName = "returnZ")]
        public bool ReturnZ { get; set; }

        [JsonProperty(PropertyName = "returnM")]
        public bool ReturnM { get; set; }

        [JsonProperty(PropertyName = "f")]
        [FormInput(Values = new string[] { "json", "pjson" })]
        public string OutputFormat { get; set; }
    }
}

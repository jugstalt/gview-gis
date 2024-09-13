using System.Linq;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonLayersDTO
    {
        [JsonPropertyName("layers")]
        public JsonLayerDTO[] Layers { get; set; }

        //public void SetParentLayers()
        //{
        //    if (Layers == null)
        //        return;

        //    foreach (var layer in Layers)
        //    {
        //        if (layer.ParentLayer != null)
        //        {
        //            layer.ParentLayer = LayerById(layer.ParentLayer.Id);
        //        }
        //    }
        //}

        public JsonLayerDTO LayerById(int id)
        {
            if (Layers == null)
            {
                return null;
            }

            return (from l in Layers where l.Id == id select l).FirstOrDefault();
        }
    }
}

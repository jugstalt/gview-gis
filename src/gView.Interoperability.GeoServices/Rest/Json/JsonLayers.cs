using Newtonsoft.Json;
using System.Linq;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonLayers
    {
        [JsonProperty("layers")]
        public JsonLayer[] Layers { get; set; }

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

        public JsonLayer LayerById(int id)
        {
            if (Layers == null)
            {
                return null;
            }

            return (from l in Layers where l.Id == id select l).FirstOrDefault();
        }
    }
}

using gView.Interoperability.GeoServices.Rest.Json;

namespace gView.Interoperability.GeoServices.Extensions;
static public class LayersExtensions
{
    static public string LayerFullname(this JsonLayers layers, JsonLayer layer)
    {
        string name = layer.Name;

        while (layer != null && layer.ParentLayer != null)
        {
            layer = layers.LayerById(layer.ParentLayer.Id);
            if (layer != null)
            {
                if (layer.Type == "Annotation Layer" || layer.Type == "Annotation SubLayer")
                {
                    name = $"{layer.Name}/{layer.Name} ({name})";
                }
                else
                {
                    name = $"{layer.Name}/{name}";
                }
            }
        }

        return name;
    }
}

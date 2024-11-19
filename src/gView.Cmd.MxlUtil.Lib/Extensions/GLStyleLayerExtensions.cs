using gView.DataSources.VectorTileCache.Json.GLStyles;
using System.Text.Json;

namespace gView.Cmd.MxlUtil.Lib.Extensions;

static internal class GLStyleLayerExtensions
{
    static public bool RequireSpriteIcon(this IEnumerable<GLStyleLayer> layers, string iconImage)
    {
        return true;  // take all sprites... ther can be function depending on a feature, etc...
                      // can only be determined later on runtime!

        //foreach (var layer in layers)
        //{
        //    if (layer?.Layout?
        //             .IconImage
        //             .CollectStrings()
        //             .Any(s => s.Contains(iconImage) || (s.Contains("{") && s.Contains("}"))) == true)
        //    {
        //        return true;
        //    }
        //}

        //return false;
    }

    static private IEnumerable<string> CollectStrings(this JsonElement? element)
    {
        List<string> strings = new();

        if(!element.HasValue)
        {
            return strings;
        }

        if(element.Value.ValueKind == JsonValueKind.String)
        {
            strings.Add(element.Value.ToString());
            return strings;
        }

        if (element.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var childElement in element.Value.EnumerateArray())
            {
                strings.AddRange(CollectStrings(childElement));
            }
        }

        if (element.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (var jsonProperty in element.Value.EnumerateObject())
            {
                strings.AddRange(CollectStrings(jsonProperty.Value));
            }
        }

        if (strings.Count() > 0) 
        {
        }
        return strings;
    }
}

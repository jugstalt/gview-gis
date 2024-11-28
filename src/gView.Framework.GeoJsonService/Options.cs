using gView.Framework.GeoJsonService.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.GeoJsonService;

internal class Options
{
    public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new RequestConverter()
        }
    };
}

using gView.GeoJsonService.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.GeoJsonService;

public class Options
{
    public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new RequestConverter(),
            new ResponseConverter()
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public static JsonSerializerOptions DeserializerOptions = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new RequestConverter(),
            new ResponseConverter(),
            new SupportedRequestConverter()
        },
        PropertyNameCaseInsensitive = true
    };
}

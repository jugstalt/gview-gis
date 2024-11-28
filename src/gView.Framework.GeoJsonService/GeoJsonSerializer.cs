using gView.Framework.GeoJsonService.DTOs;
using System.Text.Json;

namespace gView.Framework.GeoJsonService;

public class GeoJsonSerializer
{
    static public BaseRequest? DeserializeRequest(string json)
        => JsonSerializer.Deserialize<BaseRequest>(json, Options.SerializerOptions);

    static public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, Options.SerializerOptions);

    static public string Serialize<T>(T geoJsonObject)
        => JsonSerializer.Serialize(geoJsonObject, Options.SerializerOptions);
}

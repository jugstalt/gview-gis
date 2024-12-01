using gView.GeoJsonService.DTOs;
using System.Text.Json;

namespace gView.GeoJsonService;

public class GeoJsonSerializer
{
    static public BaseRequest? DeserializeRequest(string json)
        => JsonSerializer.Deserialize<BaseRequest>(json, Options.DeserializerOptions);

    static public T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, Options.DeserializerOptions);

    static public string Serialize<T>(T geoJsonObject)
        => JsonSerializer.Serialize(geoJsonObject, Options.SerializerOptions);

    static public JsonSerializerOptions JsonSerializerOptions => Options.SerializerOptions;
    static public JsonSerializerOptions JsonDeserializerOptions => Options.DeserializerOptions;
}

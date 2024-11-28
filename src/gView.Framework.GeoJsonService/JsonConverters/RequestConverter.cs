using gView.Framework.GeoJsonService.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.GeoJsonService.JsonConverters;

public class RequestConverter : JsonConverter<BaseRequest>
{
    public override BaseRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var rootElement = jsonDoc.RootElement;
            var type = rootElement.GetProperty("Type").GetString();

            return type switch
            {
                RequestTypes.GetMap 
                    => JsonSerializer.Deserialize<GetMapRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetServiceCapabilities 
                    => JsonSerializer.Deserialize<GetServiceCapabilitiesRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetFeatures
                    => JsonSerializer.Deserialize<GetFeaturesRequest>(rootElement.GetRawText(), options),
                _ => throw new JsonException("Unknown request type")
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

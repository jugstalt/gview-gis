using gView.GeoJsonService.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.GeoJsonService.JsonConverters;

public class RequestConverter : JsonConverter<BaseRequest>
{
    public override BaseRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var rootElement = jsonDoc.RootElement;
            JsonElement typeElement;

            if (!rootElement.TryGetProperty("Type", out typeElement) &&
                !rootElement.TryGetProperty("type", out typeElement)
               )
            {
                throw new JsonException("Response has no Type-property");
            }

            return typeElement.GetString() switch
            {
                RequestTypes.GetInfo
                    => JsonSerializer.Deserialize<GetInfoRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetMap
                    => JsonSerializer.Deserialize<GetMapRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetLegend
                    => JsonSerializer.Deserialize<GetLegendRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetServiceCapabilities
                    => JsonSerializer.Deserialize<GetServiceCapabilitiesRequest>(rootElement.GetRawText(), options),
                RequestTypes.GetFeatures
                    => JsonSerializer.Deserialize<GetFeaturesRequest>(rootElement.GetRawText(), options),
                RequestTypes.EditFeatures
                    => JsonSerializer.Deserialize<EditFeaturesRequest>(rootElement.GetRawText(), options),
                _ => throw new JsonException("Unknown request type")
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

public class ResponseConverter : JsonConverter<BaseResponse>
{
    public override BaseResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var rootElement = jsonDoc.RootElement;
            JsonElement typeElement;

            if (!rootElement.TryGetProperty("Type", out typeElement) &&
                !rootElement.TryGetProperty("type", out typeElement)
               )
            {
                throw new JsonException("Response has no Type-property");
            }

            return typeElement.GetString() switch
            {
                ResponseType.ServiceCapabilities
                    => JsonSerializer.Deserialize<GetServiceCapabilitiesResponse>(rootElement.GetRawText(), options),
                ResponseType.Error
                    => JsonSerializer.Deserialize<ErrorResponse>(rootElement.GetRawText(), options),
                _ => throw new JsonException("Unknown response type")
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

public class SupportedRequestConverter : JsonConverter<SupportedRequest>
{
    public override SupportedRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var rootElement = jsonDoc.RootElement;
            JsonElement nameElement;

            if (!rootElement.TryGetProperty("Name", out nameElement) &&
                !rootElement.TryGetProperty("name", out nameElement)
               )
            {
                throw new JsonException("Response has no Name-property");
            }

            return nameElement.GetString() switch
            {
                RequestProperties.GetMap
                    => JsonSerializer.Deserialize<GetMapRequestProperties>(rootElement.GetRawText(), options),
                RequestProperties.QueryFeatures
                    => JsonSerializer.Deserialize<GetFeaturesRequestProperties>(rootElement.GetRawText(), options),
                RequestProperties.GetLegend
                    => JsonSerializer.Deserialize<GetLegendRequestProperties>(rootElement.GetRawText(), options),
                RequestProperties.EditFeatures
                    => JsonSerializer.Deserialize<EditFeaturesRequestProperties>(rootElement.GetRawText(), options),

                _ => throw new JsonException("Unknown response type")
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, SupportedRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
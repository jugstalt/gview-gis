using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Common.Json.Converters;

public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Bestimmen Sie den Typ basierend auf dem TokenType des Readers
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                // Deserialize 
                var dict = new Dictionary<string, object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return dict;

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();
                        dict[propertyName] = Read(ref reader, typeof(object), options);
                    }
                }
                break;

            case JsonTokenType.StartArray:
                // Deserialisieren Sie das Array rekursiv als List
                var list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        return list;

                    list.Add(Read(ref reader, typeof(object), options));
                }
                break;

            case JsonTokenType.String:
                // Versuchen Sie, Datum/Uhrzeit zu lesen, andernfalls als String
                if (reader.TryGetDateTime(out DateTime datetime))
                    return datetime;
                return reader.GetString();

            case JsonTokenType.Number:
                // Bestimmen Sie, ob es sich um einen Integer oder Double handelt
                if (reader.TryGetInt64(out long l))
                    return l;
                return reader.GetDouble();

            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException($"Unbekannter Token-Typ: {reader.TokenType}");
        }

        throw new JsonException("Fehler beim Lesen des JSON.");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // Verwenden Sie die Standard-Serialisierung
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}


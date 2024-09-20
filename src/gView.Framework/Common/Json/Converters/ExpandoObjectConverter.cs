using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

//
// When deserialize eg JsonFeatureDTO ExpandoObect values 
// are per default JsonElement (=> bad, FeatureServer (INSERT, UPDATE) is not working)
// To convert this values directly to the native types
// is convert is implemented
//
public class ExpandoObjectConverter : JsonConverter<ExpandoObject>
{
    public override ExpandoObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadValue(ref reader) as ExpandoObject;
    }

    private object ReadValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                IDictionary<string, object> expando = new ExpandoObject();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return expando;

                    var propertyName = reader.GetString();
                    reader.Read();
                    expando[propertyName] = ReadValue(ref reader);
                }
                break;

            case JsonTokenType.StartArray:
                var list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        return list;

                    list.Add(ReadValue(ref reader));
                }
                break;

            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long l))
                    return l;
                else if (reader.TryGetDouble(out double d))
                    return d;
                else
                    return reader.GetDecimal();

            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException($"Unknown Token-Type: {reader.TokenType}");
        }

        throw new JsonException("Unknown error when reading JSON.");
    }

    public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (IDictionary<string, object>)value, options);
    }
}

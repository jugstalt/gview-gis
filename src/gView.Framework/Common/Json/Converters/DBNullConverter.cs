using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Common.Json.Converters;

public class DBNullConverter : JsonConverter<DBNull>
{
    public override DBNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Should never be called!!
        if (reader.TokenType != JsonTokenType.Null)
        {
            throw new JsonException();
        }

        return DBNull.Value;
    }

    public override void Write(Utf8JsonWriter writer, DBNull value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}

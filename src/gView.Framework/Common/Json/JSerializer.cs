using gView.Framework.Common.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace gView.Framework.Common.Json;

public class JSerializer
{
    private static JsonSerializerOptions DefaultOptions =
        new JsonSerializerOptions().AddServerDefaults();

    public static T Deserialize<T>([StringSyntax("Json")] string json)
        => JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static object Deserialize([StringSyntax("Json")] string json, Type returnType)
        => JsonSerializer.Deserialize(json, returnType, DefaultOptions);

    public static string Serialize<TValue>(TValue value)
        => JsonSerializer.Serialize(value, DefaultOptions);
}

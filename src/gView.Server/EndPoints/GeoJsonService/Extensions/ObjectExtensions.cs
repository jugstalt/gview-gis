#nullable enable

using gView.Framework.Core.Exceptions;
using System.Globalization;
using System.Text.Json;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// An object id from a request: a number (also as <see cref="JsonElement"/>) or a text of one.
    /// Anything else is a client error (400).
    /// </summary>
    static public int ToObjectId(this object? value)
        => value switch
        {
            int i => i,
            long l when l is >= int.MinValue and <= int.MaxValue => (int)l,
            JsonElement { ValueKind: JsonValueKind.Number } json when json.TryGetInt32(out var i) => i,
            JsonElement { ValueKind: JsonValueKind.String } json when int.TryParse(json.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
            string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
            _ => throw new MapServerException($"Invalid object id '{value}': object ids are integers")
        };
}

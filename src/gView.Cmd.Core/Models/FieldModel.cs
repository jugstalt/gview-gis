using gView.Framework.Core.Data;
using System.Text.Json.Serialization;

namespace gView.Cmd.Core.Models;

public class FieldModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = FieldType.String.ToString();

    [JsonPropertyName("size")]
    public int Size { get; set; }

}

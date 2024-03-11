using System;
using System.Text.Json.Serialization;

namespace gView.Cmd.Core.Models;
public class AutoFieldModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pluginGuid")]
    public Guid PluginGuid { get; set; }
}

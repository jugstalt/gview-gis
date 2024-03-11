using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace gView.Cmd.Fdb.Lib.Model;
public class RasterProviderModel
{
    [JsonPropertyName("pluginGuid")]
    public Guid PluginGuid { get; set; } = Guid.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;
}

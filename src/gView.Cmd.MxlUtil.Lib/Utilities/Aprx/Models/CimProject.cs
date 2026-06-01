using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// Root object of GISProject.json inside an APRX file.
/// </summary>
internal class CimProject
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("build")]
    public string? Build { get; set; }

    [JsonPropertyName("projectItems")]
    public List<CimProjectItem>? ProjectItems { get; set; }
}

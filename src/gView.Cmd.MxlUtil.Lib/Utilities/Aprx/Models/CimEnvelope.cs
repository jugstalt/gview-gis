using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

internal class CimEnvelope
{
    [JsonPropertyName("xmin")]
    public double XMin { get; set; }

    [JsonPropertyName("ymin")]
    public double YMin { get; set; }

    [JsonPropertyName("xmax")]
    public double XMax { get; set; }

    [JsonPropertyName("ymax")]
    public double YMax { get; set; }

    [JsonPropertyName("spatialReference")]
    public CimSpatialReference? SpatialReference { get; set; }
}

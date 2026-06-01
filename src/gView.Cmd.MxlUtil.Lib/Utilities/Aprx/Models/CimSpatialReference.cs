using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

internal class CimSpatialReference
{
    /// <summary>Well-known ID, e.g. 4326 or 102100.</summary>
    [JsonPropertyName("wkid")]
    public int Wkid { get; set; }

    /// <summary>Latest WKID (may differ from wkid for newer EPSG codes).</summary>
    [JsonPropertyName("latestWkid")]
    public int LatestWkid { get; set; }

    [JsonPropertyName("vcsWkid")]
    public int VcsWkid { get; set; }

    [JsonPropertyName("latestVcsWkid")]
    public int LatestVcsWkid { get; set; }

    [JsonPropertyName("wkt")]
    public string? Wkt { get; set; }

    /// <summary>
    /// Returns the most specific WKID: prefers LatestWkid when set, otherwise Wkid.
    /// </summary>
    public int EffectiveWkid => LatestWkid > 0 ? LatestWkid : Wkid;
}

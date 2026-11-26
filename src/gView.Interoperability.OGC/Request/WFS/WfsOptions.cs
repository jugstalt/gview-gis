#nullable enable

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// Server wide WFS settings (mapserver.json, set at startup)
/// </summary>
public static class WfsOptions
{
    /// <summary>
    /// Count the matching features of GetFeature (numberMatched) also for resultType=results.
    /// mapserver.json: mapserver-defaults:wfsNumberMatched (default true), per service: NumberMatched of the WFS export metadata.
    /// Counting costs a second query, so it can be switched off for large databases.
    /// </summary>
    public static bool NumberMatchedDefault { get; set; } = true;
}

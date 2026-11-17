namespace gView.Blazor.Core.Services;

public class MapControlCrsServiceOptions
{
    public string Default { get; set; } = "webmercator";

    /// <summary>
    /// Largest map scale (1:MaxScale) the map control can zoom to.
    /// Resolutions beyond the crs definition are extrapolated (halved per level).
    /// </summary>
    public double MaxScale { get; set; } = 10;
}

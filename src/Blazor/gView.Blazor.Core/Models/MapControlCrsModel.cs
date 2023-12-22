namespace gView.Blazor.Core.Models;

public class MapControlCrsModel
{
    public int Epsg { get; set; }
    public double[]? Bounds { get; set; }
    public double[]? Origin { get; set; }
    public double[]? Resolutions { get; set; }
}

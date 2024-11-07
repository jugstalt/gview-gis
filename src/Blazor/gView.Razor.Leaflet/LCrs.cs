namespace gView.Razor.Leaflet;

public class LCrs 
{ 
    public int Id { get; set; }
    public string Proj4Parameters { get; set; } = "";

    public double[] Resolutions { get; set; } = [];

    public double[] Origin { get; set; } = [];

    public double[]? Bounds { get; set; }
}


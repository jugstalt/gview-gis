namespace gView.GeoJsonService.DTOs;
public class BBox
{
    public double MinX { get; set; }
    public double MinY { get; set; }
    public double MaxX { get; set; }
    public double MaxY { get; set; }

    static public BBox FromArray(double[] data)
    {
        if (data == null || data.Length != 4)
            throw new Exception("Invalid bbox");

        return new BBox() { MinX = data[0], MinY = data[1], MaxX = data[2], MaxY = data[3] };
    }
}

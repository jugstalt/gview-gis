namespace gView.GeoJsonService.DTOs;

public class FeatureCollection
{
    public string Type => "FeatureCollection";

    public CoordinateReferenceSystem? CRS { get; set; }

    public IEnumerable<Feature>? Features { get; set; }
}

namespace gView.Framework.GeoJsonService.DTOs;

public class GetFeaturesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetFeatures;
    public string Layer { get; set; } = string.Empty;
    public string CRS { get; set; } = string.Empty;
    public float[]? BBox { get; set; } // Optional Bounding Box: [xmin, ymin, xmax, ymax]
    public SpatialFilter? SpatialFilter { get; set; } // Optional spatial filter object
    public AttributeFilter? Filter { get; set; } // Optional attribute filter
    public int? Limit { get; set; } // Optional limit for the number of features to return
    public int? Offset { get; set; } // Optional offset for pagination
    public bool Distinct { get; set; } = false; // Optional flag to indicate if distinct values should be returned
}




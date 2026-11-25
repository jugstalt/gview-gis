namespace gView.GeoJsonService.DTOs;

public class GetLayersRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetLayers;

    /// <summary>The CRS of the layer extents; default as in the capabilities</summary>
    public CoordinateReferenceSystem? CRS { get; set; }
}

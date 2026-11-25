namespace gView.GeoJsonService.DTOs;
public class EditFeaturesResponse
{
    public bool Succeeded { get; set; } = true;
    public string Statement { get; set; } = "";
    public int Count { get; set; }

    /// <summary>
    /// The object ids of the features, in the order of the request: for INSERT the new ids
    /// (0, if the database can't report them)
    /// </summary>
    public int[]? ObjectIds { get; set; }
}

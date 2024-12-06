namespace gView.GeoJsonService.DTOs;
public class EditFeaturesResponse
{
    public bool Succeeded { get; set; } = true;
    public string Statement { get; set; } = "";
    public int Count { get; set; }
}

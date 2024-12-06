namespace gView.GeoJsonService.DTOs;

public class ErrorResponse
{
    public string Type => "ErrorResponse";

    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
}

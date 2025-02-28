namespace gView.GeoJsonService.DTOs;

public class ErrorResponse : BaseResponse
{
    override public string Type { get; set; } = "ErrorResponse";

    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
}

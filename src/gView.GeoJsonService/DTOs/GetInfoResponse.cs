namespace gView.GeoJsonService.DTOs;

public class GetInfoResponse
{
    public string Type { get; set; } = "GetInfoResponse";

    public Version Version { get; set; } = default!;

    public int TokenMaxExpireMinutes { get; set; } = 60;

    public EndPointsClass? EndPoints { get; set; }

    public class EndPointsClass
    {
        public UrlClass[]? Token { get; set; }
        public UrlClass[]? Services { get; set; }
    }

    public class UrlClass
    {
        public string Method { get; set; } = "";
        public string Url { get; set; } = "";
        public string? ContentType { get; set; }
        public string? Body { get; set; }
    }
}
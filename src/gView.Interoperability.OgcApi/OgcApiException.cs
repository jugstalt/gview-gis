namespace gView.Interoperability.OgcApi;

/// <summary>
/// An error that maps directly to an HTTP status code and an OGC API exception body
/// ({ "code": ..., "description": ... }).
/// </summary>
public class OgcApiException : Exception
{
    public OgcApiException(int statusCode, string code, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public int StatusCode { get; }
    public string Code { get; }

    public static OgcApiException BadRequest(string message)
        => new OgcApiException(400, "InvalidParameterValue", message);

    public static OgcApiException NotFound(string message)
        => new OgcApiException(404, "NotFound", message);
}

namespace Aspire.Hosting.ApplicationModel;

public class gViewWebAppsResource(string name)
    : ContainerResource(name)
{
    internal const string HttpsEndpointName = "https";
    internal const string HttpEndpointName = "http";

    private EndpointReference? _httpsReference;
    private EndpointReference? _httpReference;

    public EndpointReference HttpsEndpoint =>
        _httpsReference ??= new(this, HttpsEndpointName);
    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);
}
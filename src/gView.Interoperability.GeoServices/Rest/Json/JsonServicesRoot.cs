using gView.Interoperability.GeoServices.Rest.Reflection;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("Geoservices Explorer", "https://geoexplorer.gviewonline.com?server={onlineresource-url}/geoservices/rest/services&servername=gview")]
    public class JsonServicesRoot : JsonServices
    {
    }
}

using gView.Framework.system;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("Geoservices Explorer", "https://geoexplorer.gviewonline.com?server={onlineresource-url}/geoservices/rest/services&servername=gview")]
    public class JsonServicesRoot : JsonServices
    {
    }
}

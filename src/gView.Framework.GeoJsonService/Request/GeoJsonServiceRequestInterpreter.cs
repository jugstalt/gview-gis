using gView.Framework.Core.Common;
using gView.Framework.Core.MapServer;

namespace gView.Framework.GeoJsonService.Request;

[RegisterPlugIn("308CEA52-F68D-4C48-A7FC-ADC7E1C46CBC")]
public class GeoJsonServiceRequestInterpreter : IServiceRequestInterpreter
{
    public string IdentityName => "geojsonservice";

    public string IdentityLongName => "GeoJson Service";

    public InterpreterCapabilities Capabilities =>
        new InterpreterCapabilities(new InterpreterCapabilities.Capability[]
        {
            new InterpreterCapabilities.SimpleCapability("Specification",InterpreterCapabilities.Method.Get,"https://docs.gviewonline.com/en/spec/geojson_service/index.html","1.0"),
            new InterpreterCapabilities.SimpleCapability("Info (REST)",InterpreterCapabilities.Method.Get,"{server}/geojsonservice/v1","1.0"),
            new InterpreterCapabilities.SimpleCapability("Services (REST)",InterpreterCapabilities.Method.Get,"{server}/geojsonservice/v1/services","1.0"),
            new InterpreterCapabilities.SimpleCapability("Service Capabililites (REST)",InterpreterCapabilities.Method.Post,"{server}/geojsonservice/v1/services/{folder/service}/capabilities","1.0")
        });

    public int Priority => 110;

    public void OnCreate(IMapServer mapServer)
    {

    }

    public Task Request(IServiceRequestContext context)
    {
        return Task.CompletedTask;
    }

    public AccessTypes RequiredAccessTypes(IServiceRequestContext context)
    {
        var accessTypes = AccessTypes.None;

        switch (context.ServiceRequest.Method.ToLower())
        {
            case "map":
                accessTypes |= AccessTypes.Map;
                break;
            case "query":
            case "identify":
                accessTypes |= AccessTypes.Query;
                break;
            case "legend":
                accessTypes |= AccessTypes.Map;
                break;
            case "features":
                accessTypes |= AccessTypes.Edit;
                break;
        }

        return accessTypes;
    }
}

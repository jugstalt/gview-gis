using gView.Framework.Core.Common;
using gView.Framework.Core.MapServer;

namespace gView.Interoperability.OgcApi.Request;

/// <summary>
/// The OGC API endpoints are minimal API endpoints in gView.Server. This interpreter only
/// represents the "ogcapi" interface for the access rules (_ogcapi + map/query/edit).
/// </summary>
[RegisterPlugIn("709344FB-0AC2-4BD1-8680-BB839D68121F")]
public class OgcApiRequestInterpreter : IServiceRequestInterpreter
{
    public string IdentityName => "ogcapi";

    public string IdentityLongName => "OGC API";

    public InterpreterCapabilities Capabilities =>
        new InterpreterCapabilities(new InterpreterCapabilities.Capability[]
        {
            new InterpreterCapabilities.SimpleCapability("Landing Page",InterpreterCapabilities.Method.Get,"{server}/ogcapi/{folder|_}/{service}","1.0"),
            new InterpreterCapabilities.SimpleCapability("Collections",InterpreterCapabilities.Method.Get,"{server}/ogcapi/{folder|_}/{service}/collections","1.0")
        });

    public int Priority => 120;

    public void OnCreate(IMapServer mapServer)
    {
    }

    public Task Request(IServiceRequestContext context)
    {
        return Task.CompletedTask;
    }

    public AccessTypes RequiredAccessTypes(IServiceRequestContext context)
        => context?.ServiceRequest?.Method?.ToLowerInvariant() switch
        {
            OgcApiOperations.Items => AccessTypes.Query,
            OgcApiOperations.Item => AccessTypes.Query,
            OgcApiOperations.Map => AccessTypes.Map,
            OgcApiOperations.CreateItem => AccessTypes.Edit,
            OgcApiOperations.ReplaceItem => AccessTypes.Edit,
            OgcApiOperations.UpdateItem => AccessTypes.Edit,
            OgcApiOperations.DeleteItem => AccessTypes.Edit,
            _ => AccessTypes.None   // metadata resources only require the interface itself
        };
}

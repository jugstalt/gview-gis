using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace gView.Server.EndPoints.GeoJsonService;

public class Root : BaseApiEndpoint
{
    public Root() : base(Routes.Base, Handler)
    {

    }

    private static Delegate Handler => (HttpContext httpContext) =>
    {
        string url = httpContext.Request.GetDisplayUrl();
        return new
        {
            Version = new System.Version(1, 0, 0),
            GetInfo = $"{url}/{Routes.GetInfo}",
            GetServices = $"{url}/{Routes.GetServices}",
            GetToken = $"{url}/{Routes.GetToken}"
        };
    };
}

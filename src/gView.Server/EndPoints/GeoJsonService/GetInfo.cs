using gView.Framework.GeoJsonService.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetInfo : BaseApiEndpoint
{
    public GetInfo()
        : base($"{Routes.Base}/{Routes.GetInfo}", Handler)
    {
    }

    static private Delegate Handler =>
        () =>
        {
            return new GetInfoResponse()
            {
                Version = new System.Version(1, 0, 0)
            };
        };
}

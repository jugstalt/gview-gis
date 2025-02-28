using gView.GeoJsonService;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetInfo : BaseApiEndpoint
{
    public GetInfo()
        : base($"{Routes.Base}/{Routes.GetInfo}", Handler)
    {
    }

    static private Delegate Handler =>
        (
            [FromServices] IConfiguration config,
            [FromServices] MapServiceManager mapServer,
            [FromServices] ILogger<GetInfo> logger
        ) =>
        {
            try
            {
                var onlineResource = mapServer.Options.OnlineResource;

                var tokenEndpoints = TokenEndpointsFromConfig(config).ToArray();
                if (!tokenEndpoints.Any())
                    tokenEndpoints = DefaultTokenEndpoints(onlineResource);

                var response = new GetInfoResponse()
                {
                    Version = new System.Version(1, 0, 0),
                    TokenMaxExpireMinutes = int.Parse(config["Jwt:MaxExpireMinutes"] ?? "60"),
                    EndPoints = new GetInfoResponse.EndPointsClass()
                    {
                        Token = tokenEndpoints.ToArray(),

                        Services = [
                        new() {
                            Url = $"{onlineResource}/{Routes.Base}/{Routes.GetServices}",
                            Method = "GET",
                        }
                        ]
                    }

                };

                return Results.Json(response, GeoJsonSerializer.JsonSerializerOptions);
            } 
            catch(Exception ex )
            {
                logger.LogError("Handle Get Info {message}", ex.Message);

                return Results.Json( new ErrorResponse()
                {
                    ErrorCode = 500,
                    ErrorMessage = "internal server error"
                }, GeoJsonSerializer.JsonSerializerOptions, statusCode: 500);
            }
        };

    static private IEnumerable<GetInfoResponse.UrlClass> TokenEndpointsFromConfig(IConfiguration config)
    {
        ConfigurationSection endPoints = config.GetSection("Jwt:TokenEndpoints") as ConfigurationSection;

        if (endPoints is not null)
        {
            foreach (var endPoint in endPoints.GetChildren() ?? [])
            {
                yield return new GetInfoResponse.UrlClass()
                {
                    Url = endPoint["Url"],
                    Method = endPoint["Method"],
                    ContentType = endPoint["ContentType}"],
                    Body = endPoint["Body"],
                };
            }
        }
    }

    static private GetInfoResponse.UrlClass[] DefaultTokenEndpoints(string onlineResource) =>
        [
            new() {
                Url = $"{onlineResource}/{Routes.Base}/{Routes.GetToken}?clientId={{client_id}}&clientSecret={{client_secret}}&expireMinutes={{expire_minutes}}",
                Method = "GET"
            },
            new() {
                Url = $"{onlineResource}/{Routes.Base}/{Routes.GetToken}",
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                Body = "clientId={client_id}&clientSecret={client_secret}&expireMinutes={expire_minutes}"
            }
        ];
}

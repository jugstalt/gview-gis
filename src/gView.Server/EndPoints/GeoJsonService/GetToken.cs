using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetToken : BaseApiEndpoint
{
    public GetToken()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetToken}"
                ],
                Handler,
                HttpMethod.Get
            )
    {
    }

    private static Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] JwtAccessTokenService jwtTokenService,
                [FromServices] IConfiguration configuration,
                [FromServices] ILogger<GetToken> logger,
                string clientId,
                string clientSecret,
                int expireMinutes = 0
            ) => GetToken.HandleTokenRequest(
                    loginManagerService, jwtTokenService, configuration, logger, clientId, clientSecret, expireMinutes
                );

    static internal object HandleTokenRequest(
                LoginManager loginManagerService,
                JwtAccessTokenService jwtTokenService,
                IConfiguration configuration,
                ILogger<GetToken> logger,
                string clientId,
                string clientSecret,
                int expireMinutes)
    {
        try
        {
            var maxExpireMinutes = int.Parse(configuration["Jwt:MaxExpireMinutes"] ?? "60");
            expireMinutes = expireMinutes switch
            {
                0 => maxExpireMinutes,
                int i when i > maxExpireMinutes => throw new MapServerException($"expireMinutes > {maxExpireMinutes} is not allowed"),
                _ => expireMinutes
            };

            var authToken = loginManagerService.GetAuthToken(
                            clientId,
                            clientSecret,
                            expireMinutes: expireMinutes);

            if (authToken == null)
            {
                throw new MapServerException("unknown clientId or clientSecret");
            }

            return new GetTokenResponse
            {
                access_token = jwtTokenService.GenerateToken(authToken),
                expires_in = expireMinutes * 60
            };
        }
        catch (MapServerException mse)
        {
            logger.LogWarning("Handle Token Request {clientId}: {message}", clientId, mse.Message);

            return new ErrorResponse()
            {
                ErrorCode = 400,
                ErrorMessage = mse.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Handle Token Request {clientId}: {message}", clientId, ex.Message);

            return new ErrorResponse()
            {
                ErrorCode = 500,
                ErrorMessage = "internal server error"
            };
        }
    }
}

public class GetTokenPost : BaseApiEndpoint
{
    public GetTokenPost()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetToken}"
                ],
                Handler,
                HttpMethod.Post
            )
    {
    }

    protected override RouteHandlerBuilder BuildEndpoint(RouteHandlerBuilder builder)
    {
        return builder.DisableAntiforgery();
    }

    private static Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] JwtAccessTokenService jwtTokenService,
                [FromServices] IConfiguration configuration,
                [FromServices] ILogger<GetToken> logger,
                [FromForm] string clientId,
                [FromForm] string clientSecret,
                [FromForm] int expireMinutes = 0
            ) => GetToken.HandleTokenRequest(
                    loginManagerService, jwtTokenService, configuration, logger, clientId, clientSecret, expireMinutes
                );
}
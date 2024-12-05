using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                HttpMethod.Get | HttpMethod.Post
            )
    {
    }

    private static Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] JwtAccessTokenService jwtTokenService
            ) => HandleSecureAsync<GetTokenRequest>(httpContext, async (tokenRequest) =>
            {
                var authToken = loginManagerService.GetAuthToken(tokenRequest.Username, tokenRequest.Password, expireMinutes: tokenRequest.ExpireMinutes);
                if (authToken == null)
                {
                    throw new MapServerException("unknown username or password");
                }

                return new GetTokenResponse
                {
                    Token = jwtTokenService.GenerateToken(authToken.Username, true, tokenRequest.ExpireMinutes)
                };
            });
}

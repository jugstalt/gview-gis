using gView.Framework.Core.Exceptions;
using gView.GeoJsonService;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Server.AppCode;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace gView.Server.Middleware.Authentication;

public class AuthenticationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationExceptionMiddleware(
                RequestDelegate next
        )
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotAuthorizedException nae) when (IsGeoJsonService(context))
        {
            await WriteGeoJsonError(context, 403, 403, nae.Message);
        }
        catch (TokenRequiredException tre) when (IsGeoJsonService(context))
        {
            await WriteGeoJsonError(context, 401, 499, tre.Message);
        }
        catch (InvalidTokenException ite) when (IsGeoJsonService(context))
        {
            context.Response.Cookies.Delete(Globals.AuthCookieName);
            await WriteGeoJsonError(context, 401, 498, ite.Message);
        }
        catch (NotAuthorizedException nae)
        {
            var error = new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef() { Code = 403, Message = nae.Message }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error);
        }
        catch (TokenRequiredException tre)
        {
            var error = new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef() { Code = 499, Message = tre.Message }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error);
        }
        catch (InvalidTokenException ite)
        {
            var error = new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef() { Code = 498, Message = ite.Message }
            };

            // Remove Cookie
            context.Response.Cookies.Delete(Globals.AuthCookieName);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error);
        }
        // GeoServices style (HTTP 200, { "error": { "code", "message" } }) for all other interfaces
        //catch (Exception ex)
        //{
        //    var error = new JsonError()
        //    {
        //        Error = new JsonError.ErrorDef() { Code = 500, Message = ex.Message }
        //    };

        //    context.Response.ContentType = "application/json";
        //    await context.Response.WriteAsJsonAsync(error);
        //}
    }

    /// <summary>
    /// The GeoJson service answers every error as its ErrorResponse with an HTTP status (specification
    /// "Error Handling"): 403 missing right, 401 with errorCode 499 (token required) or 498 (invalid token)
    /// </summary>
    private static bool IsGeoJsonService(HttpContext context)
        => context.Request.Path.StartsWithSegments("/geojsonservice", StringComparison.OrdinalIgnoreCase);

    private static async Task WriteGeoJsonError(HttpContext context, int httpStatus, int errorCode, string message)
    {
        context.Response.StatusCode = httpStatus;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new gView.GeoJsonService.DTOs.ErrorResponse() { ErrorCode = errorCode, ErrorMessage = message },
            GeoJsonSerializer.JsonSerializerOptions);
    }
}

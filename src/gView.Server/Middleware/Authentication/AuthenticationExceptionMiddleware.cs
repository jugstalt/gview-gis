using gView.Framework.Core.Exceptions;
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
}

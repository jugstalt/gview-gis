using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace gView.Server.AppCode;

/// <summary>
/// Protects state changing requests (POST, PUT, DELETE, ...) against CSRF, if the
/// browser could have sent them with its cookies: then the antiforgery token is
/// required (form field <c>__RequestVerificationToken</c> or header
/// <c>RequestVerificationToken</c>, which site.js adds to every jQuery request).
/// Requests with an Authorization header (Basic, Bearer: clients like Carto or
/// scripts) or without any cookie cannot be forged by another site and are not checked.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class AntiforgeryForCookieAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string HeaderName = "RequestVerificationToken";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        if (!RequiresValidation(request))
        {
            return;
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(new { success = false, error = "invalid or missing antiforgery token (reload the page)" });
        }
    }

    internal static bool RequiresValidation(HttpRequest request)
        => !(HttpMethods.IsGet(request.Method)
             || HttpMethods.IsHead(request.Method)
             || HttpMethods.IsOptions(request.Method)
             || HttpMethods.IsTrace(request.Method))
           && String.IsNullOrEmpty(request.Headers.Authorization)
           && request.Cookies.Count > 0;
}

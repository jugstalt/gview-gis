using gView.Server.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace gView.Server.AppCode;

/// <summary>
/// Admin pages and publishing endpoints only answer to clients in
/// <c>security:manage:allowed-networks</c> (see <see cref="Services.Security.SecurityOptions"/>).
/// Everyone else gets a 404, as if the page did not exist.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class AdminNetworkOnlyAttribute : Attribute, IResourceFilter
{
    public const string LoggerCategory = Services.Security.SecurityLog.Category;

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var request = context.HttpContext.Request;

        if (!request.IsFromAdminNetwork())
        {
            context.HttpContext.RequestServices?
                .GetService<ILoggerFactory>()?
                .CreateLogger(LoggerCategory)
                .LogWarning("Admin request from {ip} outside security:manage:allowed-networks refused: {method} {path}",
                    context.HttpContext.Connection.RemoteIpAddress, request.Method, request.Path.Value);

            context.Result = new NotFoundResult();
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}

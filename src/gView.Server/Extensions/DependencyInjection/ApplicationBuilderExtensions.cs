using Microsoft.AspNetCore.Builder;
using System;

namespace gView.Server.Extensions.DependencyInjection;

static public class ApplicationBuilderExtensions
{
    static public IApplicationBuilder UseGViewServerBasePath(this IApplicationBuilder app)
    {
        var basePath = Environment.GetEnvironmentVariable("GVIEW_SERVER_BASE_PATH");
        if (!String.IsNullOrEmpty(basePath))
        {
            app.UsePathBase(basePath);
            Console.WriteLine($"Info: Set Base Path: {basePath}"); ;
        }

        return app;
    }
}

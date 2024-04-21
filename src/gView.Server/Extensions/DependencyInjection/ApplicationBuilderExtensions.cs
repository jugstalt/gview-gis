using gView.Framework.Common;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
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

    static public TBuilder Setup<TBuilder>(this TBuilder builder, string[] args)
        where TBuilder : IHostApplicationBuilder
    {
        #region Init the global PluginManager

        PlugInManager.Init();

        #endregion

        #region First Start => init configuration

        new Setup().TrySetup(args);

        #endregion

        return builder;
    }
}

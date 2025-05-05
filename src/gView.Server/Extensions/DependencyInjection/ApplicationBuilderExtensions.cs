using gView.Framework.Common;
using gView.Server.AppCode;
using gView.Server.Middleware.Authentication;
using gView.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using System;
using System.Configuration;

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

        return app.UsePathBase("/gview-server");
    }

    static public IApplicationBuilder AddAuth(this IApplicationBuilder app,
                                              IConfiguration configuration)
    {
        var authConfig = new AuthConfigModel();
        configuration.Bind("Authentication", authConfig);

        if ("oidc".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase)
             && authConfig.Oidc is not null)
        {
            app.UseAuthentication();
            app.UseMiddleware<OidcAuthenticationMiddleware>();
            //app.UseAuthorization();
        }

        app.UseMiddleware<AuthenticationExceptionMiddleware>();
        app.UseMiddleware<JwtTokenAuthenticationMiddleware>();
        app.UseMiddleware<TokenAuthenticationMiddleware>();
        app.UseMiddleware<UrlTokenAuthenticationMiddleware>();
        app.UseMiddleware<CookieAuthenticationMiddleware>();
        app.UseMiddleware<BasicAuthenticationMiddleware>();

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

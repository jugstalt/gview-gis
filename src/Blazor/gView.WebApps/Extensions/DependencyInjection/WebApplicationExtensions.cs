using gView.WebApps.Model;

namespace gView.WebApps.Extensions.DependencyInjection;

static internal class WebApplicationExtensions
{
    static public WebApplication AddAuth(this WebApplication app, IConfiguration configuration)
    {
        var authConfig = new AuthConfigModel();
        configuration.Bind("Authentication", authConfig);

        if ("oidc".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase)
            && authConfig.Oidc is not null)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        } 
        else
        {
            //app.UseAuthentication();
            app.UseAuthorization();
        }

        return app;
    }

    static public IApplicationBuilder UseGViewWebBasePath(this WebApplication app)
    {
        var basePath = Environment.GetEnvironmentVariable("GVIEW_WEB_BASE_PATH");
        if (!String.IsNullOrEmpty(basePath))
        {
            app.UsePathBase(basePath);
            Console.WriteLine($"Info: Set Base Path: {basePath}"); ;
        }

        return app;
    }

}

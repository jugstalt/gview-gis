using gView.Web.Model;

namespace gView.Web.Extensions.DependencyInjection;

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

        return app;
    }
}
 
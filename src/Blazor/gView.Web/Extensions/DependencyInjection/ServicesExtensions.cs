using Amazon.Auth.AccessControlPolicy;
using gView.Blazor.Core.Services.Abstraction;
using gView.Web.Model;
using gView.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace gView.Web.Extensions.DependencyInjection;

static public class ServicesExtensions
{
    static public IServiceCollection AddWebScopeContextService(this IServiceCollection services)
        => services.AddScoped<IScopeContextService, WebScopeContextService>();

    static public IServiceCollection AddAppIdentityProvider(this IServiceCollection services, Action<AppIdentityProviderOptions> setupAction)
        => services
                .Configure(setupAction)
                .AddScoped<IAppIdentityProvider, AppIdentityProvider>();

    static public IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfig = new AuthConfigModel();
        configuration.Bind("Authentication", authConfig);

        if ("oidc".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase)
            && authConfig.Oidc is not null)
        {
            services
                .AddAuthentication(options => {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                    options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

                    options.Authority = authConfig.Oidc.Authority;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = authConfig.Oidc.ClientId;
                    options.ClientSecret = authConfig.Oidc.ClientSecret;
                    options.ResponseType = "code";

                    //options.SignedOutCallbackPath = "/signout-callback-oidc";
                    //options.SignedOutRedirectUri = "/signin-oidc";
                    //options.CallbackPath = "/signin-oidc";
                    
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.SaveTokens = true;

                    options.Scope.Clear();
                    foreach (var scope in authConfig.Oidc.Scopes ?? ["openid", "profile", "role"])
                    {
                        options.Scope.Add(scope);
                    }

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };

                    options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");
                });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("gview-admin", policy =>
                    policy.RequireRole(authConfig.RequiredAdminRole));

                config.AddPolicy("gview-user", policy =>
                        policy.RequireAssertion(context =>
                            context.User.IsInRole(authConfig.RequiredUserRole) 
                            || context.User.IsInRole(authConfig.RequiredAdminRole))
                    );
            });

            services.AddAppIdentityProvider(config =>
            {
                config.AdminRoleName = authConfig.RequiredAdminRole;
                config.UserRoleName = authConfig.RequiredUserRole;
            });
        }
        else
        {
            services.AddAuthorization(config =>
            {
                config.AddPolicy("gview-user", policy =>
                    policy.RequireAssertion(contextpolicy => true));

                config.AddPolicy("gview-admin", policy  =>
                    policy.RequireAssertion(contextpolicy => true));
            });

            services.AddScoped<IAppIdentityProvider, AppLocalIdentityProvider>();
        }

        return services.AddHttpContextAccessor();
    }
}

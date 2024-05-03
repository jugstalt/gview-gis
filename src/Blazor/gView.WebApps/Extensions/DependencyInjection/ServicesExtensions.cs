using gView.Blazor.Core.Services;
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
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {

                })
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
                    policy.RequireAssertion(context =>
                            context.User.IsInRoleOrHasRoleClaim(authConfig.Oidc.RequiredAdminRole))
                    );

                config.AddPolicy("gview-user", policy =>
                        policy.RequireAssertion(context =>
                            context.User.IsInRoleOrHasRoleClaim(authConfig.Oidc.RequiredUserRole)
                            || context.User.IsInRoleOrHasRoleClaim(authConfig.Oidc.RequiredAdminRole))
                    );
            });

            services.AddAppIdentityProvider(config =>
            {
                config.AdminRoleName = authConfig.Oidc.RequiredAdminRole;
                config.UserRoleName = authConfig.Oidc.RequiredUserRole;
            });
        }
        else if("forms".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase)
            && authConfig.Forms is not null)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "gViewWebCookie";
                    options.LoginPath = "/login-forms";
                    options.LogoutPath = "/logout-forms";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    // Weitere Optionen
                });

            services.AddAppIdentityProvider(config =>
            {
                config.AdminRoleName = AuthConfigModel.FormsClass.AdminRole;
                config.UserRoleName = AuthConfigModel.FormsClass.UserRole;
            });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("gview-admin", policy =>
                    policy.RequireAssertion(context =>
                            context.User.IsInRoleOrHasRoleClaim(AuthConfigModel.FormsClass.AdminRole))
                    );
                 
                config.AddPolicy("gview-user", policy =>
                        policy.RequireAssertion(context =>
                            context.User.IsInRoleOrHasRoleClaim(AuthConfigModel.FormsClass.UserRole)
                            || context.User.IsInRoleOrHasRoleClaim(AuthConfigModel.FormsClass.AdminRole))
                    );
            });
        }
        else
        {
            services.AddAuthorization(config =>
            {
                config.AddPolicy("gview-user", policy =>
                    policy.RequireAssertion(contextpolicy => true));

                config.AddPolicy("gview-admin", policy =>
                    policy.RequireAssertion(contextpolicy => true));
            });

            services
                .Configure<AppIdentityProviderOptions>((config) =>
                {
                    config.AdminRoleName = "";
                    config.UserRoleName = "";
                })
                .AddScoped<IAppIdentityProvider, AppLocalIdentityProvider>();
        }

        return services.AddHttpContextAccessor();
    }

    static public IServiceCollection AddDrives(this IServiceCollection services, IConfiguration configuration)
    {
        var drivesModel = new DrivesModel();
        configuration.Bind(drivesModel);

        #region SetEnvironment Variables

        if (drivesModel.Drives is not null)
        {
            foreach (var key in drivesModel.Drives.Keys)
            {
                Environment.SetEnvironmentVariable(key, drivesModel.Drives[key]);
            }
        }

        #endregion

        return services
            .Configure<DrivesServiceOptions>(config =>
            {
                config.Drives = drivesModel.Drives;
            })
            .AddScoped<DrivesService>();
    }
}

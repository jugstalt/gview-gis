using gView.Server.Models;
using gView.Server.Services.Hosting;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace gView.Server.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddMapServerService(this IServiceCollection services,
                                                         Action<MapServerManagerOptions> configAction)
    {
        services.Configure(configAction);
        services.AddSingleton<MapServerConfiguration>();
        services.AddSingleton<MapServiceManager>();
        services.AddSingleton<MapServiceAccessService>();
        services.AddSingleton<EncryptionCertificateService>();
        services.AddSingleton<JwtAccessTokenService>();
        services.AddSingleton<MapServiceDeploymentManager>();
        services.AddSingleton<gView.Server.Services.Network.NetworkServerService>();
        
        services.AddHttpContextAccessor();
        services.AddSingleton<LoginThrottleService>();
        services.AddSingleton<AdminSetupService>();
        services.AddSingleton<gView.Server.Services.Security.TwoFactor.TwoFactorService>();

        services.AddTransient<UrlHelperService>();
        services.AddTransient<LoginManager>();
        services.AddTransient<AccessControlService>();

        services.AddTransient<MapServicesEventLogger>();

        return services;
    }

    static public IServiceCollection AddAccessTokenAuthService(this IServiceCollection services,
                                                               Action<AccessTokenAuthServiceOptions> configAction)
    {
        services.Configure(configAction);
        return services.AddTransient<AccessTokenAuthService>();
    }

    static public IServiceCollection AddPerformanceLoggerService(this IServiceCollection services,
                                                                 Action<PerformanceLoggerServiceOptions> configAction)
    {
        services.Configure(configAction);
        return services.AddSingleton<PerformanceLoggerService>();
    }

    static public IServiceCollection AddAuth(this IServiceCollection services,
                                             IConfiguration configuration)
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
        }
        else
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        return services;
    }

    /// <summary>
    /// Request body size and the optional rate limit of the services (see <see cref="SecurityOptions"/>).
    /// </summary>
    static public IServiceCollection AddSecurityLimits(this IServiceCollection services, SecurityOptions options)
    {
        services.Configure<KestrelServerOptions>(o => o.Limits.MaxRequestBodySize = options.MaxRequestBodyBytes);
        services.Configure<IISServerOptions>(o => o.MaxRequestBodySize = options.MaxRequestBodyBytes);
        services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = options.MaxRequestBodyBytes ?? long.MaxValue);

        if (options.RateLimitEnabled)
        {
            services.AddRateLimiter(limiter =>
            {
                limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                limiter.GlobalLimiter = ServiceRateLimiter.Create(options.RateLimitRequestsPerMinute);
            });
        }

        return services;
    }
}

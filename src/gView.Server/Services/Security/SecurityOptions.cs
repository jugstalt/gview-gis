using gView.Framework.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace gView.Server.Services.Security;

/// <summary>
/// The <c>security</c> section of mapserver.json:
/// <code>
/// "security": {
///   "force-https": false,          // also read from the top level "force-https"
///   "https-redirect": false,       // redirect http to https (only if the proxy sends X-Forwarded-Proto)
///   "hsts-max-age-days": 365,
///   "known-proxies": [ "10.0.0.5" ],
///   "known-networks": [ "10.0.0.0/8" ],
///   "manage": {
///     "allowed-networks": [ "10.1.5.0/24" ],  // "*" = everywhere
///     "session-hours": 8                      // admin login, renewed while active
///   },
///   "cors": {
///     "allowed-origins": [ "*" ]              // or [ "https://maps.example.com" ]
///   },
///   "max-request-body-mb": 50,                // 0 = no limit
///   "rate-limit": {
///     "enabled": false,                       // usually done by the reverse proxy
///     "requests-per-minute": 600              // per client ip, services only
///   }
/// }
/// </code>
/// X-Forwarded-For/-Proto are only accepted from known proxies or networks.
/// Without any entry, loopback and the private networks are trusted.
/// The admin pages (manage, publishing) answer only to clients in
/// <c>manage:allowed-networks</c>, by default loopback and the private networks.
/// With an OIDC admin login the restriction only applies, if it is configured.
/// </summary>
public class SecurityOptions
{
    public const string SectionName = "security";

    /// <summary>
    /// Trusted when neither known-proxies nor known-networks are configured.
    /// </summary>
    public static readonly string[] DefaultTrustedNetworks =
    [
        "127.0.0.0/8", "::1/128",
        "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16",
        "fc00::/7"
    ];

    /// <summary>
    /// The server generates https urls (capabilities, links, output urls) and sends HSTS.
    /// </summary>
    public bool ForceHttps { get; init; }

    /// <summary>
    /// Requests that arrive as http are redirected to https.
    /// Only safe if the proxy terminating TLS sends X-Forwarded-Proto, otherwise this loops.
    /// </summary>
    public bool HttpsRedirect { get; init; }

    public int HstsMaxAgeDays { get; init; } = 365;

    public IReadOnlyList<IPAddress> KnownProxies { get; init; } = [];

    public IReadOnlyList<IPNetwork> KnownNetworks { get; init; } = [];

    /// <summary>
    /// True, if neither known-proxies nor known-networks are configured
    /// (<see cref="DefaultTrustedNetworks"/> are trusted).
    /// </summary>
    public bool UsesDefaultTrustedProxies { get; init; }

    /// <summary>
    /// Networks the admin pages can be used from; null = everywhere.
    /// </summary>
    public IReadOnlyList<IPNetwork> ManageAllowedNetworks { get; init; } = DefaultTrustedNetworks.Select(ParseNetwork).ToArray();

    /// <summary>
    /// True, if manage:allowed-networks is set in the configuration.
    /// </summary>
    public bool ManageAllowedNetworksConfigured { get; init; }

    /// <summary>
    /// Lifetime of an admin (form) login; renewed while the admin is active.
    /// </summary>
    public int ManageSessionHours { get; init; } = 8;

    /// <summary>
    /// Origins allowed to call the services from a browser (CORS); null = any origin.
    /// The admin and login pages never send CORS headers.
    /// </summary>
    public IReadOnlyList<string> CorsAllowedOrigins { get; init; }

    /// <summary>
    /// Maximum size of a request body in bytes (Kestrel, IIS, forms); null = no limit.
    /// </summary>
    public long? MaxRequestBodyBytes { get; init; } = 50L * 1024 * 1024;

    /// <summary>
    /// Rate limit for the services per client ip (admin pages are not limited).
    /// </summary>
    public bool RateLimitEnabled { get; init; }

    public int RateLimitRequestsPerMinute { get; init; } = 600;

    /// <summary>
    /// The admin login is OIDC (Authentication:Type = oidc).
    /// </summary>
    public bool OidcAdminLogin { get; init; }

    /// <summary>
    /// May a client with this (already forwarded) address use the admin pages?
    /// Requests without an address (e.g. unix sockets) are not allowed.
    /// </summary>
    public bool IsManageAllowed(IPAddress clientIp)
    {
        if (OidcAdminLogin && !ManageAllowedNetworksConfigured)
        {
            return true;
        }
        if (ManageAllowedNetworks is null)
        {
            return true;
        }
        if (clientIp is null)
        {
            return false;
        }
        if (clientIp.IsIPv4MappedToIPv6)
        {
            clientIp = clientIp.MapToIPv4();
        }

        return ManageAllowedNetworks.Any(n => n.Contains(clientIp));
    }

    public static SecurityOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        var proxies = Strings(section, "known-proxies")
            .Select(p => IPAddress.TryParse(p, out var address)
                ? address
                : throw new MapServerException($"security:known-proxies: invalid ip address '{p}'"))
            .ToArray();

        var networks = Strings(section, "known-networks")
            .Select(ParseNetwork)
            .ToArray();

        bool defaultProxies = proxies.Length == 0 && networks.Length == 0;
        if (defaultProxies)
        {
            networks = DefaultTrustedNetworks.Select(ParseNetwork).ToArray();
        }

        var manageNetworks = Strings(section, "manage:allowed-networks");
        var corsOrigins = Strings(section, "cors:allowed-origins");
        bool manageEverywhere = manageNetworks.Any(n => n == "*");

        return new SecurityOptions()
        {
            ForceHttps = Bool(section["force-https"]) ?? Bool(configuration["force-https"]) ?? false,
            HttpsRedirect = Bool(section["https-redirect"]) ?? false,
            HstsMaxAgeDays = int.TryParse(section["hsts-max-age-days"], out int days) && days >= 0 ? days : 365,
            KnownProxies = proxies.SelectMany(WithIPv4Mapped).Distinct().ToArray(),
            KnownNetworks = networks.SelectMany(WithIPv4Mapped).Distinct().ToArray(),
            UsesDefaultTrustedProxies = defaultProxies,
            ManageAllowedNetworksConfigured = manageNetworks.Length > 0,
            ManageAllowedNetworks = manageEverywhere
                ? null
                : (manageNetworks.Length > 0 ? manageNetworks : DefaultTrustedNetworks).Select(ParseNetwork).ToArray(),
            MaxRequestBodyBytes = int.TryParse(section["max-request-body-mb"], out int bodyMb) && bodyMb >= 0
                ? (bodyMb == 0 ? null : bodyMb * 1024L * 1024L)
                : 50L * 1024 * 1024,
            RateLimitEnabled = Bool(section["rate-limit:enabled"]) ?? false,
            RateLimitRequestsPerMinute = int.TryParse(section["rate-limit:requests-per-minute"], out int perMinute) && perMinute > 0 ? perMinute : 600,
            ManageSessionHours = int.TryParse(section["manage:session-hours"], out int hours) && hours > 0 ? hours : 8,
            CorsAllowedOrigins = corsOrigins.Length == 0 || corsOrigins.Contains("*")
                ? null
                : corsOrigins.Select(o => o.TrimEnd('/')).ToArray(),
            OidcAdminLogin = "oidc".Equals(configuration["Authentication:Type"]?.Trim(), StringComparison.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Parses "10.0.0.0/8", "fc00::/7" or a single address ("10.0.0.5" = /32).
    /// </summary>
    public static IPNetwork ParseNetwork(string network)
    {
        network = network?.Trim() ?? String.Empty;

        if (!network.Contains('/') && IPAddress.TryParse(network, out var address))
        {
            return new IPNetwork(address, address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128);
        }

        if (IPNetwork.TryParse(network, out var result))
        {
            return result;
        }

        throw new MapServerException($"security: invalid network '{network}' (expected e.g. 10.0.0.0/8)");
    }

    /// <summary>
    /// Warnings about the configuration, written to the log at startup.
    /// </summary>
    public IEnumerable<string> ConfigurationWarnings()
    {
        // with the default trusted proxies every host in the private networks can send
        // X-Forwarded-For and pretend to come from an allowed admin network
        if (UsesDefaultTrustedProxies && ManageAllowedNetworksConfigured && ManageAllowedNetworks is not null)
        {
            yield return "security:manage:allowed-networks is set, but security:known-proxies/known-networks are not: "
                       + "every host in the private networks is trusted to send X-Forwarded-For and can pretend "
                       + "to come from an allowed admin network. Set security:known-proxies to your reverse proxies.";
        }
    }

    #region Helper

    // Kestrel may report IPv4 clients as IPv4-mapped IPv6 addresses (::ffff:10.0.0.1)
    // on dual-stack sockets: trust both forms.
    private static IEnumerable<IPAddress> WithIPv4Mapped(IPAddress address)
    {
        yield return address;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            yield return address.MapToIPv6();
        }
    }

    private static IEnumerable<IPNetwork> WithIPv4Mapped(IPNetwork network)
    {
        yield return network;

        if (network.BaseAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            yield return new IPNetwork(network.BaseAddress.MapToIPv6(), network.PrefixLength + 96);
        }
    }

    private static string[] Strings(IConfigurationSection section, string key)
        => section.GetSection(key).Get<string[]>()?
                  .Where(s => !String.IsNullOrWhiteSpace(s))
                  .Select(s => s.Trim())
                  .ToArray() ?? [];

    private static bool? Bool(string value)
        => bool.TryParse(value?.Trim(), out bool result) ? result : null;

    #endregion
}

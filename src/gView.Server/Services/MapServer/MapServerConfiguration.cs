using gView.Server.Models;
using Microsoft.Extensions.Configuration;

namespace gView.Server.Services.MapServer;

public class MapServerConfiguration
{
    private readonly AuthConfigModel _authConfig;

    public MapServerConfiguration(IConfiguration configuration)
    {
        _authConfig = new AuthConfigModel();
        configuration.Bind("Authentication", _authConfig);

        HasExternalLoginAuthority = _authConfig?.Type?.ToLowerInvariant() switch
        {
            "oidc" when !string.IsNullOrEmpty(_authConfig.Oidc?.Authority) => true,
            _ => false
        };
    }

    public bool HasExternalLoginAuthority { get; }
}

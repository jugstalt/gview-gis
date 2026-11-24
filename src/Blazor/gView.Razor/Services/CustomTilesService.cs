using Microsoft.Extensions.Options;

namespace gView.Razor.Services;

public class CustomTilesService
{
    private readonly CustomTilesServiceOptions _options;

    public CustomTilesService(IOptions<CustomTilesServiceOptions> options)
    {
        _options = options.Value;
    }

    public IEnumerable<CustomTilesServiceOptions.CustomTileModel> Tiles
        => _options.CustomTiles ?? [];

    public CustomTilesServiceOptions.HomeModel Home
        => _options.Home ?? new();

    public bool AuthenticationConfigured
        => _options.AuthenticationConfigured;
}

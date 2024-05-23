using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

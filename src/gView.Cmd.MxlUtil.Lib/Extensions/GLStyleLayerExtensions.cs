using gView.DataSources.VectorTileCache.Json.GLStyles;

namespace gView.Cmd.MxlUtil.Lib.Extensions;

static internal class GLStyleLayerExtensions
{
    static public bool RequireSpriteIcon(this IEnumerable<GLStyleLayer> layers, string iconImage)
        => layers.Any(l => l?.Layout?
                             .IconImage?
                             .ToString()
                             .Contains(iconImage) == true);
}

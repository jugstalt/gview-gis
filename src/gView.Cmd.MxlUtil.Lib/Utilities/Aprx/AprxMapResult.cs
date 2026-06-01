using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

/// <summary>
/// Holds the parsed <see cref="CimMap"/> together with its fully resolved layer definitions.
/// </summary>
internal class AprxMapResult
{
    public AprxMapResult(CimMap map, IReadOnlyList<CimBaseLayer> layers)
    {
        Map = map;
        Layers = layers;
    }

    public CimMap Map { get; }

    /// <summary>
    /// Flattened, resolved layer definitions for this map.
    /// </summary>
    public IReadOnlyList<CimBaseLayer> Layers { get; }
}

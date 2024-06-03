using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Framework.Symbology.Vtc.Extensions;

public static class PaintSymbolExtensions
{
    static public T GetValueOrDeafult<T>(this PaintSymbol paintSymbol, string name, T defaultValue, IDisplay display, IFeature? feature)
        => paintSymbol.ValueFuncs.GetValueOrDeafult(name, defaultValue, display, feature);


}

using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Extensions;

namespace gView.Carto.Razor.Services;

public class SymbolClipboardService : ISymbolClipboard
{
    public ISymbol? Symbol { get; private set; }

    public event Action? Changed;

    public void Copy(ISymbol symbol)
    {
        Symbol = (ISymbol)symbol.Clone();
        Changed?.Invoke();
    }

    public bool CanPaste(GeometryType geometryType)
        => Symbol.FitsGeometryType(geometryType);
}

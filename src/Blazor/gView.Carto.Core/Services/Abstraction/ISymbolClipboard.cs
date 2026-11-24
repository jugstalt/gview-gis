using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using System;

namespace gView.Carto.Core.Services.Abstraction;

/// <summary>
/// In-memory clipboard for one symbol, per Carto session (scoped).
/// </summary>
public interface ISymbolClipboard
{
    /// <summary>Stores a clone of the symbol.</summary>
    void Copy(ISymbol symbol);

    /// <summary>The copied symbol (not a clone; clone it before use).</summary>
    ISymbol? Symbol { get; }

    /// <summary>True, if there is a symbol that fits the (layer) geometry type.</summary>
    bool CanPaste(GeometryType geometryType);

    event Action? Changed;
}

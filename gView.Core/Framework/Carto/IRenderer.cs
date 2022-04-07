using gView.Framework.Symbology;
using System.Collections.Generic;

namespace gView.Framework.Carto
{
    public interface IRenderer
    {
        List<ISymbol> Symbols { get; }
        bool Combine(IRenderer renderer);
    }
}
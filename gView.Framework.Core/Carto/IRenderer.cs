using gView.Framework.Core.Symbology;
using System.Collections.Generic;

namespace gView.Framework.Core.Carto
{
    public interface IRenderer
    {
        List<ISymbol> Symbols { get; }
        bool Combine(IRenderer renderer);
    }
}
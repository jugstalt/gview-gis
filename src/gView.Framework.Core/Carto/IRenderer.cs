using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;
using System.Collections.Generic;

namespace gView.Framework.Core.Carto
{
    public interface IRenderer : IClone, IClone2
    {
        string Name { get; }

        List<ISymbol> Symbols { get; }
        bool Combine(IRenderer renderer);

        bool CanRender(IFeatureLayer layer, IMap map);
    }
}
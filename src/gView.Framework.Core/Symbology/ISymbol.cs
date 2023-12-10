using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;

namespace gView.Framework.Core.Symbology
{
    public interface ISymbol : IPersistable, IClone, IClone2, ILegendItem
    {
        void Draw(IDisplay display, IGeometry geometry);

        string Name { get; }

        SymbolSmoothing SymbolSmoothingMode { get; set; }

        bool SupportsGeometryType(GeometryType type);

        bool RequireClone();
    }
}

using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Symbology
{
    public interface ISymbol : IPersistable, IClone, IClone2, ILegendItem
    {
        void Draw(IDisplay display, IGeometry geometry);

        string Name { get; }

        SymbolSmoothing SymbolSmothingMode { set; }

        bool SupportsGeometryType(GeometryType type);

        bool RequireClone();
    }
}

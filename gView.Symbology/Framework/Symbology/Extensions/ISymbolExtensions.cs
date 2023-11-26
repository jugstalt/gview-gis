using gView.Framework.Geometry;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.Extensions;

public static class ISymbolExtensions
{
    public static GeometryType ToGeometryType(this ISymbol symbol)
        => symbol switch
        {
            IFillSymbol => GeometryType.Polygon,
            ILineSymbol => GeometryType.Polyline,
            IPointSymbol => GeometryType.Point,
            ITextSymbol => GeometryType.Point,
            _ => GeometryType.Unknown
        };
}

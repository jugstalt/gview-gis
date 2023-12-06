using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using System.Reflection.Emit;

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

    public static ISymbol AddLegendLabel(this ISymbol symbol, string legendLabel)
    {
        if(symbol is ILegendItem legendItem)
        {
            symbol.LegendLabel = legendLabel;
        }

        return symbol;
    }
}

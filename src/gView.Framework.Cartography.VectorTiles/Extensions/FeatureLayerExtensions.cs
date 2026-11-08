using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;

namespace gView.Framework.Cartography.VectorTiles.Extensions;

internal static class FeatureLayerExtensions
{
    extension(IFeatureLayer layer)
    {
        /// <summary>
        /// The reference scale factor of the layer's symbols at <paramref name="scale"/>, as gView draws them
        /// (1 without reference scale, limited by the layer's <c>MaxRefScaleFactor</c>).
        /// </summary>
        public double RefScaleFactor(double referenceScale, double scale)
        {
            if (referenceScale <= 1 || layer.FeatureRenderer?.UseReferenceScale != true || !layer.ApplyRefScale)
            {
                return 1.0;
            }

            double factor = referenceScale / scale;
            return layer.MaxRefScaleFactor > 0 ? Math.Min(factor, layer.MaxRefScaleFactor) : factor;
        }

        /// <summary>
        /// The widest line or largest symbol of the layer's renderer at <paramref name="scale"/>, in pixels.
        /// </summary>
        public double MaxSymbolPixels(double scale, double referenceScale, double dpi)
        {
            if (layer.FeatureRenderer?.Symbols is not { } symbols)
            {
                return 0;
            }

            double factor = layer.RefScaleFactor(referenceScale, scale);
            double metersPerPixel = scale / (dpi / 0.0254);
            double max = 0;

            foreach (var symbol in Expand(symbols))
            {
                if (symbol is IPenWidth pen)
                {
                    max = Math.Max(max, pen.PenWidthUnit == DrawingUnit.Meters
                        ? pen.PenWidth / metersPerPixel
                        : Clamp(pen.PenWidth * factor, pen.MinPenWidth, pen.MaxPenWidth));
                }

                if (symbol is ISymbolSize size)
                {
                    max = Math.Max(max, Clamp(size.SymbolSize * factor, size.MinSymbolSize, size.MaxSymbolSize));
                }
            }

            return max;
        }
    }

    private static double Clamp(double value, double min, double max)
    {
        if (min > 0 && value < min) value = min;
        if (max > 0 && value > max) value = max;
        return value;
    }

    private static IEnumerable<ISymbol> Expand(IEnumerable<ISymbol?> symbols)
    {
        foreach (var symbol in symbols)
        {
            if (symbol == null)
            {
                continue;
            }

            if (symbol is ISymbolCollection collection)
            {
                foreach (var inner in Expand(collection.Symbols.Where(i => i.Visible).Select(i => i.Symbol)))
                {
                    yield return inner;
                }

                continue;
            }

            yield return symbol;

            if (symbol is IOutlineSymbol outlined && outlined.OutlineSymbol != null)
            {
                foreach (var inner in Expand([outlined.OutlineSymbol]))
                {
                    yield return inner;
                }
            }
        }
    }
}

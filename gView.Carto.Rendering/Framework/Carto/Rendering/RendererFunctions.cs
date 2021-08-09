using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;
using System;

namespace gView.Framework.Carto.Rendering
{
    public class RendererFunctions
    {
        static internal Random r = new Random(DateTime.Now.Millisecond);
        static internal ArgbColor RandomColor
        {
            get
            {
                return ArgbColor.FromArgb(r.Next(255), r.Next(255), r.Next(255));
            }
        }
        static public ISymbol CreateStandardSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleFillSymbol)symbol).Color = RandomColor;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = RandomColor;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = RandomColor;
                    break;
                case geometryType.Multipoint:
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = RandomColor;
                    break;
            }
            return symbol;
        }

        static public ISymbol CreateStandardSelectionSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).Color = ArgbColor.Transparent;
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = ArgbColor.Cyan;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Width = 3;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = ArgbColor.Cyan;
                    ((SimpleLineSymbol)symbol).Width = 3;
                    break;
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = ArgbColor.Cyan;
                    ((SimplePointSymbol)symbol).Size = 5;
                    break;
            }
            return symbol;
        }

        static public ISymbol CreateStandardHighlightSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).Color = ArgbColor.FromArgb(100, 255, 255, 0);
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = ArgbColor.Yellow;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Width = 5;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = ArgbColor.Yellow;
                    ((SimpleLineSymbol)symbol).Width = 5;
                    break;
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = ArgbColor.Yellow;
                    ((SimplePointSymbol)symbol).Size = 10;
                    break;
            }
            return symbol;
        }
    }
}

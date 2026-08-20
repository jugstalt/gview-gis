using gView.GraphicsEngine.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsEngine
    {
        string EngineName { get; }
        string EngineDisplayName { get; }

        IBitmap CreateBitmap(int width, int height);
        IBitmap CreateBitmap(int width, int height, PixelFormat format);
        IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0);
        IBitmap CreateBitmap(Stream stream);
        IBitmap CreateBitmap(string filename);

        /// <summary>
        /// Rasterizes SVG markup into a bitmap of the given pixel size.
        /// Not every engine can render real SVG (e.g. GDI+, which has no SVG
        /// renderer available) - such implementations should return a generic
        /// placeholder bitmap instead of throwing, so a map/symbol using this
        /// engine still renders (just without the actual SVG artwork) rather than
        /// failing the whole draw.
        /// </summary>
        IBitmap RasterizeSvg(byte[] svgBytes, int pixelWidth, int pixelHeight);

        IPen CreatePen(ArgbColor color, float width);
        IBrush CreateSolidBrush(ArgbColor color);
        IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle);
        IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor);

        IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point);
        IEnumerable<string> GetInstalledFontNames();
        string GetDefaultFontName();

        IDrawTextFormat CreateDrawTextFormat();

        IGraphicsPath CreateGraphicsPath();

        float ScreenDpi { get; }

        bool MeasuresTextWithPadding { get; }

        IThreadLocker CloneObjectsLocker { get; }

        //void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset);
    }
}

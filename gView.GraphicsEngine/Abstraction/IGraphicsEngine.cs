using gView.GraphicsEngine.Threading;
using System;
using System.IO;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsEngine
    {
        string EngineName { get; }

        IBitmap CreateBitmap(int width, int height);
        IBitmap CreateBitmap(int width, int height, PixelFormat format);
        IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0);
        IBitmap CreateBitmap(Stream stream);
        IBitmap CreateBitmap(string filename);

        IPen CreatePen(ArgbColor color, float width);
        IBrush CreateSolidBrush(ArgbColor color);
        IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle);
        IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor);

        IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point);

        IDrawTextFormat CreateDrawTextFormat();

        IGraphicsPath CreateGraphicsPath();

        float ScreenDpi { get; }

        bool MeasuresTextWithPadding { get; }

        IThreadLocker CloneObjectsLocker { get; }

        //void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset);
    }
}

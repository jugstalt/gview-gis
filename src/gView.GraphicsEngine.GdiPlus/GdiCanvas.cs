using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Drawing;
using System.Linq;

namespace gView.GraphicsEngine.GdiPlus;

internal class GdiCanvas : ICanvas
{
    private Graphics _graphics;
    private CanvasRectangle _bounds;

    public GdiCanvas(Bitmap bitmap)
    {
        _graphics = Graphics.FromImage(bitmap);
        _bounds = new CanvasRectangle(0, 0, bitmap.Width, bitmap.Height);
    }

    #region ICanvas

    public float DpiX => _graphics != null ? _graphics.DpiX : 0;

    public float DpiY => _graphics != null ? _graphics.DpiY : 0;

    public CompositingMode CompositingMode
    {
        set
        {
            CheckUsability();

            _graphics.CompositingMode = value.ToGdiCompositionMode();
        }
    }

    public InterpolationMode InterpolationMode
    {
        get
        {
            CheckUsability();

            return _graphics.InterpolationMode.ToInterpolationMode();
        }
        set
        {
            CheckUsability();

            _graphics.InterpolationMode = value.ToGdiInterpolationMode();
        }
    }

    public SmoothingMode SmoothingMode
    {
        get
        {
            CheckUsability();

            return _graphics.SmoothingMode.ToSmoothingMode();
        }
        set
        {
            CheckUsability();

            _graphics.SmoothingMode = value.ToGdiSmoothingMode();
        }
    }

    public TextRenderingHint TextRenderingHint
    {
        get
        {
            CheckUsability();

            return (TextRenderingHint)_graphics.TextRenderingHint;
        }
        set
        {
            CheckUsability();

            _graphics.TextRenderingHint = (System.Drawing.Text.TextRenderingHint)value;
        }
    }

    public void Clear(ArgbColor? argbColor = null)
    {
        using (var brush = new SolidBrush(argbColor.HasValue ? argbColor.Value.ToGdiColor() : Color.White))
        {
            _graphics.FillRectangle(brush, _bounds.ToGdiRectangle());
        }
    }

    public void Flush()
    {
    }

    public void TranslateTransform(CanvasPointF point)
    {
        CheckUsability();

        _graphics.TranslateTransform(point.X, point.Y);
    }

    public void RotateTransform(float angle)
    {
        CheckUsability();

        _graphics.RotateTransform(angle);
    }

    public void ScaleTransform(float sx, float sy)
    {
        _graphics.ScaleTransform(sx, sy);
    }

    public void ResetTransform()
    {
        CheckUsability();

        _graphics.ResetTransform();
    }

    public void SetClip(CanvasRectangle rectangle)
    {
        _graphics?.SetClip(rectangle.ToGdiRectangle());
    }

    public void SetClip(CanvasRectangleF rectangle)
    {
        _graphics?.SetClip(rectangle.ToGdiRectangleF());
    }

    //public void ResetClip() => _graphics?.ResetClip();

    public void FillRectangle(IBrush brush, int left, int right, int width, int height)
    {
        CheckUsability();

        _graphics.FillRectangle((Brush)brush.EngineElement, left, right, width, height);
    }

    public void FillRectangle(IBrushCollection brushCollection, CanvasRectangleF rectangleF)
    {
        CheckUsability();

        foreach (var brush in brushCollection.Brushes)
        {
            _graphics.FillRectangle((Brush)brush.EngineElement, rectangleF.ToGdiRectangleF());
        }
    }

    public void DrawRectangle(IPen pen, CanvasRectangle rectangle)
    {
        CheckUsability();

        _graphics.DrawRectangle((Pen)pen.EngineElement, rectangle.ToGdiRectangle());
    }

    public void DrawRectangle(IPen pen, CanvasRectangleF rectangleF)
    {
        CheckUsability();

        _graphics.DrawRectangle((Pen)pen.EngineElement,
            rectangleF.Left,
            rectangleF.Top,
            rectangleF.Width,
            rectangleF.Height);
    }

    public void FillRectangle(IBrush brush, CanvasRectangle rectangle)
    {
        CheckUsability();

        _graphics.FillRectangle((Brush)brush.EngineElement, rectangle.ToGdiRectangle());
    }

    public void FillRectangle(IBrush brush, CanvasRectangleF rectangleF)
    {
        CheckUsability();

        _graphics.FillRectangle((Brush)brush.EngineElement, rectangleF.ToGdiRectangleF());
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPoint point)
    {
        CheckUsability();

        _graphics.DrawImage((Bitmap)bitmap.EngineElement, point.ToGdiPoint());
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPointF pointF)
    {
        CheckUsability();

        _graphics.DrawImage((Bitmap)bitmap.EngineElement, pointF.ToGdiPointF());
    }

    public void DrawBitmap(IBitmap bitmap, CanvasRectangle dest, CanvasRectangle source, float opacity = 1.0f)
    {
        CheckUsability();

        var imageAttributes = CreateImageAttributes(opacity);

        if (imageAttributes != null)
        {
            _graphics.DrawImage((Bitmap)bitmap.EngineElement,
                dest.ToGdiRectangle(),
                source.Left, source.Top, source.Width, source.Height,
                System.Drawing.GraphicsUnit.Pixel,
                imageAttrs: imageAttributes);
        }
        else
        {
            _graphics.DrawImage((Bitmap)bitmap.EngineElement,
                dest.ToGdiRectangle(),
                source.ToGdiRectangle(),
                System.Drawing.GraphicsUnit.Pixel);
        }
    }

    public void DrawBitmap(IBitmap bitmap, CanvasRectangleF dest, CanvasRectangleF source)
    {
        CheckUsability();

        _graphics.DrawImage((Bitmap)bitmap.EngineElement,
            dest.ToGdiRectangleF(),
            FitDrawBitmapSourceRectangle(source).ToGdiRectangleF(),
            System.Drawing.GraphicsUnit.Pixel);
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPointF[] points, CanvasRectangleF source, float opacity = 1)
    {
        CheckUsability();

        var imageAttributes = CreateImageAttributes(opacity);

        if (imageAttributes != null)
        {
            _graphics.DrawImage((Bitmap)bitmap.EngineElement,
                points.Take(3).ToGdiPointFArray(),
                FitDrawBitmapSourceRectangle(source).ToGdiRectangleF(),
                System.Drawing.GraphicsUnit.Pixel,
                imageAttr: imageAttributes);
        }
        else
        {
            _graphics.DrawImage((Bitmap)bitmap.EngineElement,
                points.Take(3).ToGdiPointFArray(),
                source.ToGdiRectangleF(),
                System.Drawing.GraphicsUnit.Pixel);
        }
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, point.ToGdiPoint());
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, pointF.ToGdiPointF());
    }

    public void DrawText(string text, IFont font, IBrush brush, int x, int y)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, x, y);
    }

    public void DrawText(string text, IFont font, IBrush brush, float x, float y)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, x, y);
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasRectangleF rectangleF)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, rectangleF.ToGdiRectangleF());
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point, IDrawTextFormat format)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, point.ToGdiPoint(), (StringFormat)format?.EngineElement);
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF, IDrawTextFormat format)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, pointF.ToGdiPointF(), (StringFormat)format?.EngineElement);
    }

    public void DrawText(string text, IFont font, IBrush brush, int x, int y, IDrawTextFormat format)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, x, y, (StringFormat)format?.EngineElement);
    }

    public void DrawText(string text, IFont font, IBrush brush, float x, float y, IDrawTextFormat format)
    {
        CheckUsability();

        _graphics.DrawString(text, (Font)font.EngineElement, (Brush)brush.EngineElement, x, y, (StringFormat)format?.EngineElement);
    }

    public CanvasSizeF MeasureText(string text, IFont font)
    {
        CheckUsability();

        var sizeF = _graphics.MeasureString(text, (Font)font.EngineElement);
        return new CanvasSizeF(sizeF.Width, sizeF.Height);
    }

    public IDisplayCharacterRanges DisplayCharacterRanges(IFont font, IDrawTextFormat format, string text)
    {
        CheckUsability();

        return new DisplayCharacterRanges(_graphics, (Font)font.EngineElement, (StringFormat)format.EngineElement, text);
    }

    public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
    {
        CheckUsability();

        _graphics.DrawLine((Pen)pen.EngineElement, p1.ToGdiPoint(), p2.ToGdiPoint());
    }

    public void DrawLine(IPen pen, CanvasPointF p1, CanvasPointF p2)
    {
        CheckUsability();

        _graphics.DrawLine((Pen)pen.EngineElement, p1.ToGdiPointF(), p2.ToGdiPointF());
    }

    public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
    {
        CheckUsability();

        _graphics.DrawLine((Pen)pen.EngineElement, x1, y1, x2, y2);
    }

    public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
    {
        CheckUsability();

        _graphics.DrawLine((Pen)pen.EngineElement, x1, y1, x2, y2);
    }

    public void DrawEllipse(IPen pen, float x1, float y1, float x2, float y2)
    {
        CheckUsability();

        _graphics.DrawEllipse((Pen)pen.EngineElement, x1, y1, x2, y2);
    }

    public void FillEllipse(IBrush brush, float x1, float y1, float x2, float y2)
    {
        CheckUsability();

        _graphics.FillEllipse((Brush)brush.EngineElement, x1, y1, x2, y2);
    }

    public void DrawPie(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        CheckUsability();

        _graphics.DrawPie((Pen)pen.EngineElement, rect.ToGdiRectangle(), startAngle, sweepAngle);
    }

    public void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        CheckUsability();

        _graphics.FillPie((Brush)brush.EngineElement, rect.ToGdiRectangle(), startAngle, sweepAngle);
    }

    public void DrawPath(IPen pen, IGraphicsPath path)
    {
        CheckUsability();

        _graphics.DrawPath((Pen)pen.EngineElement, (System.Drawing.Drawing2D.GraphicsPath)path.EngineElement);
    }
    public void FillPath(IBrush brush, IGraphicsPath path)
    {
        CheckUsability();

        _graphics.FillPath((Brush)brush.EngineElement, (System.Drawing.Drawing2D.GraphicsPath)path.EngineElement);
    }

    public void FillPath(IBrushCollection brushCollection, IGraphicsPath path)
    {
        CheckUsability();

        foreach (var brush in brushCollection.Brushes)
        {
            _graphics.FillPath((Brush)brush.EngineElement, (System.Drawing.Drawing2D.GraphicsPath)path.EngineElement);
        }
    }

    #endregion ICanvas

    #region IDisposable

    public void Dispose()
    {
        if (_graphics != null)
        {
            _graphics.Dispose();
            _graphics = null;
        }
    }

    #endregion IDisposable

    #region Helper

    public CanvasRectangleF FitDrawBitmapSourceRectangle(CanvasRectangleF rect)
        => this.InterpolationMode switch
        {
            InterpolationMode.Bilinear
                => new CanvasRectangleF(rect.Left, rect.Top, rect.Width - 1f, rect.Height - 1f),
            InterpolationMode.Bicubic
                => new CanvasRectangleF(rect.Left, rect.Top, rect.Width - 1f, rect.Height - 1f),
            InterpolationMode.NearestNeighbor
                => new CanvasRectangleF(-0.5f + rect.Left, -0.5f + rect.Top, rect.Width, rect.Height),
            _ => rect
        };

    private void CheckUsability()
    {
        if (_graphics == null)
        {
            throw new Exception("Canvas already disposed...");
        }
    }

    private System.Drawing.Imaging.ImageAttributes CreateImageAttributes(float opacity)
    {
        if (opacity >= 0 && opacity < 1f)
        {
            float[][] ptsArray ={
                                    new float[] {1, 0, 0, 0, 0},
                                    new float[] {0, 1, 0, 0, 0},
                                    new float[] {0, 0, 1, 0, 0},
                                    new float[] {0, 0, 0, opacity, 0},
                                    new float[] {0, 0, 0, 0, 1}};

            System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
            System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
            imgAttributes.SetColorMatrix(clrMatrix,
                                         System.Drawing.Imaging.ColorMatrixFlag.Default,
                                         System.Drawing.Imaging.ColorAdjustType.Bitmap);

            return imgAttributes;
        }

        return null;
    }


    #endregion Helper
}
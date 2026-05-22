#nullable enable

using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Extensions;
using gView.GraphicsEngine.Skia3x.Extensions;
using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace gView.GraphicsEngine.Skia3x;

class SkiaCanvas : ICanvas
{
    private SKCanvas? _canvas;

    internal SkiaCanvas(SKCanvas canvas)
    {
        _canvas = canvas;
    }

    public float DpiX => 96f;

    public float DpiY => 96f;

    public CompositingMode CompositingMode { set { } }
    public InterpolationMode InterpolationMode { get; set; }
    public SmoothingMode SmoothingMode { get; set; }
    public TextRenderingHint TextRenderingHint { get; set; }

    public void Clear(ArgbColor? argbColor = null)
    {
        _canvas?.Clear(argbColor.HasValue ? argbColor.Value.ToSKColor() : ArgbColor.White.ToSKColor());
    }

    public IDisplayCharacterRanges DisplayCharacterRanges(IFont font, IDrawTextFormat format, string text)
    {
        return new DisplayCharacterRanges(_canvas, GetSKFont(font), format, text);
    }

    public void Flush()
    {
        _canvas?.Flush();
    }

    public void Dispose()
    {
        if (_canvas != null)
        {
            _canvas?.Flush();
            _canvas?.Dispose();
            _canvas = null;
        }
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPoint point)
    {
        using var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
        _canvas?.DrawImage(image, point.ToSKPoint(), this.InterpolationMode.ToSKSamplingOpitons());

        //_canvas?.DrawBitmap((SKBitmap)bitmap.EngineElement, point.ToSKPoint(), new SKPaint()
        //{

        //    FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
        //});
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPointF pointF)
    {
        using var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
        _canvas?.DrawImage(image, pointF.ToSKPoint(), this.InterpolationMode.ToSKSamplingOpitons());

        //_canvas?.DrawBitmap((SKBitmap)bitmap.EngineElement, pointF.ToSKPoint(), new SKPaint()
        //{
        //    FilterQuality = this.InterpolationMode.ToSKFilterQuality()
        //});
    }

    public void DrawBitmap(IBitmap bitmap, CanvasRectangle dest, CanvasRectangle source, float opacity = 1)
    {
        using var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
        _canvas?.DrawImage(image, source.ToSKRect(), dest.ToSKRect(), this.InterpolationMode.ToSKSamplingOpitons(), new SKPaint()
        {
            Color = SKColors.Black.WithAlpha((byte)(255 * opacity)),

        });

        //_canvas?.DrawBitmap((SKBitmap)bitmap.EngineElement, source.ToSKRect(), dest.ToSKRect(), new SKPaint()
        //{
        //    FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
        //    Color = SKColors.Black.WithAlpha((byte)(255 * opacity)),

        //});
    }

    public void DrawBitmap(IBitmap bitmap, CanvasRectangleF dest, CanvasRectangleF source)
    {
        using var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
        _canvas?.DrawImage(
            image,
            FitDrawBitmapSourceRectangle(source).ToSKRect(),
            dest.ToSKRect(),
            this.InterpolationMode.ToSKSamplingOpitons());

        //_canvas?.DrawBitmap(
        //    (SKBitmap)bitmap.EngineElement,
        //    FitDrawBitmapSourceRectangle(source).ToSKRect(),
        //    dest.ToSKRect(),
        //    new SKPaint()
        //    {
        //        FilterQuality = this.InterpolationMode.ToSKFilterQuality()
        //    });
    }

    public void DrawBitmap(IBitmap bitmap, CanvasPointF[] points, CanvasRectangleF source, float opacity = 1)
    {
        // not correct
        // var dest = new CanvasRectangleF(points[0].X, points[0].Y, points[1].X - points[0].X, points[2].Y - points[0].Y);

        // solution
        // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/transforms/3d-rotation
        var skMatrix = ComputeMatrix(new SKSize(source.Width, source.Height),
                                     points[0].ToSKPoint(),
                                     points[1].ToSKPoint(),
                                     points[2].ToSKPoint(),
                                     points.Length > 3 ? (SKPoint?)points[3].ToSKPoint() : null);


        _canvas?.SetMatrix(skMatrix);

        using var image = SKImage.FromBitmap((SKBitmap)bitmap.EngineElement);
        _canvas?.DrawImage(image, 0, 0,
            this.InterpolationMode.ToSKSamplingOpitons(),
            new SKPaint()
            {
                Color = SKColors.Black.WithAlpha((byte)(255 * opacity))
            });

        //_canvas?.DrawBitmap((SKBitmap)bitmap.EngineElement, 0f, 0f, new SKPaint()
        //{
        //    FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
        //    Color = SKColors.Black.WithAlpha((byte)(255 * opacity))
        //});

        _canvas?.ResetMatrix();
    }

    public void DrawEllipse(IPen pen, float x1, float y1, float width, float height)
    {
        _canvas?.DrawOval(new SKRect(x1, y1, x1 + width, y1 + height), GetSKPaint(pen));
    }

    public void FillEllipse(IBrush brush, float x1, float y1, float width, float height)
    {
        _canvas?.DrawOval(new SKRect(x1, y1, x1 + width, y1 + height), GetSKPaint(brush));
    }

    public void DrawPie(IPen pen, CanvasRectangle rect, float startAngle, float sweepAngle)
        => _canvas?.DrawArc(rect.ToSKRect(), startAngle, sweepAngle, true, GetSKPaint(pen));

    public void FillPie(IBrush brush, CanvasRectangle rect, float startAngle, float sweepAngle)
         => _canvas?.DrawArc(rect.ToSKRect(), startAngle, sweepAngle, true, GetSKPaint(brush));


    public void DrawRectangle(IPen pen, CanvasRectangle rectangle)
    {
        _canvas?.DrawRect(rectangle.ToSKRect(), GetSKPaint(pen));
    }

    public void DrawRectangle(IPen pen, CanvasRectangleF rectangleF)
    {
        _canvas?.DrawRect(rectangleF.ToSKRect(), GetSKPaint(pen));
    }

    public void FillRectangle(IBrush brush, CanvasRectangle rectangle)
    {
        _canvas?.DrawRect(rectangle.ToSKRect(), GetSKPaint(brush));
    }

    public void FillRectangle(IBrush brush, CanvasRectangleF rectangleF)
    {
        _canvas?.DrawRect(rectangleF.ToSKRect(), GetSKPaint(brush));
    }

    public void FillRectangle(IBrushCollection brushCollection, CanvasRectangleF rectangleF)
    {
        foreach (var brush in brushCollection.Brushes)
        {
            _canvas?.DrawRect(rectangleF.ToSKRect(), GetSKPaint(brush));
        }
    }

    public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
    {
        _canvas?.DrawLine(p1.ToSKPoint(), p2.ToSKPoint(), GetSKPaint(pen));
    }

    public void DrawLine(IPen pen, CanvasPointF p1, CanvasPointF p2)
    {
        _canvas?.DrawLine(p1.ToSKPoint(), p2.ToSKPoint(), GetSKPaint(pen));
    }

    public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
    {
        _canvas?.DrawLine(x1, y1, x2, y2, GetSKPaint(pen));
    }

    public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
    {
        _canvas?.DrawLine(x1, y1, x2, y2, GetSKPaint(pen));
    }

    public void DrawPath(IPen pen, IGraphicsPath path)
    {
        _canvas?.DrawPath((SKPath)path.EngineElement, GetSKPaint(pen));
    }

    public void FillPath(IBrush brush, IGraphicsPath path)
    {
        _canvas?.DrawPath((SKPath)path.EngineElement, GetSKPaint(brush));
    }

    public void FillPath(IBrushCollection brushCollection, IGraphicsPath path)
    {
        foreach (var brush in brushCollection.Brushes)
        {
            _canvas?.DrawPath((SKPath)path.EngineElement, GetSKPaint(brush));
        }
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point)
    {
        DrawMultilineText(text.RemoveReturns(), point.ToSKPoint(), (SKPaint)brush.EngineElement, font);
    }

    public void DrawText(string text, IFont font, IBrush brush, int x, int y)
    {
        DrawMultilineText(text.RemoveReturns(), x, y, (SKPaint)brush.EngineElement, font);
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF)
    {
        DrawMultilineText(text.RemoveReturns(), pointF.ToSKPoint(), (SKPaint)brush.EngineElement, font);
    }

    public void DrawText(string text, IFont font, IBrush brush, float x, float y)
    {
        DrawMultilineText(text.RemoveReturns(), x, y, (SKPaint)brush.EngineElement, font);
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasRectangleF rectangleF)
    {
        var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement);
        var center = rectangleF.Center;
        var size = this.MeasureText(text, font);

        DrawMultilineText(
                text.RemoveReturns(),
                new CanvasPointF(center.X - size.Width / 2f, center.Y - size.Height / 2f).ToSKPoint(),
                skPaint,
                font
             );
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point, IDrawTextFormat format)
    {
        var skPoint = point.ToSKPoint();
        var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

        DrawMultilineText(text, skPoint, skPaint, font, format);
    }

    public void DrawText(string text, IFont font, IBrush brush, int x, int y, IDrawTextFormat format)
    {
        var skPoint = new SKPoint(x, y);
        var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

        DrawMultilineText(text, skPoint, skPaint, font, format);
    }

    public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF, IDrawTextFormat format)
    {
        var skPoint = pointF.ToSKPoint();
        var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

        DrawMultilineText(text, skPoint, skPaint, font, format);
    }

    public void DrawText(string text, IFont font, IBrush brush, float x, float y, IDrawTextFormat format)
    {
        var skPoint = new SKPoint(x, y);
        var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

        DrawMultilineText(text, skPoint, skPaint, font, format);
    }

    public CanvasSizeF MeasureText(string text, IFont font)
    {
        text = text.RemoveReturns();

        SKRect bounds = new SKRect();
        var size = new CanvasSizeF();

        if (text.IsMultiline())
        {
            var fontHeight = font.Size.FontSizePointsToPixels();
            foreach (var line in text.GetLines())
            {
                GetSKFont(font).MeasureText(line, out bounds);
                size.Width = Math.Max(size.Width, bounds.Width);
                size.Height += fontHeight;
            }
        }
        else
        {
            GetSKFont(font).MeasureText(text, out bounds);
            size.Width = bounds.Width;
            size.Height = bounds.Height;
        }

        if (font.Style.HasFlag(FontStyle.Underline))
        {
            size.Height *= 1.1f;
        }

        return size;
    }

    public void ResetTransform()
    {
        _canvas?.ResetMatrix();
    }

    public void RotateTransform(float angle)
    {
        _canvas?.RotateDegrees(angle);
    }

    public void TranslateTransform(CanvasPointF point)
    {
        _canvas?.Translate(point.ToSKPoint());
    }

    public void ScaleTransform(float sx, float sy)
    {
        _canvas?.Scale(sx, sy);
    }

    public void SetClip(CanvasRectangle rectangle)
    {
        _canvas?.ClipRect(rectangle.ToSKRect());
    }

    public void SetClip(CanvasRectangleF rectangle)
    {
        _canvas?.ClipRect(rectangle.ToSKRect());
    }


    #region Helper
    public CanvasRectangleF FitDrawBitmapSourceRectangle(CanvasRectangleF rect)
        => this.InterpolationMode switch
        {
            _ => rect
        };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IPen pen)
    {
        var skPaint = (SKPaint)pen.EngineElement;

        skPaint.IsAntialias = this.SmoothingMode == SmoothingMode.AntiAlias;

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IBrush brush)
    {
        var skPaint = (SKPaint)brush.EngineElement;

        skPaint.IsAntialias = this.SmoothingMode == SmoothingMode.AntiAlias;

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IFont font)
    {
        var skPaint = ((SkiaFontPaint)(font.EngineElement)).SKPaint;

        skPaint.IsAntialias = this.SmoothingMode == SmoothingMode.AntiAlias;
        skPaint.TextEncoding = SKTextEncoding.Utf16;

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKFont GetSKFont(IFont font)
    {
        var skFont = ((SkiaFontPaint)(font.EngineElement)).SKFont;

        return skFont;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IFont font, SKPaint brush)
    {
        var skPaint = GetSKPaint(font);

        skPaint.Color = brush.Color;
        skPaint.ColorF = brush.ColorF;

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SKPaint GetSKPaint(IFont font, SKPaint brush, IDrawTextFormat? format, ref string text, ref SKPoint point)
    {
        var skPaint = GetSKPaint(font, brush);
        var skFont = GetSKFont(font);

        if (this.TextRenderingHint == TextRenderingHint.AntiAlias)
        {
            skPaint.IsAntialias = true;
        }

        text = text.RemoveReturns();

        if (format != null)
        {
            var skAlignment = (SKTextAlign)format.EngineElement;

            //skPaint.TextAlign = skAlignment;
            //if(format.LineAlignment != StringAlignment.Far)
            {
                //var height = font.Size.FontSizePointsToPixels() * 0.72f; //this.MeasureText("X", font).Height;
                //switch(format.LineAlignment)
                //{
                //    case StringAlignment.Center:
                //        point.Y += skPaint.FontMetrics != null ? skPaint.FontMetrics.XHeight / 2f : height / 2.5f;
                //        break;
                //    case StringAlignment.Near:
                //        point.Y += skPaint.FontMetrics != null ? skPaint.FontMetrics.CapHeight : height;
                //        break;
                //}

                //var capHeight = skPaint.FontMetrics != null ? skPaint.FontMetrics.CapHeight : height * 0.75f;


                var far = -skFont.Metrics.Bottom; // -skPaint.FontMetrics.Bottom /*- height * 0.00f*/;
                var near = -skFont.Metrics.Top; // -skPaint.FontMetrics.Top;

                switch (format.LineAlignment)
                {
                    case StringAlignment.Far:
                        point.Y += far;

                        var span = text.AsSpan();
                        if (span.IsMultiline())
                        {
                            point.Y += (span.LinesCount() - 1) * (skFont.Metrics.Ascent /*skPaint.FontMetrics.Ascent*/);
                        }

                        break;
                    case StringAlignment.Center:
                        point.Y += (far + near) * .5f;

                        var span2 = text.AsSpan();
                        if (span2.IsMultiline())
                        {
                            point.Y += (span2.LinesCount() - 1) * (skFont.Metrics.Ascent /*skPaint.FontMetrics.Ascent*/) * .5f;
                        }

                        //point.Y = this.MeasureText(text, font).Height;
                        break;
                    case StringAlignment.Near:
                        point.Y += near;
                        break;
                }
            }
        }

        return skPaint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawMultilineText(string text, SKPoint point, SKPaint brush, IFont font, IDrawTextFormat? format = null)
    {
        var skPaint = GetSKPaint(font, brush);
        var skFont = GetSKFont(font);

        if (text.Contains("\n"))
        {
            String[] lines = text.Replace("\r", "").Split('\n');

            foreach (var line in lines)
            {
                DrawText(line, point.X, point.Y, skPaint, skFont, font, format);

                point.Y += skFont.Size;
            }
        }
        else
        {
            DrawText(text, point.X, point.Y, skPaint, skFont, font, format);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawMultilineText(string text, float x, float y, SKPaint brush, IFont font, IDrawTextFormat? format = null)
    {
        var skPaint = GetSKPaint(font, brush);
        var skFont = GetSKFont(font);

        if (text.Contains("\n"))
        {
            String[] lines = text.Replace("\r", "").Split('\n');

            foreach (var line in lines)
            {
                DrawText(line, x, y, skPaint, skFont, font, format);

                y += skFont.Size; // skPaint.TextSize;
            }
        }
        else
        {
            DrawText(text, x, y, skPaint, skFont, font, format);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawText(string text, float x, float y, SKPaint skPaint, SKFont skFont, IFont font, IDrawTextFormat? format = null)
    {
        if(_canvas is null) { return; }

        font.LockObject.InterLock(() =>
            _canvas?.DrawText(text, x, y, 
                format?.Alignment.ToSkTextAlign() ?? SKTextAlign.Left, 
                skFont, skPaint)
        );

        if (font.Style.HasFlag(FontStyle.Underline) ||
            font.Style.HasFlag(FontStyle.Strikeout))
        {
            #region Draw underline/strikout line

            var sizeF = this.MeasureText(text, font);
            float strokeWidth = skPaint.StrokeWidth;
            float w2 = sizeF.Width / 2f,
                  x1 = x - sizeF.Width / 2f,
                  x2 = x + sizeF.Width / 2f;

            switch (format?.Alignment)
            {
                case StringAlignment.Near:
                    x1 += w2;
                    x2 += w2;
                    break;
                case StringAlignment.Far:
                    x1 -= w2;
                    x2 -= w2;
                    break;
            }

            if (font.Style.HasFlag(FontStyle.Underline))
            {
                float lineY = y + skFont.Size * 0.1f;

                skPaint.StrokeWidth = skFont.Size * 0.1f;
                _canvas.DrawLine(x1, lineY,
                                 x2, lineY, skPaint);
            }
            if (font.Style.HasFlag(FontStyle.Strikeout))
            {
                float lineY = y - skFont.Size / 3f * 0.82f; // empiric

                skPaint.StrokeWidth = skFont.Size * 0.05f;
                _canvas.DrawLine(x1, lineY,
                                 x2, lineY, skPaint);
            }

            skPaint.StrokeWidth = strokeWidth;

            #endregion
        }
    }

    #region Matrix

    private SKMatrix ComputeMatrix(SKSize size, SKPoint ptUL, SKPoint ptUR, SKPoint ptLL, SKPoint? ptLR = null)
    {
        // Scale transform
        SKMatrix S = SKMatrix.CreateScale(1 / size.Width, 1 / size.Height);

        // Affine transform
        SKMatrix A = new SKMatrix
        {
            ScaleX = ptUR.X - ptUL.X,
            SkewY = ptUR.Y - ptUL.Y,
            SkewX = ptLL.X - ptUL.X,
            ScaleY = ptLL.Y - ptUL.Y,
            TransX = ptUL.X,
            TransY = ptUL.Y,
            Persp2 = 1
        };

        // Non-Affine transform
        SKMatrix N = SKMatrix.CreateIdentity();

        if (ptLR.HasValue)
        {
            SKMatrix inverseA;
            A.TryInvert(out inverseA);
            SKPoint abPoint = inverseA.MapPoint(ptLR.Value);
            float a = abPoint.X;
            float b = abPoint.Y;

            float scaleX = a / (a + b - 1);
            float scaleY = b / (a + b - 1);

            N = new SKMatrix
            {
                ScaleX = scaleX,
                ScaleY = scaleY,
                Persp0 = scaleX - 1,
                Persp1 = scaleY - 1,
                Persp2 = 1
            };
        }

        // Multiply S * N * A
        SKMatrix result = SKMatrix.CreateIdentity();

        //SKMatrix.PostConcat(ref result, S);
        //SKMatrix.PostConcat(ref result, N);
        //SKMatrix.PostConcat(ref result, A);

        result = result.PostConcat(S);
        result = result.PostConcat(N);
        result = result.PostConcat(A);

        return result;
    }

    #endregion

    #endregion
}
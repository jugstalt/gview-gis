using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace gView.GraphicsEngine.Skia
{
    class SkiaCanvas : ICanvas
    {
        private SKCanvas _canvas;

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
            _canvas.Clear(argbColor.HasValue ? argbColor.Value.ToSKColor() : ArgbColor.White.ToSKColor());
        }

        public IDisplayCharacterRanges DisplayCharacterRanges(IFont font, IDrawTextFormat format, string text)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _canvas.Dispose();
        }

        public void DrawBitmap(IBitmap bitmap, CanvasPoint point)
        {
            _canvas.DrawBitmap((SKBitmap)bitmap.EngineElement, point.ToSKPoint(), new SKPaint()
            {
                FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
            });
        }

        public void DrawBitmap(IBitmap bitmap, CanvasPointF pointF)
        {
            _canvas.DrawBitmap((SKBitmap)bitmap.EngineElement, pointF.ToSKPoint(), new SKPaint()
            {
                FilterQuality = this.InterpolationMode.ToSKFilterQuality()
            });
        }

        public void DrawBitmap(IBitmap bitmap, CanvasRectangle dest, CanvasRectangle source, float opacity = 1)
        {
            _canvas.DrawBitmap((SKBitmap)bitmap.EngineElement, source.ToSKRect(), dest.ToSKRect(), new SKPaint()
            {
                FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
                Color = SKColors.Black.WithAlpha((byte)(255 * opacity))
            }); ;
        }

        public void DrawBitmap(IBitmap bitmap, CanvasRectangleF dest, CanvasRectangleF source)
        {
            _canvas.DrawBitmap((SKBitmap)bitmap.EngineElement, source.ToSKRect(), dest.ToSKRect(), new SKPaint()
            {
                FilterQuality = this.InterpolationMode.ToSKFilterQuality()
            });
        }

        public void DrawBitmap(IBitmap bitmap, CanvasPointF[] points, CanvasRectangleF source, float opacity = 1)
        {
            // ToDo: Nicht korrekt!!
            //var dest = new CanvasRectangleF(points[0].X, points[0].Y, points[1].X - points[0].X, points[2].Y - points[0].Y);

            // siehe: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/transforms/3d-rotation
            var skMatrix = ComputeMatrix(new SKSize(source.Width, source.Height),
                                         points[0].ToSKPoint(),
                                         points[1].ToSKPoint(),
                                         points[2].ToSKPoint(),
                                         points.Length > 3 ? (SKPoint?)points[3].ToSKPoint() : null);


            _canvas.SetMatrix(skMatrix);

            _canvas.DrawBitmap((SKBitmap)bitmap.EngineElement, 0f, 0f, new SKPaint()
            {
                FilterQuality = this.InterpolationMode.ToSKFilterQuality(),
                Color = SKColors.Black.WithAlpha((byte)(255 * opacity))
            });

            _canvas.ResetMatrix();
        }

        public void DrawEllipse(IPen pen, float x1, float y1, float width, float height)
        {
            _canvas.DrawOval(new SKRect(x1, y1, x1 + width, y1 + width), GetSKPaint(pen));
        }

        public void FillEllipse(IBrush brush, float x1, float y1, float width, float height)
        {
            _canvas.DrawOval(new SKRect(x1, y1, x1 + width, y1 + width), GetSKPaint(brush));
        }

        public void DrawRectangle(IPen pen, CanvasRectangle rectangle)
        {
            _canvas.DrawRect(rectangle.ToSKRect(), GetSKPaint(pen));
        }

        public void DrawRectangle(IPen pen, CanvasRectangleF rectangleF)
        {
            _canvas.DrawRect(rectangleF.ToSKRect(), GetSKPaint(pen));
        }

        public void FillRectangle(IBrush brush, CanvasRectangle rectangle)
        {
            _canvas.DrawRect(rectangle.ToSKRect(), GetSKPaint(brush));
        }

        public void FillRectangle(IBrush brush, CanvasRectangleF rectangleF)
        {
            _canvas.DrawRect(rectangleF.ToSKRect(), GetSKPaint(brush));
        }

        public void DrawLine(IPen pen, CanvasPoint p1, CanvasPoint p2)
        {
            _canvas.DrawLine(p1.ToSKPoint(), p2.ToSKPoint(), GetSKPaint(pen));
        }

        public void DrawLine(IPen pen, CanvasPointF p1, CanvasPointF p2)
        {
            _canvas.DrawLine(p1.ToSKPoint(), p2.ToSKPoint(), GetSKPaint(pen));
        }

        public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
        {
            _canvas.DrawLine(x1, y1, x2, y2, GetSKPaint(pen));
        }

        public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
        {
            _canvas.DrawLine(x1, y1, x2, y2, GetSKPaint(pen));
        }

        public void DrawPath(IPen pen, IGraphicsPath path)
        {
            _canvas.DrawPath((SKPath)path.EngineElement, GetSKPaint(pen));
        }

        public void FillPath(IBrush brush, IGraphicsPath path)
        {
            _canvas.DrawPath((SKPath)path.EngineElement, GetSKPaint(brush));
        }

        public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point)
        {
            DrawMultilineText(text.RemoveReturns(), point.ToSKPoint(), GetSKPaint(font, (SKPaint)brush.EngineElement));
        }

        public void DrawText(string text, IFont font, IBrush brush, int x, int y)
        {
            DrawMultilineText(text.RemoveReturns(), x, y, GetSKPaint(font, (SKPaint)brush.EngineElement));
        }

        public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF)
        {
            DrawMultilineText(text.RemoveReturns(), pointF.ToSKPoint(), GetSKPaint(font, (SKPaint)brush.EngineElement));
        }

        public void DrawText(string text, IFont font, IBrush brush, float x, float y)
        {
            DrawMultilineText(text.RemoveReturns(), x, y, GetSKPaint(font, (SKPaint)brush.EngineElement));
        }

        public void DrawText(string text, IFont font, IBrush brush, CanvasRectangleF rectangleF)
        {
            DrawMultilineText(text.RemoveReturns(), rectangleF.Center.ToSKPoint(), GetSKPaint(font, (SKPaint)brush.EngineElement));
        }

        public void DrawText(string text, IFont font, IBrush brush, CanvasPoint point, IDrawTextFormat format)
        {
            var skPoint = point.ToSKPoint();
            var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

            DrawMultilineText(text, skPoint, skPaint);
        }

        public void DrawText(string text, IFont font, IBrush brush, int x, int y, IDrawTextFormat format)
        {
            var skPoint = new SKPoint(x, y);
            var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

            DrawMultilineText(text, skPoint, skPaint);
        }

        public void DrawText(string text, IFont font, IBrush brush, CanvasPointF pointF, IDrawTextFormat format)
        {
            var skPoint = pointF.ToSKPoint();
            var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

            DrawMultilineText(text, skPoint, skPaint);
        }

        public void DrawText(string text, IFont font, IBrush brush, float x, float y, IDrawTextFormat format)
        {
            var skPoint = new SKPoint(x, y);
            var skPaint = GetSKPaint(font, (SKPaint)brush.EngineElement, format, ref text, ref skPoint);

            DrawMultilineText(text, skPoint, skPaint);
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
                    GetSKPaint(font).MeasureText(line, ref bounds);
                    size.Width = Math.Max(size.Width, bounds.Width);
                    size.Height += fontHeight;
                }
            }
            else
            {
                GetSKPaint(font).MeasureText(text, ref bounds);
                size.Width = bounds.Width;
                size.Height = bounds.Height;
            }

            return size;
        }

        public void ResetTransform()
        {
            _canvas.ResetMatrix();
        }

        public void RotateTransform(float angle)
        {
            _canvas.RotateDegrees(angle);
        }

        public void TranslateTransform(CanvasPointF point)
        {
            _canvas.Translate(point.ToSKPoint());
        }

        #region Helper

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
            var skPaint = (SKPaint)font.EngineElement;

            skPaint.IsAntialias = this.SmoothingMode == SmoothingMode.AntiAlias;
            skPaint.TextEncoding = SKTextEncoding.Utf16;

            return skPaint;
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
        private SKPaint GetSKPaint(IFont font, SKPaint brush, IDrawTextFormat format, ref string text, ref SKPoint point)
        {
            var skPaint = GetSKPaint(font, brush);
            text = text.RemoveReturns();

            if(format!=null)
            {
                var skAlignment = (SKPaint)format.EngineElement;

                skPaint.TextAlign = skAlignment.TextAlign;
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

                    var far = -skPaint.FontMetrics.Bottom /*- height * 0.00f*/;
                    var near = -skPaint.FontMetrics.Top;

                    switch (format.LineAlignment)
                    {
                        case StringAlignment.Far:
                            point.Y += far;
                            break;
                        case StringAlignment.Center:
                            point.Y += (far + near) * .5f;
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
        private void DrawMultilineText(string text, SKPoint point, SKPaint paint)
        {
            if (text.Contains("\n"))
            {
                String[] lines = text.Replace("\r","").Split('\n');

                foreach(var line in lines)
                {
                    _canvas.DrawText(line, point, paint);

                    point.Y += paint.TextSize;
                }
            }
            else
            {
                _canvas.DrawText(text, point, paint);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawMultilineText(string text, float x, float y, SKPaint paint)
        {
            if (text.Contains("\n"))
            {
                String[] lines = text.Replace("\r", "").Split('\n');

                foreach (var line in lines)
                {
                    _canvas.DrawText(line, x, y, paint);

                    y += paint.TextSize;
                }
            }
            else
            {
                _canvas.DrawText(text, x, y, paint);
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

            SKMatrix.PostConcat(ref result, S);
            SKMatrix.PostConcat(ref result, N);
            SKMatrix.PostConcat(ref result, A);

            return result;
        }

        #endregion

        #endregion
    }
}
 
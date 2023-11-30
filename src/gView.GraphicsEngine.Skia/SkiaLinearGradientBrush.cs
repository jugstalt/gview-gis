using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;

namespace gView.GraphicsEngine.Skia
{
    class SkiaLinearGradientBrush : IBrush
    {
        private SKPaint _skPaint;

        public SkiaLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            var center = rect.Center;

            angle = angle * (float)(Math.PI / 180.0);

            var angleRect = new SKRect(center.X - rect.Width * (float)Math.Cos(angle),
                                       center.Y - rect.Width * (float)Math.Sin(angle),
                                       center.X + rect.Width * (float)Math.Cos(angle),
                                       center.Y + rect.Width * (float)Math.Sin(angle));

            _skPaint = new SKPaint()
            {
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(angleRect.Left, angleRect.Top),
                    new SKPoint(angleRect.Right, angleRect.Bottom),
                    new SKColor[] { col1.ToSKColor(), col2.ToSKColor() },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp)
            };
        }

        public ArgbColor Color { get; set; }

        public object EngineElement => _skPaint;

        public void Dispose()
        {
            _skPaint.Dispose();
        }
    }
}

using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System.Collections.Generic;

namespace gView.GraphicsEngine.Skia
{
    class SkiaHatchBrush : IBrush
    {
        private SKPaint _skPaint;

        public SkiaHatchBrush(ArgbColor color, float angle, float distance = 5, float widh = 2f)
        {
            _skPaint = new SKPaint()
            {
                PathEffect = SKPathEffect.Create2DLine(widh, Multiply(SKMatrix.CreateRotationDegrees(angle), SKMatrix.CreateScale(1.007f, distance))),
                Color = color.ToSKColor(),
                IsAntialias = true,
            };
        }

        public object EngineElement => _skPaint;

        public ArgbColor Color { get; set; }

        public void Dispose()
        {
            _skPaint.Dispose();
        }

        #region Helper

        static SKMatrix Multiply(SKMatrix first, SKMatrix second)
        {
            SKMatrix target = SKMatrix.CreateIdentity();
            SKMatrix.Concat(ref target, first, second);
            return target;
        }

        #endregion

        #region Static Members

        static public IBrushCollection CreateCollection(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            List<IBrush> brushes = new List<IBrush>();

            if (backColor.A > 0)
            {
                brushes.Add(new SkiaSolidBrush(backColor));
            }

            switch (hatchStyle)
            {
                case HatchStyle.Horizontal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 0));
                    break;
                case HatchStyle.Vertical:
                    brushes.Add(new SkiaHatchBrush(foreColor, 90));
                    break;
                case HatchStyle.ForwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 45));
                    break;
                case HatchStyle.BackwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, -45));
                    break;
                case HatchStyle.SmallGrid:
                    brushes.Add(new SkiaHatchBrush(foreColor, 0));
                    brushes.Add(new SkiaHatchBrush(foreColor, 90));
                    break;
                case HatchStyle.LargeGrid:
                    brushes.Add(new SkiaHatchBrush(foreColor, 0, 15));
                    brushes.Add(new SkiaHatchBrush(foreColor, 90, 15));
                    break;
                case HatchStyle.DiagonalCross:
                    brushes.Add(new SkiaHatchBrush(foreColor, 45));
                    brushes.Add(new SkiaHatchBrush(foreColor, -45));
                    break;
                case HatchStyle.DarkHorizontal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 0, 3));
                    break;
                case HatchStyle.DarkVertical:
                    brushes.Add(new SkiaHatchBrush(foreColor, 90, 3));
                    break;
                case HatchStyle.DarkDownwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 45, 3));
                    break;
                case HatchStyle.DarkUpwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, -45, 3));
                    break;
                case HatchStyle.LightHorizontal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 0, 15));
                    break;
                case HatchStyle.LightVertical:
                    brushes.Add(new SkiaHatchBrush(foreColor, 90, 15));
                    break;
                case HatchStyle.LightDownwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, 45, 15));
                    break;
                case HatchStyle.LightUpwardDiagonal:
                    brushes.Add(new SkiaHatchBrush(foreColor, -45, 15));
                    break;
            }

            return new BrushCollection(brushes);
        }

        #endregion
    }
}

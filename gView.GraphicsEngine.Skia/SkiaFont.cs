using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class SkiaFont : IFont
    {
        private SKPaint _skPaint;

        public SkiaFont(string name, float size, FontStyle fontStyle, GraphicsUnit unit, char? typefaceCharakter = null)
        {
            var pixelSize = size;
            switch(unit)
            {
                case GraphicsUnit.Point:
                    pixelSize = size.FontSizePointsToPixels();
                    break;
            }

            var skFont = new SKFont(SKTypeface.FromFamilyName(name, fontStyle.ToSKFontStyle()), size: pixelSize);

            _skPaint = new SKPaint(skFont)
            {
                Style = SKPaintStyle.Fill
            };

            this.Name = name;
            this.Size = size;
            this.Style = fontStyle;
            this.Unit = unit;

            if(typefaceCharakter.HasValue)
            {
                var fontManager = SKFontManager.Default;
                var typeFace = fontManager.MatchCharacter(name, typefaceCharakter.Value);
                if(typeFace!=null)
                {
                    _skPaint.Typeface = typeFace;
                }
            }
        }

        public string Name { get; }

        public float Size { get; }

        public FontStyle Style { get; }

        public GraphicsUnit Unit { get; }

        public object EngineElement => _skPaint;

        public void Dispose()
        {
            _skPaint.Dispose();
        }
    }
}

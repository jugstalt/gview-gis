using gView.GraphicsEngine.Abstraction;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia
{
    class SkiaFont : IFont
    {
        private SKPaint _skPaint;

        public SkiaFont(string name, float size, FontStyle fontStyle, GraphicsUnit unit)
        {
            _skPaint = new SKPaint(new SKFont(SKTypeface.Default, size: size));
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

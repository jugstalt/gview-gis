using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Concurrent;

namespace gView.GraphicsEngine.Skia
{
    class SkiaFont : IFont
    {
        private static ConcurrentDictionary<IntPtr, object> _lockers = new ConcurrentDictionary<IntPtr, object>();

        private object _locker = null;
        private SKPaint _skPaint;

        public SkiaFont(string name, float size, FontStyle fontStyle, GraphicsUnit unit, char? typefaceCharakter = null)
        {
            var pixelSize = size;
            switch (unit)
            {
                case GraphicsUnit.Point:
                    pixelSize = size.FontSizePointsToPixels();
                    break;
            }

            var fontTypeFace = SKTypeface.FromFamilyName(name, fontStyle.ToSKFontStyle());
            var skFont = new SKFont(fontTypeFace, size: pixelSize);

            // SKTypeface is not thread safe
            // https://groups.google.com/g/skia-discuss/c/-G1cyl1QD9E
            if (!_lockers.ContainsKey(fontTypeFace.Handle))
            {
                _lockers.TryAdd(fontTypeFace.Handle, new object());
            }
            _lockers.TryGetValue(fontTypeFace.Handle, out _locker);

            _skPaint = new SKPaint(skFont)
            {
                Style = SKPaintStyle.Fill
            };

            this.Name = name;
            this.Size = size;
            this.Style = fontStyle;
            this.Unit = unit;

            if (typefaceCharakter.HasValue)
            {
                var fontManager = SKFontManager.Default;
                var typeFace = fontManager.MatchCharacter(name, typefaceCharakter.Value);
                if (typeFace != null)
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

        public object LockObject => _locker;

        public void Dispose()
        {
            _skPaint.Dispose();
        }
    }
}

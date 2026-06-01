using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Extensions;
using gView.GraphicsEngine.Skia.Extensions;
using gView.GraphicsEngine.Threading;
using SkiaSharp;
using System;
using System.Collections.Concurrent;

namespace gView.GraphicsEngine.Skia
{
    class SkiaFont : IFont
    {
        private static ConcurrentDictionary<IntPtr, IThreadLocker> _threadLockers = new ConcurrentDictionary<IntPtr, IThreadLocker>();
        private static IThreadLocker _threadLocker=new ThreadLocker();

        private IThreadLocker _locker = null;
        private SKPaint _skPaint;
        private SKFont _skFont;
        private SkiaFontPaint _skFontPaint;

        public SkiaFont(string name, float size, FontStyle fontStyle, GraphicsUnit unit/*, char? typefaceCharakter = null*/)
        {
            var pixelSize = size;
            switch (unit)
            {
                case GraphicsUnit.Point:
                    pixelSize = size.FontSizePointsToPixels();
                    break;
            }

            var fontTypeFace =
                _threadLocker.GetInterLocked(() =>
                    SKTypeface.FromFamilyName(name, fontStyle.ToSKFontStyle())
                );

            //var skFont = new SKFont(fontTypeFace, size: pixelSize);
            _skFont = new SKFont(fontTypeFace, size: pixelSize);

            // SKTypeface is not thread safe
            // https://groups.google.com/g/skia-discuss/c/-G1cyl1QD9E
            if (!_threadLockers.ContainsKey(fontTypeFace.Handle))
            {
                _threadLockers.TryAdd(fontTypeFace.Handle, new ThreadLocker());
            }
            _threadLockers.TryGetValue(fontTypeFace.Handle, out _locker);

            _skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill
            };

            this.Name = name;
            this.Size = size;
            this.Style = fontStyle;
            this.Unit = unit;

            _skFontPaint = new SkiaFontPaint(_skFont, _skPaint);

            //if (typefaceCharakter.HasValue)
            //{
            //    var fontManager = SKFontManager.Default;
            //    var typeFace = fontManager.MatchCharacter(name, typefaceCharakter.Value);
            //    if (typeFace != null)
            //    {
            //        _skPaint.Typeface = typeFace;
            //    }
            //}
        }

        public string Name { get; }

        public float Size { get; }

        public FontStyle Style { get; }

        public GraphicsUnit Unit { get; }

        public object EngineElement => _skFontPaint;

        public IThreadLocker LockObject => _locker;

        public void Dispose()
        {
            _skFont.Dispose();
            _skPaint.Dispose();
        }
    }

    class SkiaFontPaint
    {
        public SkiaFontPaint(SKFont font, SKPaint paint)
        {
            SKFont = font;
            SKPaint = paint;
        }
        public SKFont SKFont { get; }
        public SKPaint SKPaint { get; }
    }
}

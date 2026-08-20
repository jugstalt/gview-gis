#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Extensions;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace gView.Framework.Symbology
{
    /// <summary>
    /// Point marker symbol that renders SVG markup (from a file, or embedded as a
    /// map resource via the "resource:&lt;name&gt;" convention) instead of a
    /// TrueType glyph. This avoids depending on a font being installed on the
    /// rendering server. The SVG is rasterized once via the current graphics
    /// engine (<see cref="Current.Engine"/>) and cached on the symbol instance;
    /// the cache is only rebuilt when the requested pixel size changes materially,
    /// so panning/redrawing at (roughly) the same size stays cheap.
    /// </summary>
    [RegisterPlugIn("6FA05B74-F6CC-4A9F-AFFD-3C1E7E171A85")]
    public sealed class SvgMarkerSymbol : LegendItem, IPointSymbol, ISymbolRotation, IIconSymbol
    {
        private float _xOffset = 0, _yOffset = 0, _angle = 0, _rotation = 0, _hOffset = 0, _vOffset = 0;
        private float _sizeX = 10f, _sizeY = 10f;
        private string _filename = String.Empty;

        // gView.Server shares layers (and therefore renderers/symbols) by reference
        // across concurrently handled requests against the same map/service (see
        // ServiceMap.CreateAsync: "serviceMap._layers = original._layers"), and
        // RequireClone() => false means this exact instance - not a per-feature or
        // per-request clone - is what gets drawn from. So this cache genuinely is
        // read and (rarely) rebuilt from multiple threads at once, not just in
        // theory. _svgBytes is left as a plain nullable field: loading it twice due
        // to a race is just wasted I/O (idempotent, never corrupts anything). The
        // rasterized bitmap is different - swapping it while another thread is
        // mid-draw with the old one must never dispose that old bitmap out from
        // under it - see EnsureBitmap.
        private byte[]? _svgBytes = null;
        private volatile RasterCache? _cache = null;
        private readonly object _rasterizeLock = new object();

        private sealed class RasterCache
        {
            public readonly IBitmap Image;
            private readonly int _pixelWidth, _pixelHeight;

            public RasterCache(IBitmap image, int pixelWidth, int pixelHeight)
            {
                Image = image;
                _pixelWidth = pixelWidth;
                _pixelHeight = pixelHeight;
            }

            public bool MatchesWithinTolerance(int pixelWidth, int pixelHeight)
            {
                const float tolerance = 0.15f;

                return Math.Abs(pixelWidth - _pixelWidth) <= _pixelWidth * tolerance
                    && Math.Abs(pixelHeight - _pixelHeight) <= _pixelHeight * tolerance;
            }
        }

        [Browsable(true)]
        [PropertyDescription(EditorPropertyType = typeof(FileInfo), FileExtensions = ".svg")]
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                if (value != _filename)
                {
                    // No synchronous Dispose here either - see EnsureBitmap.
                    _cache = null;
                    _svgBytes = null;
                    _filename = value;
                }
            }
        }

        public float SizeX
        {
            get { return _sizeX; }
            set { _sizeX = value; }
        }

        public float SizeY
        {
            get { return _sizeY; }
            set { _sizeY = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        #region IPointSymbol Member

        public void DrawPoint(IDisplay display, IPoint point)
        {
            if (String.IsNullOrEmpty(_filename))
            {
                return;
            }

            float sizeX = _sizeX, sizeY = _sizeY;

            if (display.IsLegendItemSymbol())
            {
                sizeX = Math.Min(_sizeX, display.ImageWidth);
                sizeY = Math.Min(_sizeY, display.ImageHeight);
            }
            float x = _xOffset - sizeX / 2;
            float y = _yOffset - sizeY / 2;

            try
            {
                var transformRotation = _angle + _rotation;

                if (display.DisplayTransformation.UseDisplayRotation)
                {
                    transformRotation -= (float)display.DisplayTransformation.DisplayRotation;
                }

                display.Canvas.TranslateTransform(new CanvasPointF((float)point.X, (float)point.Y));
                display.Canvas.RotateTransform(transformRotation);

                var rect = new CanvasRectangle((int)x, (int)y, (int)sizeX, (int)sizeY);

                var previousInterpolationMode = display.Canvas.InterpolationMode;
                try
                {
                    // Read the cache reference exactly once - a concurrently running
                    // EnsureBitmap on another thread may swap `_cache` at any moment,
                    // but never mutates or disposes an already-published RasterCache,
                    // so this local snapshot is safe to draw from regardless of what
                    // happens on other threads afterwards.
                    var cache = EnsureBitmap(display, (int)Math.Max(1, sizeX), (int)Math.Max(1, sizeY));

                    if (cache != null)
                    {
                        // The bitmap is rasterized at (roughly) its on-screen size already,
                        // so a linear filter without mipmapping keeps rotated/blitted edges
                        // smooth without the extra blur mipmapping would add at this scale.
                        // This also avoids inheriting whatever interpolation mode a raster
                        // basemap layer left set on the shared canvas.
                        display.Canvas.InterpolationMode = GraphicsEngine.InterpolationMode.Low;

                        display.Canvas.DrawBitmap(
                                cache.Image,
                                rect,
                                new CanvasRectangle(0, 0, cache.Image.Width, cache.Image.Height));
                    }
                }
                catch
                {
                }
                finally
                {
                    display.Canvas.InterpolationMode = previousInterpolationMode;
                }
            }
            finally
            {
                display.Canvas.ResetTransform();
            }
        }

        #endregion IPointSymbol Member

        #region ISymbol Member

        public bool SupportsGeometryType(GeometryType geomType) => geomType == GeometryType.Point || geomType == GeometryType.Multipoint;

        public void Draw(IDisplay display, IGeometry geometry)
        {
            if (display != null && geometry is IPoint)
            {
                double x = ((IPoint)geometry).X;
                double y = ((IPoint)geometry).Y;
                display.World2Image(ref x, ref y);
                IPoint p = new gView.Framework.Geometry.Point(x, y);
                DrawPoint(display, p);
            }
            else if (geometry is IMultiPoint)
            {
                for (int i = 0, to = ((IMultiPoint)geometry).PointCount; i < to; i++)
                {
                    IPoint p = ((IMultiPoint)geometry)[i];
                    Draw(display, p);
                }
            }
        }

        public void Release()
        {
            // No synchronous Dispose here either - see EnsureBitmap.
            _cache = null;
        }

        [Browsable(false)]
        public string Name
        {
            get { return "Svg Marker Symbol"; }
        }

        #endregion ISymbol Member

        #region IClone2 Member

        public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return Clone();
            }

            float fac = ReferenceScaleHelper.CalcPixelUnitFactor(options);

            SvgMarkerSymbol marker = new SvgMarkerSymbol();
            marker.Angle = Angle;
            marker.HorizontalOffset = HorizontalOffset * fac;
            marker.VerticalOffset = VerticalOffset * fac;
            marker.SizeX = _sizeX * fac;
            marker.SizeY = _sizeY * fac;
            marker.Filename = _filename;
            marker.LegendLabel = _legendLabel;

            return marker;
        }

        #endregion IClone2 Member

        #region ISymbolTransformation Member

        [Browsable(false)]
        public float HorizontalOffset
        {
            get { return _hOffset; }
            set
            {
                _hOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float VerticalOffset
        {
            get { return _vOffset; }
            set
            {
                _vOffset = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        [Browsable(false)]
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                SymbolTransformation.Transform(_angle, _hOffset, _vOffset, out _xOffset, out _yOffset);
            }
        }

        #endregion ISymbolTransformation Member

        #region IPersistable Member

        new public void Load(IPersistStream stream)
        {
            base.Load(stream);

            Filename = (string)stream.Load("fn", String.Empty);
            HorizontalOffset = (float)stream.Load("x", 0f);
            VerticalOffset = (float)stream.Load("y", 0f);
            Angle = (float)stream.Load("a", 0f);
            SizeX = (float)stream.Load("sx", 10f);
            SizeY = (float)stream.Load("sy", 10f);
        }

        new public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("fn", Filename);
            stream.Save("x", HorizontalOffset);
            stream.Save("y", VerticalOffset);
            stream.Save("a", Angle);
            stream.Save("sx", _sizeX);
            stream.Save("sy", _sizeY);
        }

        #endregion IPersistable Member

        #region ISymbolRotation Member

        [Browsable(false)]
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        #endregion ISymbolRotation Member

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmoothingMode
        {
            get => SymbolSmoothing.None;
            set { }
        }

        public bool RequireClone()
        {
            return false;
        }

        #endregion ISymbol Member

        public void ReloadIfEmpty(IDisplay display, bool setSize)
        {
            if (_svgBytes == null)
            {
                LoadSvgBytes(display);
            }

            if (setSize && _svgBytes != null)
            {
                var size = TryReadIntrinsicSize(_svgBytes);
                if (size != null)
                {
                    SizeX = size.Value.Item1;
                    SizeY = size.Value.Item2;
                }
            }
        }

        /// <summary>
        /// Loads the raw SVG bytes (once) and (re-)rasterizes them only when there is
        /// no cached bitmap yet, or the requested pixel size has changed materially
        /// since the last rasterization (more than ~15%). This keeps the symbol crisp
        /// across zoom/reference-scale changes without re-rasterizing on every redraw,
        /// and is safe to call concurrently from multiple rendering threads (see the
        /// field comments on <see cref="_cache"/>/<see cref="_rasterizeLock"/>).
        /// </summary>
        private RasterCache? EnsureBitmap(IDisplay display, int pixelWidth, int pixelHeight)
        {
            // Fast path: no locking once warmed up - this runs once per drawn feature,
            // so it needs to stay cheap. Reading a reference field is already atomic;
            // RasterCache itself is immutable, so a cache someone else is concurrently
            // reading/replacing is always either fully the old one or fully the new one.
            var cache = _cache;
            if (cache != null && cache.MatchesWithinTolerance(pixelWidth, pixelHeight))
            {
                return cache;
            }

            lock (_rasterizeLock)
            {
                // Re-check: another thread may have already rebuilt the cache while
                // this one was waiting for the lock.
                cache = _cache;
                if (cache != null && cache.MatchesWithinTolerance(pixelWidth, pixelHeight))
                {
                    return cache;
                }

                if (_svgBytes == null)
                {
                    LoadSvgBytes(display);
                }

                var image = Current.Engine.RasterizeSvg(
                    _svgBytes!,
                    OversampledDimension(pixelWidth),
                    OversampledDimension(pixelHeight));

                // Deliberately not disposing the previous cache's bitmap: another
                // thread may still be mid-DrawBitmap with the reference it read from
                // `_cache` just before this assignment (see the class-level comment on
                // why that's a real scenario here, not just theoretical). SkiaSharp's
                // native wrapper releases the unmanaged pixel buffer via its own
                // finalizer once nothing references it anymore, trading a short delay
                // in native memory reclamation for zero risk of a use-after-dispose
                // crash on a concurrent reader.
                cache = new RasterCache(image, pixelWidth, pixelHeight);
                _cache = cache;

                return cache;
            }
        }

        // Marker symbols are commonly drawn quite small (10-30px), where rasterizing
        // 1:1 to the on-screen size leaves too few source pixels to rotate/downscale
        // smoothly - the result looks blobby rather than just "not antialiased".
        // Rasterizing at a higher resolution and letting the (linear, no-mipmap)
        // draw scale it down gives a crisp result at any rotation. The multiplier
        // matters most for small symbols, so it's capped for larger ones to avoid
        // pointlessly large bitmaps.
        private const int OversampleMinDimension = 64;
        private const float OversampleFactor = 2f;
        private const int OversampleMaxDimension = 512;

        private static int OversampledDimension(int targetPixels)
        {
            int oversampled = Math.Max(OversampleMinDimension, (int)Math.Round(targetPixels * OversampleFactor));
            return Math.Min(oversampled, Math.Max(targetPixels, OversampleMaxDimension));
        }

        private void LoadSvgBytes(IDisplay display)
        {
            if (_filename.StartsWith("resource:"))
            {
                var resourceData = display.Map.ResourceContainer[_filename.Substring(9)];
                if (resourceData == null)
                {
                    throw new Exception($"Can't find map resource {_filename.Substring(9)}");
                }
                _svgBytes = resourceData;
            }
            else
            {
                _svgBytes = File.ReadAllBytes(_filename);
            }
        }

        /// <summary>
        /// Best-effort read of an SVG's intrinsic size from its root element's
        /// width/height (numeric prefix, ignoring the unit) or, failing that, its
        /// viewBox - without needing a full SVG/graphics engine parse.
        /// </summary>
        private static (float, float)? TryReadIntrinsicSize(byte[] svgBytes)
        {
            try
            {
                using var ms = new MemoryStream(svgBytes);
                var root = XDocument.Load(ms).Root;
                if (root == null)
                {
                    return null;
                }

                if (TryParseLength(root.Attribute("width")?.Value, out var w) &&
                    TryParseLength(root.Attribute("height")?.Value, out var h))
                {
                    return (w, h);
                }

                var viewBox = root.Attribute("viewBox")?.Value;
                if (!String.IsNullOrEmpty(viewBox))
                {
                    var parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4 &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var vw) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var vh))
                    {
                        return (vw, vh);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool TryParseLength(string? value, out float result)
        {
            result = 0f;
            if (String.IsNullOrEmpty(value))
            {
                return false;
            }

            int i = 0;
            while (i < value.Length && (Char.IsDigit(value[i]) || value[i] == '.' || value[i] == '-' || value[i] == '+'))
            {
                i++;
            }

            return float.TryParse(value.Substring(0, i), NumberStyles.Float, CultureInfo.InvariantCulture, out result) && result > 0;
        }
    }
}

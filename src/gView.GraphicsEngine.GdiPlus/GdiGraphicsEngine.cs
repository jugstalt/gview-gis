using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace gView.GraphicsEngine.GdiPlus
{
    public class GdiGraphicsEngine : IGraphicsEngine
    {
        private static IThreadLocker _cloneObjectsLocker = new ThreadLocker();

        public GdiGraphicsEngine(float screenDpi)
        {
            ScreenDpi = screenDpi;
        }

        public string EngineName => "GdiPlus";

        public string EngineDisplayName => "GDI+ (GdiPlus)";

        public float ScreenDpi { get; }

        public bool MeasuresTextWithPadding => true;

        public IBitmap CreateBitmap(int width, int height)
        {
            return new GdiBitmap(width, height);
        }

        public IBitmap CreateBitmap(int width, int height, PixelFormat format)
        {
            return new GdiBitmap(width, height, format);
        }

        public IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            return new GdiBitmap(width, height, stride, format, scan0);
        }

        public IBitmap CreateBitmap(Stream stream)
        {
            return new GdiBitmap(stream);
        }

        public IBitmap CreateBitmap(string filename)
        {
            return new GdiBitmap(filename);
        }

        /// <summary>
        /// GDI+ has no SVG renderer available (Skia is the engine SVG markers are
        /// designed for; GDI+ is the legacy/opt-in fallback). Rather than fail the
        /// draw call - and with it the whole map render, since a thrown exception
        /// here would bubble out of the calling symbol's draw - this returns a
        /// generic placeholder marker instead. The map still renders; the affected
        /// symbols just don't show their actual SVG artwork under GDI+.
        /// </summary>
        public IBitmap RasterizeSvg(byte[] svgBytes, int pixelWidth, int pixelHeight)
        {
            pixelWidth = Math.Max(1, pixelWidth);
            pixelHeight = Math.Max(1, pixelHeight);

            var bitmap = CreateBitmap(pixelWidth, pixelHeight, PixelFormat.Rgba32);
            var gdiBitmap = (Bitmap)bitmap.EngineElement;

            using (var g = Graphics.FromImage(gdiBitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                var inset = Math.Max(1f, Math.Min(pixelWidth, pixelHeight) * 0.1f);
                var rect = new RectangleF(inset, inset, pixelWidth - 2 * inset, pixelHeight - 2 * inset);
                var penWidth = Math.Max(1f, Math.Min(pixelWidth, pixelHeight) / 12f);

                using (var fill = new SolidBrush(Color.FromArgb(160, 160, 160, 160)))
                using (var pen = new Pen(Color.FromArgb(220, 90, 90, 90), penWidth))
                {
                    g.FillEllipse(fill, rect);
                    g.DrawEllipse(pen, rect);
                }
            }

            return bitmap;
        }

        public IDrawTextFormat CreateDrawTextFormat()
        {
            return new DrawTextFormat();
        }

        public IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point)
        {
            return new GdiFont(fontFamily, size, fontStyle);
        }

        private static string[] _installedFontNames = null;
        public IEnumerable<string> GetInstalledFontNames()
        {
            if(_installedFontNames is null)
            {
                _installedFontNames = System.Drawing.FontFamily.Families.Select(f => f.Name)
                    .Concat(CustomFontFamilyNames())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            return _installedFontNames ?? Array.Empty<string>();
        }

        #region Font Provisioning

        // System.Drawing does not resolve PrivateFontCollection families through
        // "new Font(name, ...)" - a FontFamily from the collection has to be used
        // explicitly (see GdiFont). The collection is kept alive for the process
        // lifetime; its FontFamily instances must not be disposed while in use.
        //
        // Registration happens once at startup (before any request is served).
        // _familiesByName is the read index for the render hot path and is only
        // written under _registrationLock at startup, so lookups need no lock.
        private static readonly PrivateFontCollection _privateFonts = new PrivateFontCollection();
        private static readonly ConcurrentDictionary<string, FontFamily> _familiesByName
            = new ConcurrentDictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _loadedFontFiles
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _registrationLock = new object();

        public void RegisterFontDirectory(string path)
        {
            var files = FontProvisioning.EnumerateFontFiles(path).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            int loaded = 0;
            lock (_registrationLock)   // startup only - never contended by request threads
            {
                foreach (var file in files)
                {
                    if (!_loadedFontFiles.Add(file))
                    {
                        continue;
                    }

                    try
                    {
                        _privateFonts.AddFontFile(file);
                        loaded++;

                        Console.WriteLine($"[fonts] {EngineDisplayName}: registered '{file}'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[fonts] {EngineDisplayName}: error loading '{file}': {ex.Message}");
                    }
                }

                foreach (var family in _privateFonts.Families)
                {
                    _familiesByName[family.Name] = family;
                }

                // installed-font cache must be rebuilt so the new families show up
                _installedFontNames = null;
            }

            Console.WriteLine($"[fonts] {EngineDisplayName}: {loaded} font file(s) registered from '{path}', " +
                              $"{_familiesByName.Count} custom family/families total");
        }

        private static IEnumerable<string> CustomFontFamilyNames() => _familiesByName.Keys;

        /// <summary>
        /// Returns the registered (directory-provided) <see cref="FontFamily"/> for
        /// <paramref name="familyName"/>, or <c>null</c> when no such family was registered.
        /// Lock-free: <see cref="_familiesByName"/> is only written at startup.
        /// </summary>
        internal static FontFamily TryGetPrivateFontFamily(string familyName)
            => !String.IsNullOrEmpty(familyName) && _familiesByName.TryGetValue(familyName, out var family)
                ? family
                : null;

        #endregion

        private static string _defaultFontName = null;
        public string GetDefaultFontName()
        {
            if (String.IsNullOrEmpty(_defaultFontName))
            {
                _defaultFontName = System.Drawing.FontFamily.GenericSansSerif.Name;
            }

            return _defaultFontName;
        }

        public IGraphicsPath CreateGraphicsPath()
        {
            return new GdiGraphicsPath();
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new GdiPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new GdiSolidBrush(color);
        }

        public IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            return new GdiLinearGradientBrush(rect, col1, col2, angle);
        }

        public IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            return new BrushCollection(new IBrush[] { new GdiHatchBrush(hatchStyle, foreColor, backColor) });
        }

        public void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset)
        {
            // System.Drawing use unit "points" => do nothing
        }

        public IThreadLocker CloneObjectsLocker => _cloneObjectsLocker;
    }
}

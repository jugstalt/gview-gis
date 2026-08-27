using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Skia.Extensions;
using gView.GraphicsEngine.Threading;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.GraphicsEngine.Skia
{
    public class SkiaGraphicsEngine : IGraphicsEngine
    {
        public SkiaGraphicsEngine(float screenDpi)
        {
            ScreenDpi = screenDpi;
        }

        static public SKAlphaType AplphaType { get; set; } = SKAlphaType.Unpremul;  // Unpremul: default for PNG
                                                                                    // with jpg, transparency looks better with "Premul"                                                                 // Maybe, server sould switch Premul/UnPremul with depend of the output format in future?

        #region IGraphicsEngine

        public string EngineName => "SkiaSharp";

        public string EngineDisplayName => "SkiaSharp 3.x";

        public float ScreenDpi { get; }

        //
        // Skia measures text exact without a padding like GDI+
        // Some Funktions like BlockoutText and IncludesSuperScript requires MeasureText with padding
        //
        public bool MeasuresTextWithPadding => false;  

        #region Bitmap

        public IBitmap CreateBitmap(int width, int height)
        {
            return new SkiaBitmap(width, height);
        }

        public IBitmap CreateBitmap(int width, int height, PixelFormat format)
        {
            return new SkiaBitmap(width, height, format);
        }

        public IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            return new SkiaBitmap(width, height, stride, format, scan0);
        }

        public IBitmap CreateBitmap(Stream stream)
        {
            return new SkiaBitmap(stream);
        }

        public IBitmap CreateBitmap(string filename)
        {
            return new SkiaBitmap(filename);
        }

        public IBitmap RasterizeSvg(byte[] svgBytes, int pixelWidth, int pixelHeight)
        {
            if (svgBytes == null || svgBytes.Length == 0)
            {
                throw new ArgumentException("svgBytes must not be empty", nameof(svgBytes));
            }

            pixelWidth = Math.Max(1, pixelWidth);
            pixelHeight = Math.Max(1, pixelHeight);

            using var svg = new SKSvg();
            SKPicture picture;
            using (var svgStream = new MemoryStream(svgBytes))
            {
                picture = svg.Load(svgStream);
            }

            if (picture is null)
            {
                throw new InvalidOperationException("Could not parse SVG source.");
            }

            var bitmap = CreateBitmap(pixelWidth, pixelHeight, PixelFormat.Rgba32);
            var skBitmap = (SKBitmap)bitmap.EngineElement;

            var bounds = picture.CullRect;
            float boundsWidth = bounds.Width > 0 ? bounds.Width : pixelWidth;
            float boundsHeight = bounds.Height > 0 ? bounds.Height : pixelHeight;

            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Transparent);

                float sx = pixelWidth / boundsWidth;
                float sy = pixelHeight / boundsHeight;

                canvas.Translate(-bounds.Left * sx, -bounds.Top * sy);
                canvas.Scale(sx, sy);
                canvas.DrawPicture(picture);
                canvas.Flush();
            }

            return bitmap;
        }

        #endregion

        public IDrawTextFormat CreateDrawTextFormat()
        {
            return new SkiaDrawTextFormat();
        }

        public IFont CreateFont(string fontFamily, float size, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit grUnit = GraphicsUnit.Point)
        {
            return new SkiaFont(fontFamily, size, fontStyle, grUnit);
        }

        private static string[] _installedFontNames = null;
        public IEnumerable<string> GetInstalledFontNames()
        {
            if (_installedFontNames is null)
            {
                using (var fontManager = SKFontManager.Default)
                {
                    var systemFonts = fontManager.FontFamilies?.ToArray() ?? Array.Empty<string>();

                    _installedFontNames = systemFonts
                        .Concat(_customTypefaces.Keys)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                }
            }

            return _installedFontNames;
        }

        #region Font Provisioning

        private static readonly ConcurrentDictionary<string, List<SKTypeface>> _customTypefaces
            = new ConcurrentDictionary<string, List<SKTypeface>>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _loadedFontFiles
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _customFontsLock = new object();

        public void RegisterFontDirectory(string path)
        {
            var files = FontProvisioning.EnumerateFontFiles(path).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            int loaded = 0;
            lock (_customFontsLock)
            {
                foreach (var file in files)
                {
                    if (!_loadedFontFiles.Add(file))
                    {
                        continue;
                    }

                    try
                    {
                        var typeface = SKTypeface.FromFile(file);
                        if (typeface is null)
                        {
                            Console.WriteLine($"[fonts] {EngineDisplayName}: could not load '{file}'");
                            continue;
                        }

                        _customTypefaces
                            .GetOrAdd(typeface.FamilyName, _ => new List<SKTypeface>())
                            .Add(typeface);
                        loaded++;

                        Console.WriteLine($"[fonts] {EngineDisplayName}: registered '{typeface.FamilyName}' " +
                                          $"(weight {typeface.FontStyle.Weight}, slant {typeface.FontStyle.Slant}) " +
                                          $"from {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[fonts] {EngineDisplayName}: error loading '{file}': {ex.Message}");
                    }
                }

                // installed-font caches must be rebuilt so the new families show up
                _installedFontNames = null;
                _defaultFontName = null;
            }

            Console.WriteLine($"[fonts] {EngineDisplayName}: {loaded} font file(s) registered from '{path}', " +
                              $"{_customTypefaces.Count} custom family/families total");
        }

        /// <summary>
        /// Returns the registered (directory-provided) typeface that best matches the
        /// requested family name and style, or <c>null</c> when no such family was registered.
        /// </summary>
        internal static SKTypeface TryResolveCustomTypeface(string familyName, FontStyle fontStyle)
        {
            if (String.IsNullOrEmpty(familyName) ||
                !_customTypefaces.TryGetValue(familyName, out var candidates) ||
                candidates.Count == 0)
            {
                return null;
            }

            var want = fontStyle.ToSKFontStyle();

            SKTypeface best = null;
            int bestScore = int.MaxValue;

            lock (_customFontsLock)
            {
                foreach (var typeface in candidates)
                {
                    var have = typeface.FontStyle;
                    int score = Math.Abs(have.Weight - want.Weight)
                              + Math.Abs(have.Width - want.Width) * 10
                              + (have.Slant == want.Slant ? 0 : 1000);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = typeface;
                    }
                }
            }

            return best;
        }

        #endregion

        private static readonly string[] CommonFonts = new string[]
        {
            "Times New Roman", "Verdana", "Tahoma", "Arial", "Helvetica",
            "Georgia", "Trebuchet MS", "Lucida Sans","Courier New"
        };
        private static string _defaultFontName = null;
        public string GetDefaultFontName()
        {
            if (_defaultFontName is null)
            {
                var availableFonts = GetInstalledFontNames();

                foreach (var font in CommonFonts)
                {
                    if (availableFonts.Any(f => f.Equals(font, StringComparison.OrdinalIgnoreCase)))
                    {
                        return _defaultFontName = font;
                    }
                }

                // Fallback
                return _defaultFontName = "Sans-serif";
            }

            return _defaultFontName;
        }

        public IGraphicsPath CreateGraphicsPath()
        {
            return new SkiaGraphicsPath();
        }

        public IBrushCollection CreateHatchBrush(HatchStyle hatchStyle, ArgbColor foreColor, ArgbColor backColor)
        {
            return SkiaHatchBrush.CreateCollection(hatchStyle, foreColor, backColor);
        }

        public IBrush CreateLinearGradientBrush(CanvasRectangleF rect, ArgbColor col1, ArgbColor col2, float angle)
        {
            return new SkiaLinearGradientBrush(rect, col1, col2, angle);
        }

        public IPen CreatePen(ArgbColor color, float width)
        {
            return new SkiaPen(color, width);
        }

        public IBrush CreateSolidBrush(ArgbColor color)
        {
            return new SkiaSolidBrush(color);
        }

        public void DrawTextOffestPointsToFontUnit(ref CanvasPointF offset)
        {
            //offset.X = offset.X.PointsToPixels() / 1.2f;
            //offset.Y = offset.Y.PointsToPixels() / 1.2f;
        }

        public IThreadLocker CloneObjectsLocker => null;
        
        #endregion
    }
}

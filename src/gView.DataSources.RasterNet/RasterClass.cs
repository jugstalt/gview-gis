using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Extensions;
using gView.GraphicsEngine;
using RasterNet;
using RasterNet.Rendering;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RasterEnvelope = RasterNet.Envelope;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// One raster file read with RasterNet.Core. The raster is opened once and shared by all requests:
    /// RasterNet rasters are immutable after opening and can be read from any number of threads.
    /// Rendering, identify and the grid settings behave like the GDAL raster class, so maps look the same.
    /// </summary>
    public class RasterClass : IRasterClass, IRasterFile, IPointIdentify, IGridIdentify, IGridClass, IPersistable, IDisposable
    {
        private static readonly ColorClass[] AllWhite =
        {
            new ColorClass(double.NegativeInfinity, double.PositiveInfinity, new RgbaColor(255, 255, 255)),
        };

        /// <summary>
        /// Threads one map request may use for decoding. JPEG2000 code-blocks and GeoTIFF blocks are decoded in
        /// parallel; half of the cores leaves room for the other requests of the server.
        /// </summary>
        private static readonly ReadOptions RenderReadOptions = new()
        {
            MaxDegreeOfParallelism = Math.Clamp(Environment.ProcessorCount / 2, 1, 8),
        };

        private readonly Raster _raster;
        private readonly IRasterDataset _dataset;
        private readonly string _filename;
        private readonly string _title;
        private readonly IPolygon _polygon;
        private readonly RasterWorldFile _worldFile;
        private readonly bool _isGrid;

        // Grid settings with the defaults of the GDAL raster class.
        private GridColorClass[] _colorClasses;
        private bool _useHillShade = true;
        private readonly double[] _hillShade = { 0.0, 1.0, 1.0 };
        private double _ignoreValue;
        private bool _useIgnoreValue;
        private bool _renderRawGridValues;

        public RasterClass(IRasterDataset dataset, string filename, IPolygon polygon)
        {
            var file = new FileInfo(filename);
            _filename = file.FullName;
            _title = file.Name;
            _dataset = dataset;
            _raster = Raster.Open(_filename);

            try
            {
                var band = _raster.Bands[0];
                _isGrid = _raster.Format == RasterFormat.ArcInfoBinaryGrid ||
                          (_raster.Bands.Count == 1 && band.DataType != RasterDataType.UInt8);
                _worldFile = new RasterWorldFile(_raster.GeoTransform, FindWorldFile(file));
                SpatialReference = file.FileSpatialReference() ?? ToSpatialReference(_raster.Crs);
                _polygon = polygon ?? CreatePolygon();
            }
            catch
            {
                _raster.Dispose();
                throw;
            }
        }

        #region IRasterClass

        public IPolygon Polygon => _polygon;

        // gView describes the georeferencing like a world file: the origin is the center of the top-left cell.
        public double oX => _worldFile.X;
        public double oY => _worldFile.Y;
        public double dx1 => _worldFile.dx_X;
        public double dx2 => _worldFile.dx_Y;
        public double dy1 => _worldFile.dy_X;
        public double dy2 => _worldFile.dy_Y;

        public ISpatialReference SpatialReference { get; set; }

        public Task<IRasterPaintContext> BeginPaint(IDisplay display, ICancelTracker cancelTracker)
        {
            if (CancelTracker.Canceled(cancelTracker))
            {
                return Task.FromResult<IRasterPaintContext>(null);
            }

            IEnvelope bounds = display.DisplayTransformation.RotatedBounds();
            if (display.GeometricTransformer != null)
            {
                bounds = ((IGeometry)display.GeometricTransformer.InvTransform2D(bounds))?.Envelope;
            }
            if (bounds == null || !(bounds.MaxX > bounds.MinX) || !(bounds.MaxY > bounds.MinY))
            {
                return Task.FromResult<IRasterPaintContext>(null);
            }

            double cellSize = display.MapScale / (display.Dpi / 0.0254); // meters per screen pixel
            var plan = RasterRenderer.Plan(_raster, new RasterEnvelope(bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY), cellSize);
            if (plan.IsEmpty)
            {
                return Task.FromResult<IRasterPaintContext>(null);
            }

            var bitmap = Current.Engine.CreateBitmap(plan.Width, plan.Height, PixelFormat.Rgba32);
            try
            {
                var data = bitmap.LockBitmapPixelData(BitmapLockMode.WriteOnly, PixelFormat.Rgba32);
                try
                {
                    Render(plan, data.Scan0, data.Stride);
                }
                finally
                {
                    bitmap.UnlockBitmapPixelData(data);
                }

                return Task.FromResult<IRasterPaintContext>(new RasterPaintContext2(bitmap)
                {
                    PicPoint1 = new Point(plan.TopLeft.X, plan.TopLeft.Y),
                    PicPoint2 = new Point(plan.TopRight.X, plan.TopRight.Y),
                    PicPoint3 = new Point(plan.BottomLeft.X, plan.BottomLeft.Y),
                });
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        #endregion

        #region Rendering

        private unsafe void Render(RenderPlan plan, IntPtr scan0, int stride)
        {
            var pixels = new Span<byte>((void*)scan0, stride * plan.Height);

            if (!_isGrid)
            {
                RasterRenderer.Render(plan, pixels, stride, new RenderOptions { ReadOptions = RenderReadOptions });
            }
            else if (_renderRawGridValues)
            {
                RenderRawValues(plan, pixels, stride);
            }
            else
            {
                RasterRenderer.Render(plan, pixels, stride, new RenderOptions
                {
                    GridRenderer = new ColorClassRenderer(ToColorClasses(_colorClasses)),
                    Hillshade = _useHillShade && HasDirection(_hillShade) ? new Hillshade(_hillShade[0], _hillShade[1], _hillShade[2]) : null,
                    IgnoreValue = _useIgnoreValue ? _ignoreValue : null,
                    ReadOptions = RenderReadOptions,
                });
            }
        }

        /// <summary>
        /// The values times 100 as 24-bit sign-magnitude integers in B, G, R (alpha 255), as the GDAL raster class
        /// encodes them for clients that read the values from the image; NoData is transparent.
        /// </summary>
        private void RenderRawValues(RenderPlan plan, Span<byte> pixels, int stride)
        {
            var values = ArrayPool<float>.Shared.Rent(plan.Width * plan.Height);
            try
            {
                RasterRenderer.RenderValues(plan, 0, values, new RenderOptions { ReadOptions = RenderReadOptions });
                for (int y = 0; y < plan.Height; y++)
                {
                    var row = pixels.Slice(y * stride, plan.Width * 4);
                    for (int x = 0; x < plan.Width; x++)
                    {
                        float value = values[y * plan.Width + x];
                        var pixel = row.Slice(4 * x, 4);
                        if (float.IsNaN(value))
                        {
                            pixel.Clear();
                            continue;
                        }

                        long scaled = (long)(value * 100f);
                        long magnitude = Math.Abs(scaled) & 0x7FFFFF;
                        pixel[0] = (byte)magnitude;
                        pixel[1] = (byte)(magnitude >> 8);
                        pixel[2] = (byte)((magnitude >> 16) + (scaled < 0 ? 0x80 : 0));
                        pixel[3] = 255;
                    }
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(values);
            }
        }

        /// <summary>gView's color classes; without classes every valid value is white (as in the GDAL raster class).</summary>
        private static ColorClass[] ToColorClasses(GridColorClass[] classes)
        {
            if (classes == null)
            {
                return AllWhite;
            }

            var result = new ColorClass[classes.Length];
            for (int i = 0; i < classes.Length; i++)
            {
                var color = classes[i].Color;
                result[i] = new ColorClass(classes[i].MinValue, classes[i].MaxValue, new RgbaColor(color.R, color.G, color.B, color.A));
            }
            return result;
        }

        private static bool HasDirection(double[] vector) =>
            vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2] > 0;

        #endregion

        #region IClass

        public string Name => _title;

        public string Aliasname => _title;

        public IDataset Dataset => _dataset;

        #endregion

        #region IRasterFile

        public string Filename => _filename;

        public IRasterWorldFile WorldFile => _worldFile;

        #endregion

        #region IPointIdentify

        public IPointIdentifyContext CreatePointIdentifyContext()
        {
            return new DummyPointIdentifyContext();
        }

        public Task<ICursor> PointQuery(IDisplay display, IPoint point, ISpatialReference sRef, IUserData userdata, IPointIdentifyContext context)
        {
            if (!TryGetCell(display, point, sRef, out int column, out int row))
            {
                return Task.FromResult<ICursor>(null);
            }

            var fields = new List<(string Name, object Value)>
            {
                ("ImageX", column),
                ("ImageY", row),
            };

            if (_isGrid)
            {
                float value = ReadCell<float>(0, column, row);
                if (IsNoData(value))
                {
                    return Task.FromResult<ICursor>(null);
                }
                fields.Add(("band1", value));
                return Task.FromResult<ICursor>(new QueryCursor(fields));
            }

            for (int i = 0; i < _raster.Bands.Count; i++)
            {
                var band = _raster.Bands[i];
                int value = ReadCell<int>(i, column, row);
                fields.Add(($"Band {i + 1} {BandName(band.ColorInterpretation)}".TrimEnd(), value));
                if (band.ColorTable is { } table && value >= 0 && value < table.Count)
                {
                    var color = table[value];
                    fields.Add(("Alpha", color.A));
                    fields.Add(("Red", color.R));
                    fields.Add(("Green", color.G));
                    fields.Add(("Blue", color.B));
                }
            }
            return Task.FromResult<ICursor>(new QueryCursor(fields));
        }

        private static string BandName(ColorInterpretation color) => color switch
        {
            ColorInterpretation.Red => "(red)",
            ColorInterpretation.Green => "(green)",
            ColorInterpretation.Blue => "(blue)",
            ColorInterpretation.Alpha => "(alpha)",
            _ => "",
        };

        #endregion

        #region IGridIdentify

        public void InitGridQuery()
        {
        }

        public float GridQuery(IDisplay display, IPoint point, ISpatialReference sRef)
        {
            float noData = (float)(_raster.Bands[0].NoData ?? 0);
            if (!TryGetCell(display, point, sRef, out int column, out int row))
            {
                return noData;
            }

            float value = ReadCell<float>(0, column, row);
            return IsNoData(value) ? noData : value;
        }

        public void ReleaseGridQuery()
        {
        }

        private bool TryGetCell(IDisplay display, IPoint point, ISpatialReference sRef, out int column, out int row)
        {
            column = row = -1;
            if (SpatialReference != null && sRef != null && !sRef.Equals(SpatialReference))
            {
                point = GeometricTransformerFactory.Transform2D(point, sRef, SpatialReference, display?.DatumTransformations) as IPoint;
            }
            if (point == null || !_raster.GeoTransform.IsInvertible)
            {
                return false;
            }

            var (x, y) = _raster.GeoTransform.WorldToPixel(point.X, point.Y);
            if (!(x >= 0 && x < _raster.Width && y >= 0 && y < _raster.Height))
            {
                return false;
            }
            column = (int)Math.Floor(x);
            row = (int)Math.Floor(y);
            return true;
        }

        private T ReadCell<T>(int band, int column, int row) where T : unmanaged
        {
            Span<T> value = stackalloc T[1];
            _raster.Read<T>(band, 0, new Window(column, row, 1, 1), value);
            return value[0];
        }

        private bool IsNoData(float value)
        {
            var noData = _raster.Bands[0].NoData;
            return float.IsNaN(value) ||
                   (noData.HasValue && value == (float)noData.Value) ||
                   (_useIgnoreValue && value == (float)_ignoreValue);
        }

        #endregion

        #region IGridClass

        public GridRenderMethod ImplementsRenderMethods => _isGrid
            ? GridRenderMethod.Colors | GridRenderMethod.HillShade | GridRenderMethod.NullValue
            : GridRenderMethod.None;

        public GridColorClass[] ColorClasses
        {
            get => _colorClasses == null ? null : (GridColorClass[])_colorClasses.Clone();
            set => _colorClasses = value == null ? null : (GridColorClass[])value.Clone();
        }

        public bool UseHillShade
        {
            get => _useHillShade;
            set => _useHillShade = value;
        }

        public double[] HillShadeVector
        {
            get => (double[])_hillShade.Clone();
            set
            {
                if (value != null && value.Length == 3)
                {
                    value.CopyTo(_hillShade, 0);
                }
            }
        }

        // RasterNet does not compute statistics; 0 like the GDAL raster class for rasters without statistics.
        public double MinValue => 0;

        public double MaxValue => 0;

        public double IgnoreDataValue
        {
            get => _ignoreValue;
            set => _ignoreValue = value;
        }

        public bool UseIgnoreDataValue
        {
            get => _useIgnoreValue;
            set => _useIgnoreValue = value;
        }

        public bool RenderRawGridValues
        {
            get => _renderRawGridValues;
            set => _renderRawGridValues = value;
        }

        #endregion

        #region IPersistable (same keys as the GDAL raster class)

        public void Load(IPersistStream stream)
        {
            if (!_isGrid)
            {
                return;
            }

            var classes = new List<GridColorClass>();
            GridColorClass colorClass;
            while ((colorClass = (GridColorClass)stream.Load("GridClass", null, new GridColorClass(0, 0, ArgbColor.White))) != null)
            {
                classes.Add(colorClass);
            }
            _colorClasses = classes.Count > 0 ? classes.ToArray() : null;

            _useHillShade = (bool)stream.Load("UseHillShade", true);
            _hillShade[0] = (double)stream.Load("HillShadeDx", 0.0);
            _hillShade[1] = (double)stream.Load("HillShadeDy", 0.0);
            _hillShade[2] = (double)stream.Load("HillShadeDz", 0.0);
            _useIgnoreValue = (bool)stream.Load("UseIgnoreData", false);
            _ignoreValue = (double)stream.Load("IgnoreData", 0.0);
            _renderRawGridValues = (bool)stream.Load("RenderRawGridValues", false);
        }

        public void Save(IPersistStream stream)
        {
            if (!_isGrid)
            {
                return;
            }

            if (_colorClasses != null)
            {
                foreach (var colorClass in _colorClasses)
                {
                    stream.Save("GridClass", colorClass);
                }
            }
            stream.Save("UseHillShade", _useHillShade);
            stream.Save("HillShadeDx", _hillShade[0]);
            stream.Save("HillShadeDy", _hillShade[1]);
            stream.Save("HillShadeDz", _hillShade[2]);
            stream.Save("UseIgnoreData", _useIgnoreValue);
            stream.Save("IgnoreData", _ignoreValue);
            stream.Save("RenderRawGridValues", _renderRawGridValues);
        }

        #endregion

        public void Dispose()
        {
            _raster.Dispose();
        }

        #region Helper

        private IPolygon CreatePolygon()
        {
            var transform = _raster.GeoTransform;
            var ring = new Ring();
            foreach (var (column, row) in new[] { (0, 0), (_raster.Width, 0), (_raster.Width, _raster.Height), (0, _raster.Height) })
            {
                var (x, y) = transform.PixelToWorld(column, row);
                ring.AddPoint(new Point(x, y));
            }

            var polygon = new Polygon();
            polygon.AddRing(ring);
            return polygon;
        }

        /// <summary>The coordinate system of the raster when there is no .prj, .wkt or .epsg file next to it.</summary>
        private static ISpatialReference ToSpatialReference(CrsInfo crs)
        {
            if (crs?.EpsgCode is int code)
            {
                return gView.Framework.Geometry.SpatialReference.FromID($"EPSG:{code}");
            }
            if (!string.IsNullOrWhiteSpace(crs?.Proj4))
            {
                return new gView.Framework.Geometry.SpatialReference("custom", crs.Proj4, crs.Proj4, null);
            }
            return null;
        }

        private static string FindWorldFile(FileInfo file)
        {
            string extension = file.Extension.ToLowerInvariant();
            if (extension.Length < 3)
            {
                return null;
            }
            var path = System.IO.Path.ChangeExtension(file.FullName, $"{extension[1]}{extension[^1]}w");
            return File.Exists(path) ? path : null;
        }

        #endregion
    }
}

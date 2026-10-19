using gView.Framework.Core.Data;
using System;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// The georeferencing of a RasterNet raster in gView's world file terms: X/Y is the center of the top-left cell,
    /// dx_X/dx_Y the step per column, dy_X/dy_Y the step per row.
    /// </summary>
    internal sealed class RasterWorldFile : IRasterWorldFile
    {
        public RasterWorldFile(global::RasterNet.GeoTransform transform, string filename)
        {
            Filename = filename ?? string.Empty;
            dx_X = transform.XPerColumn;
            dx_Y = transform.YPerColumn;
            dy_X = transform.XPerRow;
            dy_Y = transform.YPerRow;
            X = transform.X0 + dx_X / 2.0 + dy_X / 2.0;
            Y = transform.Y0 + dx_Y / 2.0 + dy_Y / 2.0;

            // A raster without georeferencing has the pixel space transformation (0, 1, 0, 0, 0, 1).
            isGeoReferenced = !(transform.X0 == 0 && transform.Y0 == 0 && transform.XPerColumn == 1 &&
                                transform.XPerRow == 0 && transform.YPerColumn == 0 && transform.YPerRow == 1);
        }

        public string Filename { get; }

        public bool isGeoReferenced { get; }

        public double dx_X { get; }

        public double dx_Y { get; }

        public double dy_X { get; }

        public double dy_Y { get; }

        public double cellX => Math.Sqrt(dx_X * dx_X + dx_Y * dx_Y);

        public double cellY => Math.Sqrt(dy_X * dy_X + dy_Y * dy_Y);

        public double X { get; }

        public double Y { get; }
    }
}

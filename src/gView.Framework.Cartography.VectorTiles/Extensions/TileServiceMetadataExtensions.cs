using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Metadata;
using SpatialNET.IO;

namespace gView.Framework.Cartography.VectorTiles.Extensions;

public static class TileServiceMetadataExtensions
{
    extension(TileServiceMetadata metadata)
    {
        /// <summary>
        /// The SpatialNET tile grid of the metadata for one EPSG code and origin. Resolutions are in the
        /// units of the CRS (degrees for geographic systems), as the raster tile cache computes them.
        /// </summary>
        public TileGrid CreateTileGrid(int epsg, GridOrientation orientation, ISpatialReference? sRef = null)
        {
            sRef ??= SpatialReference.FromID($"epsg:{epsg}")
                ?? throw new Exception($"Can't load spatial reference EPSG:{epsg}");

            var origin = (orientation == GridOrientation.UpperLeft
                ? metadata.GetOriginUpperLeft(epsg)
                : metadata.GetOriginLowerLeft(epsg))
                ?? throw new Exception($"No origin defined for EPSG:{epsg}");

            var converter = sRef.SpatialParameters.IsGeographic ? new GeoUnitConverter() : null;

            var resolutions = metadata.Scales.InnerList.Select(scale =>
            {
                double res = scale / (metadata.Dpi / 0.0254);
                return converter != null ? converter.Convert(res, GeoUnits.Meters, GeoUnits.DecimalDegrees) : res;
            });

            return new TileGrid(origin.X, origin.Y, resolutions,
                metadata.TileWidth, metadata.TileHeight,
                orientation == GridOrientation.UpperLeft ? TileOrigin.UpperLeft : TileOrigin.LowerLeft);
        }

        /// <summary>
        /// The scale index (level) that is the standard WebMercator zoom level <paramref name="zoom"/>
        /// (EPSG:3857, upper-left origin at -20037508.34/20037508.34), or -1. Only such tiles are Mapbox conform.
        /// </summary>
        public int WebMercatorLevel(int zoom)
        {
            for (int level = 0; level < metadata.Scales.InnerList.Count; level++)
            {
                if (metadata.WebMercatorZoom(level) == zoom)
                {
                    return level;
                }
            }

            return -1;
        }

        /// <summary>
        /// The standard WebMercator zoom level of the scale index <paramref name="level"/>, or -1 if the
        /// level (or the EPSG:3857 grid of the metadata) is not Mapbox conform.
        /// </summary>
        public int WebMercatorZoom(int level)
        {
            if (!metadata.EPSGCodes.Contains(3857)
                || level < 0 || level >= metadata.Scales.InnerList.Count
                || metadata.TileWidth != metadata.TileHeight || metadata.TileWidth <= 0)
            {
                return -1;
            }

            var origin = metadata.GetOriginUpperLeft(3857);
            if (origin == null
                || Math.Abs(origin.X + TileGrid.WebMercatorHalfExtent) > 1e-2
                || Math.Abs(origin.Y - TileGrid.WebMercatorHalfExtent) > 1e-2)
            {
                return -1;
            }

            // z/x/y only depends on the tile's ground size: a 512px tile at resolution r is the 256px tile at 2r
            double resolution = metadata.Scales.InnerList[level] / (metadata.Dpi / 0.0254) * (metadata.TileWidth / 256.0);

            return TileGrid.TryGetWebMercatorZoomForResolution(resolution, out int z, relativeTolerance: 1e-4) ? z : -1;
        }

        /// <summary>
        /// True if every scale of the metadata is a standard WebMercator zoom level: the cache can be served as z/x/y.
        /// </summary>
        public bool IsWebMercatorConform()
            => metadata.Scales.InnerList.Count > 0
               && Enumerable.Range(0, metadata.Scales.InnerList.Count).All(level => metadata.WebMercatorZoom(level) >= 0);
    }
}

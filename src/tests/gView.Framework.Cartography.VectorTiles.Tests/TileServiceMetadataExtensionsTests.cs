using gView.Framework.Cartography.VectorTiles.Extensions;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Metadata;
using SpatialNET.IO;

namespace gView.Framework.Cartography.VectorTiles.Tests;

public class TileServiceMetadataExtensionsTests
{
    private const double HalfExtent = 20037508.342789244;

    private static TileServiceMetadata WebMercatorMetadata(double originX = -HalfExtent, int tileSize = 256, params int[] zooms)
    {
        var metadata = new TileServiceMetadata { TileWidth = tileSize, TileHeight = tileSize };
        metadata.EPSGCodes.Clear();
        metadata.EPSGCodes.Add(3857);
        metadata.SetOriginUpperLeft(3857, new Point(originX, HalfExtent));

        metadata.Scales.InnerList.Clear();
        foreach (int z in zooms)
        {
            // scale of the 256px zoom level z at the metadata dpi, as the tile cache stores it (rounded)
            double resolution = TileGrid.WebMercatorResolution(z) * 256.0 / tileSize;
            metadata.Scales.InnerList.Add(Math.Round(resolution * metadata.Dpi / 0.0254, 3));
        }

        return metadata;
    }

    [Fact]
    public void WebMercatorLevel_StandardScales_ReturnsScaleIndexOfZoom()
    {
        var metadata = WebMercatorMetadata(zooms: [10, 12, 14]);

        Assert.Equal(0, metadata.WebMercatorLevel(10));
        Assert.Equal(1, metadata.WebMercatorLevel(12));
        Assert.Equal(2, metadata.WebMercatorLevel(14));
        Assert.Equal(-1, metadata.WebMercatorLevel(11));
    }

    [Fact]
    public void WebMercatorLevel_512pxTiles_ReturnsZoomOfTheTileGroundSize()
    {
        var metadata = WebMercatorMetadata(tileSize: 512, zooms: [5]);

        Assert.Equal(0, metadata.WebMercatorLevel(5));
    }

    [Fact]
    public void WebMercatorLevel_OtherOrigin_IsNotMapboxConform()
    {
        var metadata = WebMercatorMetadata(originX: -20000000, zooms: [10]);

        Assert.Equal(-1, metadata.WebMercatorLevel(10));
    }

    [Theory]
    [InlineData(GridOrientation.UpperLeft)]
    [InlineData(GridOrientation.LowerLeft)]
    public void CreateTileGrid_CustomProjection_MatchesRasterTileEnvelope(GridOrientation orientation)
    {
        var metadata = new TileServiceMetadata { TileWidth = 256, TileHeight = 256 };
        metadata.EPSGCodes.Clear();
        metadata.EPSGCodes.Add(31256);
        metadata.SetOriginUpperLeft(31256, new Point(-115309.0, 5.5e6));
        metadata.SetOriginLowerLeft(31256, new Point(-115309.0, 4.5e6));
        metadata.Scales.InnerList.Clear();
        metadata.Scales.InnerList.AddRange([50000.0, 10000.0, 1000.0]);

        var grid = metadata.CreateTileGrid(31256, orientation, new SpatialReference("epsg:31256"));
        var envelope = grid.TileEnvelope(level: 1, row: 3, column: 7);

        // the formula of the raster tile cache (TileServiceInterpreter.GetTile)
        double res = 10000.0 / (metadata.Dpi / 0.0254);
        double w = 256 * res, h = 256 * res;
        var origin = orientation == GridOrientation.UpperLeft ? metadata.GetOriginUpperLeft(31256) : metadata.GetOriginLowerLeft(31256);
        double minY = orientation == GridOrientation.UpperLeft ? origin.Y - h * 4 : origin.Y + h * 3;
        double minX = origin.X + w * 7;

        Assert.Equal(minX, envelope.MinX, 6);
        Assert.Equal(minY, envelope.MinY, 6);
        Assert.Equal(minX + w, envelope.MaxX, 6);
        Assert.Equal(minY + h, envelope.MaxY, 6);
    }
}

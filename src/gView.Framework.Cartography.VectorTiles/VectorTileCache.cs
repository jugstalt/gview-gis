using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Metadata;

namespace gView.Framework.Cartography.VectorTiles;

/// <summary>
/// Reading, rendering and storing vector tiles (<c>.pbf</c>, gzipped) in the tile cache directory, in the
/// same layouts as the raster tiles: exploded (<c>_alllayers/{ul|ll}/{epsg}/{scale}/{row}/{col}.pbf</c>)
/// and compact (<c>_alllayers/compact/ul/{epsg}/{scale}/R..C...pbfbundle</c>, 128 x 128 tiles per bundle).
/// </summary>
/// <remarks>
/// An empty tile is stored as a zero-length file (exploded) or a missing index entry (compact),
/// so it is not rendered again. Readers return an empty array for it.
/// </remarks>
public sealed class VectorTileCache
{
    public const string Extension = ".pbf";
    private const int BundleSize = 128;

    private readonly string _tileCachePath;
    private readonly string _mapName;
    private readonly TileServiceMetadata _metadata;
    private readonly Action<string>? _logWarning;

    public VectorTileCache(string tileCachePath, string mapName, TileServiceMetadata metadata, Action<string>? logWarning = null)
    {
        _tileCachePath = tileCachePath;
        _mapName = mapName;
        _metadata = metadata;
        _logWarning = logWarning;
    }

    #region Exploded

    public string ExplodedTilePath(GridOrientation orientation, int epsg, double scale, int row, int col)
        => Path.Combine(_tileCachePath, _mapName, "_alllayers",
                        TileServiceMetadata.TilePath(orientation, epsg, scale, row, col) + Extension);

    /// <summary>
    /// The cached tile, or <c>null</c> if it is not cached. An empty array is an empty tile.
    /// </summary>
    public Task<byte[]?> ReadExplodedAsync(GridOrientation orientation, int epsg, double scale, int row, int col)
        // a file locked by a concurrent writer counts as not cached: the caller renders (or waits for that rendering)
        => TileCacheFile.TryReadAsync(ExplodedTilePath(orientation, epsg, scale, row, col));

    /// <summary>
    /// Renders the tile and (if <paramref name="writeToCache"/>) stores it as exploded tile.
    /// Returns the gzipped tile or an empty array for an empty tile.
    /// </summary>
    /// <remarks>
    /// Concurrent requests of the same tile (clients often ask several times at once) render it once in this
    /// process and share the result. A failed cache write is only logged, the tile is returned anyway.
    /// </remarks>
    public Task<byte[]> RenderExplodedAsync(IServiceMap map, GridOrientation orientation, int epsg, double scale, int row, int col,
                                            bool writeToCache, ICancelTracker? cancelTracker = null)
    {
        string path = ExplodedTilePath(orientation, epsg, scale, row, col);

        return TileCacheFile.GetOrCreateAsync(path, async () =>
        {
            var data = await RenderAsync(map, orientation, epsg, scale, row, col, cancelTracker);
            if (writeToCache)
            {
                await TileCacheFile.TryWriteAsync(path, data, _logWarning);
            }
            return data;
        });
    }

    #endregion

    #region Compact

    public string CompactScalePath(int epsg, double scale)
        => Path.Combine(_tileCachePath, _mapName, "_alllayers", "compact",
                        TileServiceMetadata.ScalePath(GridOrientation.UpperLeft, epsg, scale));

    public static int CompactTileStart(int index) => index < 0 ? 0 : (index >> 7) * BundleSize;

    public static string CompactBundleName(int row, int col)
        => "R" + CompactTileStart(row).ToString("X8") + "C" + CompactTileStart(col).ToString("X8");

    private string BundleFile(int epsg, double scale, int row, int col)
        => Path.Combine(CompactScalePath(epsg, scale), CompactBundleName(row, col) + ".pbfbundle");

    private static string IndexFile(string bundleFile) => Path.ChangeExtension(bundleFile, ".pbfbundlx");

    public bool CompactBundleExists(int epsg, double scale, int row, int col)
        => File.Exists(BundleFile(epsg, scale, row, col)) || File.Exists(BundleFile(epsg, scale, row, col) + ".done");

    /// <summary>
    /// The tile from its compact bundle, or <c>null</c> if the bundle does not exist. An empty array is an empty tile.
    /// </summary>
    public async Task<byte[]?> ReadCompactAsync(int epsg, double scale, int row, int col)
    {
        string bundleFile = BundleFile(epsg, scale, row, col);
        if (File.Exists(bundleFile + ".done"))
        {
            return [];  // rendered, no tile has data
        }

        string indexFile = IndexFile(bundleFile);
        if (!File.Exists(bundleFile) || !File.Exists(indexFile))
        {
            return null;
        }

        int r = row - CompactTileStart(row), c = col - CompactTileStart(col);
        var entry = new byte[8];
        using (var fs = File.Open(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.Position = ((r * BundleSize) + c) * 8;
            await fs.ReadExactlyAsync(entry, 0, 8);
        }

        int position = BitConverter.ToInt32(entry, 0), length = BitConverter.ToInt32(entry, 4);
        if (position < 0 || length <= 0)
        {
            return [];
        }

        var data = new byte[length];
        using (var fs = File.Open(bundleFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fs.Position = position;
            await fs.ReadExactlyAsync(data, 0, length);
        }

        return data;
    }

    /// <summary>
    /// Renders the whole compact bundle (128 x 128 tiles, upper-left origin) that contains row/col.
    /// <paramref name="inBounds"/> limits the rendered tiles (seeding a bounding box).
    /// </summary>
    public async Task RenderCompactBundleAsync(IServiceMap map, int epsg, double scale, int row, int col,
                                               Func<int, int, bool>? inBounds = null,
                                               ICancelTracker? cancelTracker = null)
    {
        string bundleFile = BundleFile(epsg, scale, row, col);
        string calcFile = bundleFile + ".calc";
        if (File.Exists(bundleFile) || File.Exists(bundleFile + ".done") || File.Exists(calcFile))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(bundleFile)!);
        await File.WriteAllTextAsync(calcFile, "calc...");

        string tempFile = bundleFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var index = new int[BundleSize * BundleSize * 2];
        Array.Fill(index, -1);
        int position = 0;

        try
        {
            int startRow = CompactTileStart(row), startCol = CompactTileStart(col);

            using (var bundle = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                for (int r = 0; r < BundleSize; r++)
                {
                    for (int c = 0; c < BundleSize; c++)
                    {
                        if (cancelTracker?.Continue == false)
                        {
                            return;
                        }

                        if (inBounds?.Invoke(startRow + r, startCol + c) == false)
                        {
                            continue;
                        }

                        var data = await RenderAsync(map, GridOrientation.UpperLeft, epsg, scale, startRow + r, startCol + c, cancelTracker);
                        if (data.Length == 0)
                        {
                            continue;
                        }

                        await bundle.WriteAsync(data);
                        int i = ((r * BundleSize) + c) * 2;
                        index[i] = position;
                        index[i + 1] = data.Length;
                        position += data.Length;
                    }
                }
            }

            if (position == 0)
            {
                File.Delete(tempFile);
                await File.WriteAllTextAsync(bundleFile + ".done", "");
            }
            else
            {
                using (var fs = new FileStream(IndexFile(bundleFile), FileMode.Create, FileAccess.Write))
                {
                    foreach (int value in index)
                    {
                        fs.Write(BitConverter.GetBytes(value));
                    }
                }

                File.Move(tempFile, bundleFile, overwrite: true);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }

            try { File.Delete(calcFile); } catch { }
        }
    }

    #endregion

    private async Task<byte[]> RenderAsync(IServiceMap map, GridOrientation orientation, int epsg, double scale, int row, int col, ICancelTracker? cancelTracker)
    {
        var data = await new VectorTileBuilder(map, _metadata).BuildAsync(epsg, scale, row, col, orientation, cancelTracker) ?? [];

        if (_metadata.MvtSizeWarningKb > 0 && data.Length > _metadata.MvtSizeWarningKb * 1024)
        {
            _logWarning?.Invoke($"Vector tile {_mapName} EPSG:{epsg} 1:{Math.Round(scale)} row {row} col {col} is {data.Length / 1024} kB (gzipped), more than {_metadata.MvtSizeWarningKb} kB");
        }

        return data;
    }
}

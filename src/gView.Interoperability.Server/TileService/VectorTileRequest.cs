using gView.Framework.Cartography.VectorTiles;
using gView.Framework.Cartography.VectorTiles.Extensions;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.Geometry.Tiling;
using gView.Framework.Metadata;
using System;
using System.Threading.Tasks;

namespace gView.Interoperability.Server.TileService;

/// <summary>
/// Serves vector tiles (MVT, gzipped) of the tile service metadata grid to clients (route <c>vtc/...</c>).
/// </summary>
/// <remarks>
/// Requests:
/// <list type="bullet">
/// <item><c>{exploded|compact}/{ul|ll}/{epsg}/{level}/{row}/{col}.pbf</c>: any grid of the metadata, level = scale index</item>
/// <item><c>xyz/{z}/{x}/{y}.pbf</c>: EPSG:3857 upper-left, only for metadata scales that are standard WebMercator zoom levels</item>
/// </list>
/// Missing tiles are rendered on the fly if the metadata allows it, and written to the exploded cache (not into bundles).
/// </remarks>
[RegisterPlugIn("6C2A9F41-3B7E-4D5A-9E12-8F0B6D4C7A31")]
public class VectorTileRequest : IServiceRequestInterpreter
{
    private static readonly Guid MetadataProvider = new Guid("D33D3DD2-DD63-4a47-9F84-F840FE0D01C0");
    private IMapServer _mapServer = null;

    public void OnCreate(IMapServer mapServer) => _mapServer = mapServer;

    public InterpreterCapabilities Capabilities => null;
    public string IdentityName => "vtc";
    public string IdentityLongName => "Vector Tile Cache (MVT)";
    public int Priority => 70;

    public AccessTypes RequiredAccessTypes(IServiceRequestContext context) => AccessTypes.Map;

    async public Task Request(IServiceRequestContext context)
    {
        if (context?.ServiceRequest == null || _mapServer == null)
        {
            return;
        }

        var metadata = await context.GetMetadtaProviderAsync(MetadataProvider) as TileServiceMetadata;
        if (metadata == null || metadata.Use == false || metadata.FormatMvt == false)
        {
            throw new Exception("Service is not used with vector tiles");
        }

        string[] args = context.ServiceRequest.Request.Split('/');

        string cacheFormat;
        GridOrientation orientation;
        int epsg, level, row, col;

        if (args.Length == 4 && args[0].Equals("xyz", StringComparison.OrdinalIgnoreCase))
        {
            (cacheFormat, orientation, epsg) = ("", GridOrientation.UpperLeft, 3857);
            int z = int.Parse(args[1]);
            col = int.Parse(args[2]);
            row = int.Parse(args[3].Split('.')[0]);
            level = metadata.WebMercatorLevel(z);
            if (level < 0)
            {
                throw new ArgumentException($"Zoom level {z} is not a scale of the tile cache");
            }
        }
        else if (args.Length == 6)
        {
            cacheFormat = args[0].ToLower();
            orientation = args[1].ToLower() switch
            {
                "ul" => GridOrientation.UpperLeft,
                "ll" => GridOrientation.LowerLeft,
                _ => throw new ArgumentException("Orientation must be ul or ll")
            };
            epsg = int.Parse(args[2]);
            level = int.Parse(args[3]);
            row = int.Parse(args[4]);
            col = int.Parse(args[5].Split('.')[0]);
        }
        else
        {
            throw new ArgumentException("Invalid vector tile request");
        }

        if (!metadata.EPSGCodes.Contains(epsg) || level < 0 || level >= metadata.Scales.InnerList.Count)
        {
            throw new ArgumentException("Wrong epsg or level argument");
        }

        double scale = metadata.Scales.InnerList[level];
        string mapName = String.IsNullOrWhiteSpace(context.ServiceRequest.Folder)
            ? context.ServiceRequest.Service
            : $"{context.ServiceRequest.Folder}/{context.ServiceRequest.Service}";

        var cache = new VectorTileCache(_mapServer.TileCachePath, mapName, metadata,
            message => _mapServer.LogAsync(mapName, "VectorTiles", loggingMethod.error, message));

        byte[] data = null;
        if (cacheFormat == "compact" && orientation == GridOrientation.UpperLeft)
        {
            data = await cache.ReadCompactAsync(epsg, scale, row, col);
        }

        data ??= await cache.ReadExplodedAsync(orientation, epsg, scale, row, col);

        if (data == null && metadata.RenderTilesOnTheFly)
        {
            bool writeToCache = (orientation == GridOrientation.UpperLeft && metadata.UpperLeftCacheTiles) ||
                                (orientation == GridOrientation.LowerLeft && metadata.LowerLeftCacheTiles);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                data = await cache.RenderExplodedAsync(serviceMap, orientation, epsg, scale, row, col, writeToCache);
            }
        }

        context.ServiceRequest.ResponseContentType = VectorTileBuilder.ContentType;
        context.ServiceRequest.ResponseExpries = DateTime.UtcNow.AddDays(7);
        context.ServiceRequest.Response = data ?? Array.Empty<byte>();
    }
}

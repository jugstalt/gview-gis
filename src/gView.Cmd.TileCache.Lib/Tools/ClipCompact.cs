using gView.Cmd.Core.Abstraction;
using gView.Cmd.TileCache.Lib.CompactCache;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Data.TileCache;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Cmd.TileCache.Lib.Tools;

internal class ClipCompact
{
    private readonly ICancelTracker _cancelTracker;

    public ClipCompact(ICancelTracker? cancelTracker)
    {
        _cancelTracker = cancelTracker ?? new CancelTracker();
    }

    async public Task<bool> Run(
            string cacheSourceConfigFile,
            string cacheTargetFolder,
            IFeatureClass clipperFeatureClass,
            string? clipperDefintionQuery,
            int jpegQuality = -1,
            int maxlevel = -1,
            TileCacheClipType clipType = TileCacheClipType.Copy,
            ICommandLogger? logger = null
        )
    {
        // ToDo: check if it is a compact tilecache?

        var configFile = new FileInfo(cacheSourceConfigFile);
        if (!configFile.Exists)
        {
            throw new ArgumentException($"File {configFile.FullName} not exists");
        }

        string cacheSourceFolder = configFile.Directory!.FullName;
        CompactTileConfig cacheConfig = JsonSerializer.Deserialize<CompactTileConfig>(File.ReadAllText(configFile.FullName))!;
        var cacheSRef = SpatialReference.FromID($"epsg:{cacheConfig.Epsg}");
        if(cacheSRef is null || cacheSRef.EpsgCode == 0)
        {
            throw new Exception($"The config file dont provide an valid or known EPSG Code ({cacheConfig.Epsg})");
        }

        if (clipType == TileCacheClipType.Copy)
        {
            if (String.IsNullOrEmpty(cacheTargetFolder))
            {
                throw new ArgumentException("Target directory must be set");
            }
            if (Directory.Exists(cacheTargetFolder) && 
                    (
                        Directory.GetFiles(cacheTargetFolder).Length > 0 ||
                        Directory.GetDirectories(cacheTargetFolder).Length > 0
                    )
                )
            {
                throw new ArgumentException("Target directory is not empty!");
            }
            else
            {
                Directory.CreateDirectory(cacheTargetFolder);
            }
        }

        #region Determine Clipper Polygons

        List<IPolygon> clipperPolygons = new List<IPolygon>();

        using (var cursor = await clipperFeatureClass.GetFeatures(
                new QueryFilter()
                {
                    SubFields = clipperFeatureClass.ShapeFieldName,
                    FeatureSpatialReference = cacheSRef,
                    WhereClause = clipperDefintionQuery ?? ""
                })
            )
        {
            IFeature feature;

            while ((feature = await cursor.NextFeature()) != null)
            {
                if (feature.Shape is IPolygon)
                {
                    clipperPolygons.Add((IPolygon)feature.Shape);
                }
            }
        }

        if (clipType != TileCacheClipType.List)
        {
            logger?.LogLine($"{clipperPolygons.Count} polygons found for clipping...");
        }

        if (clipperPolygons.Count == 0)
        {
            logger?.LogLine($"No polygons found to clip");
            return false;
        }

        #endregion

        #region Image Encoding Parameters

        System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

        // Create an Encoder object based on the GUID
        // for the Quality parameter category.
        System.Drawing.Imaging.Encoder myEncoder =
            System.Drawing.Imaging.Encoder.Quality;

        // Create an EncoderParameters object.
        // An EncoderParameters object has an array of EncoderParameter
        // objects. In this case, there is only one
        // EncoderParameter object in the array.
        System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

        #endregion

        double dpm = cacheConfig.Dpi / 0.0254;

        foreach (var level in cacheConfig.Levels)
        {
            if (clipType != TileCacheClipType.List)
            {
                logger?.LogLine($"Level: {level.Level} Scale={level.Scale}");
            }

            double resolution = (level.Scale / dpm);
            double tileWorldWidth = cacheConfig.TileSize[0] * resolution;
            double tileWorldHeight = cacheConfig.TileSize[1] * resolution;

            var scaleDirectory = new DirectoryInfo(
                System.IO.Path.Combine(cacheSourceFolder, ((int)level.Scale).ToString()));
            if (!scaleDirectory.Exists)
            {
                continue;
            }

            Directory.CreateDirectory(System.IO.Path.Combine(cacheTargetFolder, ((int)level.Scale).ToString()));

            var scaleBundleFiles = new List<FileInfo>(scaleDirectory.GetFiles("*.tilebundle"));
            scaleBundleFiles.AddRange(scaleDirectory.GetFiles("*.tilebundle.done"));

            foreach (var bundleFile in scaleBundleFiles.ToArray())
            {
                var bundle = new Bundle(bundleFile.FullName);

                if (!bundle.Index.Exists)
                {
                    continue;
                }

                int startRow = bundle.StartRow, startCol = bundle.StartCol;
                double bundleWorldWidth = tileWorldWidth * 128D, bundleWorldHeight = tileWorldHeight * 128D;

                IPoint bundleLowerLeft = new Point(cacheConfig.Origin[0] + startCol * tileWorldWidth,
                                                   cacheConfig.Origin[1] - startRow * tileWorldHeight - bundleWorldHeight);
                IEnvelope bundleEnvelope = new Envelope(bundleLowerLeft, new Point(bundleLowerLeft.X + bundleWorldWidth, bundleLowerLeft.Y + bundleWorldHeight));

                if (!Intersect(bundleEnvelope, clipperPolygons))
                {
                    continue;
                }

                if (clipType == TileCacheClipType.List)
                {
                    logger?.LogLine(bundleFile.FullName);
                    continue;
                }
                if (clipType == TileCacheClipType.Cut)
                {
                    logger?.LogLine($"delete bundle: {bundleFile.FullName}");
                    bundleFile.Delete();
                    continue;
                }

                logger?.LogLine($"Clip bundle: {bundleFile.FullName}");

                var clippedBundleFile = new FileInfo(
                    System.IO.Path.Combine(cacheTargetFolder, ((int)level.Scale).ToString(), bundleFile.Name));

                if (!clippedBundleFile.Directory!.Exists)
                {
                    clippedBundleFile.Directory.Create();
                }

                if (clippedBundleFile.Exists)
                {
                    clippedBundleFile.Delete();
                }

                var indexBuilder = new CompactTilesIndexBuilder();
                int clippedTilePos = 0;

                for (int r = 0; r < 128; r++)
                {
                    for (int c = 0; c < 128; c++)
                    {
                        int tileLength;
                        int tilePos = bundle.Index.TilePosition(r, c, out tileLength);

                        if (tilePos >= 0 && tileLength >= 0)
                        {
                            IPoint tileLowerLeft = new Point(cacheConfig.Origin[0] + (startCol + c) * tileWorldWidth,
                                                             cacheConfig.Origin[1] - (startRow + r + 1) * tileWorldHeight);
                            IEnvelope tileEnvelope = new Envelope(tileLowerLeft, new Point(tileLowerLeft.X + tileWorldWidth, tileLowerLeft.Y + tileWorldHeight));

                            if (!Intersect(tileEnvelope, clipperPolygons))
                            {
                                continue;
                            }

                            logger?.LogLine($"Append tile {level.Level}/{startRow + r}/{startCol + c}");

                            byte[] data = bundle.ImageData(tilePos, tileLength);

                            if (jpegQuality > 0)
                            {
                                #region New Jpeg Quality

                                var ms = new System.IO.MemoryStream(data);
                                using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                                {
                                    var outputMs = new System.IO.MemoryStream();

                                    System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, Convert.ToInt64(jpegQuality));
                                    myEncoderParameters.Param[0] = myEncoderParameter;

                                    image.Save(outputMs, jpgEncoder, myEncoderParameters);
                                    data = outputMs.ToArray();
                                }

                                #endregion
                            }
                            using (var stream = new System.IO.FileStream(clippedBundleFile.FullName, System.IO.FileMode.Append))
                            {
                                stream.Write(data, 0, data.Length);
                            }

                            indexBuilder.SetValue(r, c, clippedTilePos, data.Length);
                            clippedTilePos += data.Length;
                        }
                    }
                }

                if (clippedTilePos > 0)
                {
                    indexBuilder.Save(
                        System.IO.Path.Combine(clippedBundleFile.Directory.FullName, new System.IO.FileInfo(bundle.Index.Filename).Name));
                }
            }

            if (maxlevel >= 0 && level.Level >= maxlevel)
            {
                break;
            }
        }

        if (clipType == TileCacheClipType.Copy)
        {
            #region write conf.json

            if (maxlevel >= 0)
            {
                cacheConfig.Levels = cacheConfig.Levels.Take(maxlevel + 1).ToArray();
            }

            File.WriteAllText(
                    System.IO.Path.Combine(cacheTargetFolder, "conf.json"), 
                    JsonSerializer.Serialize(cacheConfig, new JsonSerializerOptions() { WriteIndented = true })
                );

            #endregion
        }

        return true;
    }

    #region Helper

    bool Intersect(IEnvelope envelope, List<IPolygon> polygons)
    {
        foreach (var polygon in polygons)
        {
            var polygonEnvelope = polygon.Envelope;

            if (!envelope.Intersects(polygonEnvelope) && !envelope.Contains(polygonEnvelope) && !polygonEnvelope.Contains(envelope))
            {
                continue;
            }

            if (envelope.Contains(polygon.Envelope))
            {
                return true;
            }

            if (Algorithm.IntersectBox(polygon, envelope))
            {
                return true;
            }
        }
        return false;
    }

    static private System.Drawing.Imaging.ImageCodecInfo? GetEncoder(System.Drawing.Imaging.ImageFormat format)
    {

        System.Drawing.Imaging.ImageCodecInfo[] codecs
            = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();

        foreach (var codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }

        return null;
    }

    #endregion
}

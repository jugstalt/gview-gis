using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.TileCache;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace gView.Cmd.ClipCompactTilecache
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            string gmlSource = String.Empty;
            string cacheSource = String.Empty;
            string cacheTarget = String.Empty;
            int jpegQuality = -1, maxlevel = -1;
            bool listFilenames = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-gml")
                {
                    gmlSource = args[++i];
                }
                if (args[i] == "-cache")
                {
                    cacheSource = args[++i];
                }
                else if (args[i] == "-target")
                {
                    cacheTarget = args[++i];
                }
                else if (args[i] == "-jpeg-qual")
                {
                    jpegQuality = int.Parse(args[++i]);
                }
                else if (args[i] == "-maxlevel")
                {
                    maxlevel = int.Parse(args[++i]);
                }
                else if (args[i] == "-listfilenames")
                {
                    PlugInManager.InitSilent = true;
                    listFilenames = true;
                }
            }

            if (String.IsNullOrWhiteSpace(gmlSource) || String.IsNullOrWhiteSpace(cacheSource) || String.IsNullOrWhiteSpace(cacheTarget))
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.ClipCompactTilecache.exe -gml <Filename> -cache <cachedirectory> -target <cachetarget>");
                Console.WriteLine("                      [-jpeg-qual <quality  0..100>] -maxlevel <level>");
                Console.WriteLine("                      [-listfilenames]");
                return 1;
            }

            PlugInManager compMan = new PlugInManager();
            IFeatureDataset gmlDataset = compMan.CreateInstance(new Guid("dbabe7f1-fe46-4731-ab2b-8a324c60554e")) as IFeatureDataset;

            if (gmlDataset == null)
            {
                Console.WriteLine("GML Dataset-plugin is not supported");
                return 1;
            }

            await gmlDataset.SetConnectionString(gmlSource);
            await gmlDataset.Open();

            List<IPolygon> sourcePolygons = new List<IPolygon>();
            foreach (var element in await gmlDataset.Elements())
            {
                if (element.Class is IFeatureClass)
                {
                    var fc = (IFeatureClass)element.Class;

                    using (var cursor = await fc.GetFeatures(null))
                    {
                        IFeature feature;
                        while ((feature = await cursor.NextFeature()) != null)
                        {
                            if (feature.Shape is IPolygon)
                            {
                                sourcePolygons.Add((IPolygon)feature.Shape);
                            }
                        }
                    }
                }
            }

            if (!listFilenames)
            {
                Console.WriteLine(sourcePolygons.Count + " polygons found for clipping...");
            }

            FileInfo configFile = new FileInfo(cacheSource + @"\conf.json");
            if (!configFile.Exists)
            {
                throw new ArgumentException("File " + configFile.FullName + " not exists");
            }

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

            CompactTileConfig cacheConfig = JsonConvert.DeserializeObject<CompactTileConfig>(File.ReadAllText(configFile.FullName));
            double dpm = cacheConfig.Dpi / 0.0254;

            foreach (var level in cacheConfig.Levels)
            {
                if (!listFilenames)
                {
                    Console.WriteLine("Level: " + level.Level + " Scale=" + level.Scale);
                }

                double resolution = (level.Scale / dpm);
                double tileWorldWidth = cacheConfig.TileSize[0] * resolution;
                double tileWorldHeight = cacheConfig.TileSize[1] * resolution;

                var scaleDirectory = new DirectoryInfo(cacheSource + @"\" + ((int)level.Scale).ToString());
                if (!scaleDirectory.Exists)
                {
                    continue;
                }

                foreach (var bundleFile in scaleDirectory.GetFiles("*.tilebundle"))
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

                    if (!Intersect(bundleEnvelope, sourcePolygons))
                    {
                        continue;
                    }

                    if (listFilenames)
                    {
                        Console.WriteLine(bundleFile.FullName);
                        continue;
                    }

                    Console.WriteLine("Clip bundle: " + bundleFile.FullName);

                    var clippedBundleFile = new FileInfo(cacheTarget + "/" + (int)level.Scale + "/" + bundleFile.Name);
                    if (!clippedBundleFile.Directory.Exists)
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

                                if (!Intersect(tileEnvelope, sourcePolygons))
                                {
                                    continue;
                                }

                                Console.WriteLine("Append tile " + level.Level + "/" + (startRow + r) + "/" + (startCol + c));

                                byte[] data = bundle.ImageData(tilePos, tileLength);

                                if (jpegQuality > 0)
                                {
                                    #region New Jpeg Quality

                                    MemoryStream ms = new MemoryStream(data);
                                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                                    {
                                        MemoryStream outputMs = new MemoryStream();

                                        System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, Convert.ToInt64(jpegQuality));
                                        myEncoderParameters.Param[0] = myEncoderParameter;

                                        image.Save(outputMs, jpgEncoder, myEncoderParameters);
                                        data = outputMs.ToArray();
                                    }

                                    #endregion
                                }
                                using (var stream = new FileStream(clippedBundleFile.FullName, FileMode.Append))
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
                        indexBuilder.Save(clippedBundleFile.Directory.FullName + @"\" + new FileInfo(bundle.Index.Filename).Name);
                    }
                }

                if (maxlevel >= 0 && level.Level >= maxlevel)
                {
                    break;
                }
            }

            return 0;
        }

        static bool Intersect(IEnvelope envelope, List<IPolygon> polygons)
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

        static private System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {

            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}

using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace gView.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                //TestProj4();
                TestPerformance();

                //Console.ReadLine();

                //TestVtc().Wait();

                //ParseSQL();

                //Current.Engine = new gView.GraphicsEngine.GdiPlus.GdiGraphicsEngine(96);
                ////Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine(96);
                //Current.Encoder = new GraphicsEngine.GdiPlus.GdiBitmapEncoding();
                ////Current.Encoder = new GraphicsEngine.Skia.SkiaBitmapEncoding();

                //using (var bitmap = CreateImage(850, 600))
                //{
                //    using (var filteredBitmap = BaseFilter.ApplyFilter(bitmap, FilterImplementations.GrayscaleBT709))
                //    {
                //        var start = DateTime.Now;

                //        SaveBitmap(filteredBitmap, "C:\\temp\\graphic.jpg");

                //        Console.WriteLine($"Encoding Time: { (DateTime.Now - start).TotalMilliseconds }ms");
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
        }

        static void TestProj4()
        {
            var sRef4326 = SpatialReference.FromID("epsg:4326");

            var sRef31255 = SpatialReference.FromID("epsg:31255");
            var sRef31256 = SpatialReference.FromID("epsg:31256");
            var sRef3857 = SpatialReference.FromID("epsg:3857");
            var sRef23032 = new SpatialReference("test", "", "+proj=utm +zone=32 +ellps=intl +towgs84=-87,-98,-121,0,0,0,0 +units=m +no_defs ", null);

            using (var transformer = GeometricTransformerFactory.Create())
            {
                //var point = new Point(1443413.9574, 6133464.20414);
                ////var point = new Point(-27239.046, 335772.696625);
                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());

                //transformer.SetSpatialReferences(sRef3857, sRef4326);
                //point = (Point)transformer.Transform2D(point);
                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());

                var pCollection = new PointCollection();
                //pCollection.AddPoint(new Point(1443413.9574, 6133464.20414));
                pCollection.AddPoint(new Point(-27239.046, 335772.696625));
                Console.WriteLine(pCollection[0].X.ToString() + "    " + pCollection[0].Y.ToString());

                transformer.SetSpatialReferences(sRef31255, sRef3857);
                pCollection = (PointCollection)transformer.Transform2D(pCollection);
                Console.WriteLine(pCollection[0].X.ToString() + "    " + pCollection[0].Y.ToString());


                //transformer.SetSpatialReferences(sRef3857, sRef4326);
                //point = (Point)transformer.Transform2D(point);
                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());

                //transformer.SetSpatialReferences(sRef4326, sRef31255);
                //point = (Point)transformer.Transform2D(point);
                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());


                //var point = new Point(-27239.046, 335772.696625);

                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());

                //transformer.SetSpatialReferences(sRef31255, sRef23032);
                //var point2 = (Point)transformer.Transform2D(point);

                //Console.WriteLine(point2.X.ToString() + "    " + point2.Y.ToString());

                //transformer.SetSpatialReferences(sRef31255, sRef31256);
                //point2 = (Point)transformer.Transform2D(point);

                //Console.WriteLine(point2.X.ToString() + "    " + point2.Y.ToString());

                //transformer.SetSpatialReferences(sRef31256, sRef3857);
                //point=(Point)transformer.Transform2D(point);

                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());

                //transformer.SetSpatialReferences(sRef4326, sRef3857);
                //point = (Point)transformer.Transform2D(point);

                //Console.WriteLine(point.X.ToString() + "    " + point.Y.ToString());
            }
        }

        static void ExportProj4ToCsv()
        {
            //SpatialReference.FromID
        }

        static void TestPerformance()
        {
            //int[] bbox = new int[] { -115309, 231374, -114905, 231530 };
            //int[] bbox = new int[] { -219217, 23153, -10996, 231374 };
            int[] bbox = new int[] { -218012, 159987, -9987, 301012 };

            List<Task<double>> tasks = new List<Task<double>>();

            var startTime = DateTime.UtcNow;

            for (int i = 0; i < 300; i++)
            {
                //string server = "sever/gview5-basis";
                //string service = "geoservices/rest/services/sdep/gv_xyz_dkm_sdep";

                string server = "localhost:44331";
                string service = "geoservices/rest/services/test-publish/dgm_10m";

                //string service = "geoservices/rest/services/default/UEBERSICHTSKARTE";
                //string service = "geoservices/rest/services/sdep/gv_estag_dkm_sdep";
                //string service = "arcgis/rest/services/sdep/estag_dkm_sdep";

                //string server = "server";
                //string service = "geoservices/rest/services/sdep/gv_estag_dkm_sdep";

                string url = String.Empty;

                #region Image Request

                bbox = bbox.Select(x => x + (i / 1)).ToArray();

                url = "https://" + server + "/" + service + "/MapServer/export?" +
                "size=800,800&dpi=96&imageSR=&bboxSR=&format=png&layerDefs=&layers=&transparent=true&time=&layerTimeOptions=&dynamicLayers=&mapScale=0&rotation=0&datumTransformations=&mapRangeValues=&layerRangeValues=&layerParameterValues=&historicMoment=0&f=pjson&";
                //"bboxSR=&layers=&layerDefs=&size=800%2C800&imageSR=&format=png&transparent=true&dpi=&time=&layerTimeOptions=&dynamicLayers=&gdbVersion=&mapScale=&rotation=&f=pjson&";

                url += "bbox=" + String.Join(",", bbox);

                #endregion

                #region Query

                //url = "https://"+server+"/arcgis/rest/services/GRAZG81_SDET/estag_dkm_sdet_grazg81/MapServer/13/query?where=OBJECTID="+(i)+"&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=json";
                //url = "https://"+server+"/gview5/geoservices/rest/services/KATASTER_BEV/MapServer/306/query?text=&geometry=&geometryType=&inSR=&relationParam=&where=OBJECTID="+(i)+"&objectIds=&time=0&distance=0&units=&outFields=*&returnGeometry=true&maxAllowableOffset=0&geometryPrecision=0&outSR=&returnIdsOnly=false&returnCountOnly=false&returnExtentOnly=false&orderByFields=&outStatistics=&groupByFieldsForStatistics=&returnZ=false&returnM=false&returnDistinctValues=false&returnTrueCurves=false&resultOffset=0&resultRecordCount=0&datumTransformation=&rangeValues=&quantizationParameters=&parameterValues=&historicMoment=0&layerId=306&f=json";

                #endregion

                var task = ExportMapAsync(url);

                tasks.Add(task);
                //task.Wait();

                //Task.Delay(30).Wait();
            }

            Task.WaitAll(tasks.ToArray());

            double ms = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Min: " + tasks.Select(t => t.Result).Min() + " ms");
            Console.WriteLine("Max: " + tasks.Select(t => t.Result).Max() + " ms");
            Console.WriteLine("Avg: " + tasks.Select(t => t.Result).Average() + " ms");
            Console.WriteLine("====================================================");
            Console.WriteLine("Total: " + ms + " ms");
        }

        async static Task<double> ExportMapAsync(string url)
        {
            try
            {
                Console.WriteLine(url);

                var startTime = DateTime.UtcNow;

                using (var client = new WebClient())
                {
                    client.Proxy = null;
                    client.UseDefaultCredentials = true;
                    var response = await client.DownloadStringTaskAsync(new Uri(url));

                    //Console.WriteLine(response.Substring(0, Math.Min(80, response.Length)));
                    var exportResponse = JsonConvert.DeserializeObject<ExportResponse>(response);
                    Console.WriteLine(exportResponse.href);
                }

                var ms = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Console.WriteLine(ms);

                return ms;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Task Exception: " + ex);
            }
            return 0;
        }

        static double ExportMap(string url)
        {
            Console.WriteLine(url);

            var startTime = DateTime.UtcNow;

            using (var client = new WebClient())
            {
                client.Proxy = null;
                client.UseDefaultCredentials = true;
                var response = client.DownloadString(new Uri(url));
                
                //var exportResponse= JsonConvert.DeserializeObject<ExportResponse>(response);
                //Console.WriteLine(exportResponse.href);
            }

            var ms = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Console.WriteLine(ms);

            return ms;
        }

        #region Vector Tile Cache

        async static Task TestVtc()
        {
            var dataset = new DataSources.VectorTileCache.Dataset();
            await dataset.SetConnectionString("name=kagis;source=https://gis.ktn.gv.at/osgdi/flawi/tile.json");
            await dataset.Open();

            var start = DateTime.Now;

            for (int i = 0; i < 1; i++)
            {
                var featureCache = new gView.DataSources.VectorTileCache.FeatureCache(dataset);
                await featureCache.LoadAsync(14, 8841, 8842, 5787, 5788);

                Console.WriteLine($"TimeSpan: { (DateTime.Now - start).TotalMilliseconds } ms");

                int count = 0;
                foreach (var layername in featureCache.LayersNames)
                {
                    count += featureCache.FeatureCount(layername);
                    Console.WriteLine($"{ layername }: { featureCache.FeatureCount(layername) }");

                    //System.IO.File.WriteAllText($@"c:\temp\{ layername }.json", featureCache.ToGeoJson(layername));
                }

                Console.WriteLine($"Sum Featuers: { count }");
            }

            if (!String.IsNullOrEmpty(dataset.LastErrorMessage))
            {
                Console.WriteLine($"ERROR: { dataset.LastErrorMessage }");
            }
        }

        #endregion

        #region Classes

        private class ExportResponse
        {
            public string href { get; set; }
        }

        #endregion

        #region SQL Parser

        static public void ParseSQL()
        {
            "OBJECTID in (1,2)".CheckWhereClauseForSqlInjection();
        }

        #endregion

        #region Test Graphicsengine

        static public IBitmap CreateImage(int width, int height)
        {
            var bitmap = Current.Engine.CreateBitmap(width, height, PixelFormat.Rgb24);

            using (var canvas = bitmap.CreateCanvas())
            using (var brush = Current.Engine.CreateSolidBrush(ArgbColor.Yellow))
            using (var blackBrush = Current.Engine.CreateSolidBrush(ArgbColor.Black))
            using (var pen = Current.Engine.CreatePen(ArgbColor.Red, 10))
            {
                canvas.TextRenderingHint = TextRenderingHint.AntiAlias;
                canvas.SmoothingMode = SmoothingMode.AntiAlias;
                canvas.Clear(ArgbColor.White);

                using(var path = Current.Engine.CreateGraphicsPath())
                {
                    path.StartFigure();
                    if (path.PathBuildPerferences == GraphicsPathBuildPerferences.AddPointsPreferred)
                    {
                        path.AddPoint(10, 10);
                        path.AddPoint(100, 10);
                        path.AddPoint(100, 100);
                        path.AddPoint(10, 100);
                    } 
                    else
                    {
                        path.AddLine(10, 10, 100, 10);
                        path.AddLine(100, 10, 100, 100);
                        path.AddLine(100, 100, 10, 100);
                    }
                    path.CloseFigure();

                    var rect = path.GetBounds();

                    canvas.FillPath(brush, path);
                }

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        char c = (char)(byte)(x + y * 32);
                        using (var font = Current.Engine.CreateFont("Arial", 20, typefaceCharakter: c))
                        {
                            canvas.DrawText(c.ToString(), font, blackBrush, (x * 20) * 1.3f, ((y + 1) * 20) * 1.5f);
                        }
                    }
                }
                //canvas.SmoothingMode = SmoothingMode.AntiAlias;
                //canvas.FillEllipse(brush, 10, 10, width - 20, height - 20);
                //canvas.DrawEllipse(pen, 10, 10, width - 20, height - 20);

                //canvas.DrawText($"Umlaute: ÄÜÖßöäü{ Environment.NewLine }Und dann noch eine{ Environment.NewLine }Zeile", font, blackBrush, new CanvasPoint(10, 50));

                char cc = (char)40;
                var fontFamily = System.Drawing.FontFamily.Families.Where(f => f.Name == "BEV_DKM_Symbole_05_2012").FirstOrDefault();
                var iFont = Current.Engine.CreateFont(fontFamily.Name, 140);

                canvas.DrawText(cc.ToString(), iFont, blackBrush, 400, 400);

                using (var gdiBitmap = new System.Drawing.Bitmap(800, 800))
                using (var gr = System.Drawing.Graphics.FromImage(gdiBitmap))
                {
                    var gdiFont = new System.Drawing.Font(fontFamily, 140);
                    gr.DrawString(cc.ToString(), gdiFont, System.Drawing.Brushes.Black, 400, 400);
                    gdiBitmap.Save("C:\\temp\\font_test.png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }

            return bitmap;
        }

        static public void SaveBitmap(IBitmap bitmap, string filename)
        {
            bitmap.Save(filename, filename.EndsWith(".jpg") ? ImageFormat.Jpeg : ImageFormat.Png);

            //BitmapPixelData bmPixelData = null;
            //try
            //{
            //    bmPixelData = bitmap.Clone(PixelFormat.Format32bppArgb).LockBitmapPixelData(BitmapLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            //    using (var bm = new System.Drawing.Bitmap(bmPixelData.Width,
            //                      bmPixelData.Height,
            //                      bmPixelData.Stride,
            //                      System.Drawing.Imaging.PixelFormat.Format32bppArgb,
            //                      bmPixelData.Scan0))
            //    {
            //        bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            //    }
            //}
            //finally
            //{
            //    if (bmPixelData != null)
            //    {
            //        bitmap.UnlockBitmapPixelData(bmPixelData);
            //    }
            //}
        }


        #endregion
    }
}

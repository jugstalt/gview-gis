using gView.Framework.Geometry;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

        static void TestPerformance()
        {
            int[] bbox = new int[] { -115309, 231374, -114905, 231530 };
            //int[] bbox = new int[] { -219217, 23153, -10996, 231374 };

            List<Task<double>> tasks = new List<Task<double>>();

            var startTime = DateTime.UtcNow;

            for (int i = 0; i < 100; i++)
            {
                string server = "server.domain.at";
                //string service = "geoservices/rest/services/default/UEBERSICHTSKARTE";
                string service = "gview5/geoservices/rest/services/basis/KATASTER_BEV";
                //string service = "arcgis/rest/services/sdep/estag_dkm_sdep";

                string url = String.Empty;

                #region Image Request

                bbox = bbox.Select(x => x+ (i/1)).ToArray();

                url = "https://"+server+"/" + service + "/MapServer/export?" +
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

        #region Classes

        private class ExportResponse
        {
            public string href { get; set; }
        }

        #endregion
    }
}

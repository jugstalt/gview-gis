using gView.Framework.Geometry;
using System;

namespace gView.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            TestProj4();

            Console.ReadLine();
        }

        static void TestProj4()
        {
            var sRef4326 = SpatialReference.FromID("epsg:4326");

            var sRef31255 = SpatialReference.FromID("epsg:31255");
            var sRef31256 = SpatialReference.FromID("epsg:31256");
            var sRef3857 = SpatialReference.FromID("epsg:3857");

            var sRef23032= new SpatialReference("test", "", "+proj=utm +zone=32 +ellps=intl +towgs84=-87,-98,-121,0,0,0,0 +units=m +no_defs ", null);

            using (var transformer = new GeometricTransformer())
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
    }
}

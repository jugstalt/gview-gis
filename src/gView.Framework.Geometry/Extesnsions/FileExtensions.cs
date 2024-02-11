using gView.Framework.Core.Geometry;
using System.IO;

namespace gView.Framework.Geometry.Extesnsions
{

    static public class FileExtensions
    {
        static public ISpatialReference FileSpatialReference(this FileInfo fi)
        {
            FileInfo fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".prj");
            if (fiPrj.Exists)
            {
                StreamReader sr = new StreamReader(fiPrj.FullName);
                string wkt = sr.ReadToEnd();
                sr.Close();

                return SpatialReference.FromWKT(File.ReadAllText(fiPrj.FullName));
            }

            fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".wkt");
            if (fiPrj.Exists)
            {
                return SpatialReference.FromWKT(fiPrj.FullName);
            }

            fiPrj = new FileInfo(fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".epsg");
            if (fiPrj.Exists)
            {
                return new SpatialReference(File.ReadAllText(fiPrj.FullName));
            }

            // global ".epsg" File
            fiPrj = new FileInfo(System.IO.Path.Combine(fi.Directory.FullName, ".epsg"));
            if (fiPrj.Exists)
            {
                return new SpatialReference(File.ReadAllText(fiPrj.FullName));
            }


            return null;
        }
    }
}

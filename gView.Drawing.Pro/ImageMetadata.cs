using System;
using System.Collections.Generic;
using gView.Drawing.Pro.Exif;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Drawing.Pro
{
    public class ImageMetadata
    {
        public double? Longitute { get; set; }
        public double? Latitude { get; set; }

        public DateTime? DateTimeOriginal { get; set; }

        public void ReadExif(ExifTagCollection exif)
        {
            if (exif == null)
                return;

            if (exif["GPSLatitude"] != null)
            {
                try
                {
                    string lat = exif["GPSLatitude"].Value;
                    this.Latitude = FromGMS(lat);

                    if (exif["GPSLatitudeRef"] != null && exif["GPSLatitudeRef"].Value.ToLower().StartsWith("south"))
                        this.Latitude = -this.Latitude;
                }
                catch { }
            }

            if (exif["GPSLongitude"] != null)
            {
                try
                {
                    string lng = exif["GPSLongitude"].Value;
                    this.Longitute = FromGMS(lng);

                    if (exif["GPSLongitudeRef"] != null && exif["GPSLongitudeRef"].Value.ToLower().StartsWith("west"))
                        this.Longitute = -this.Longitute;
                }
                catch { }
            }

            if (exif["DateTimeOriginal"] != null)
            {
                DateTime t;
                if (DateTime.TryParseExact(exif["DateTimeOriginal"].Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out t))
                    this.DateTimeOriginal = t;
            }
        }

        #region Helper

        private double FromGMS(string gms)
        {
            gms = gms.Replace("°", " ").Replace("'", " ").Replace("\"", " ").Replace(",", ".");
            while (gms.Contains("  "))
                gms = gms.Replace("  ", " ");

            string[] p = gms.Split(' ');

            double ret = double.Parse(p[0], ImageGlobals.Nhi );
            if (p.Length > 1)
                ret += double.Parse(p[1], ImageGlobals.Nhi) / 60.0;
            if (p.Length > 2)
                ret += double.Parse(p[2], ImageGlobals.Nhi) / 3600.0;

            return ret;
        }

        #endregion
    }
}

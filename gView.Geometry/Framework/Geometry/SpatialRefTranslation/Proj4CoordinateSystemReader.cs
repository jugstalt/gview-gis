using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using System.Globalization;
using gView.Framework.Proj;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    internal class Proj4CoordinateSystemReader
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        
        public static object Create(string Proj4Parameters)
        {
            if (Proj4Parameters == null) 
                throw new ArgumentException();

            Proj4Parameters = Proj4Parameters.Trim();
            while (Proj4Parameters.IndexOf("  ") != -1)
                Proj4Parameters = Proj4Parameters.Replace("  ", " ");

            string[] parameters = Proj4Parameters.Split(' ');

            switch (ParameterValue(parameters, "+proj"))
            {
                case "longlat":
                    return ReadGeographicCoordinateSystem(parameters);
                case "merc":
                case "tmerc":
                case "lcc":
                case "utm":
                case "laea":
                    return ReadProjectedCoordinateSystem(parameters);
                default:
                    throw new NotSupportedException(String.Format("projection {0} is not implemented.", (ParameterValue(parameters, "+proj") != null ? ParameterValue(parameters, "+proj") : "none")));
            }

            return null;
        }

        public static string ParameterValue(string [] parameters, string parameter)
        {
            foreach (string param in parameters)
            {
                string[] pv = param.Split('=');
                if (pv[0] == parameter)
                {
                    if (pv.Length > 1) return pv[1];
                    return "";
                }
            }
            return null;
        }

        private static string Parameter(string p4param, out string val)
        {
            string[] pv = p4param.Split('=');
            if (pv.Length > 1)
                val = pv[1];
            else
                val = "";

            return pv[0];
        }

        private static GeographicCoordinateSystem ReadGeographicCoordinateSystem(string[] parameters)
        {
            //+proj=longlat +ellps=intl +pm=ferro +towgs84=145,-187,103,0,0,0,0 +no_defs 

            WGS84ConversionInfo wgs84 = ReadWGS84ConversionInfo(ParameterValue(parameters, "+towgs84"));
            LinearUnit unit = ReadUnit(parameters) as LinearUnit;
            Ellipsoid ellipsoid = ReadEllipsoid(parameters, unit);
            HorizontalDatum datum = new HorizontalDatum(
                "Unknown",
                DatumType.IHD_Classic,
                ellipsoid,
                wgs84);

            GeographicCoordinateSystem geogrCoordSystem =
                new GeographicCoordinateSystem(
                    "Unknown",
                    new AngularUnit(Math.PI / 180.0,String.Empty,String.Empty,String.Empty,"degree",String.Empty,String.Empty),
                    datum,
                    ReadPrimeMeridian(parameters),
                    new AxisInfo("Geodetic latitude", AxisOrientation.North),
                    new AxisInfo("Geodetic longitude", AxisOrientation.East));

            return geogrCoordSystem;
        }

        private static ProjectedCoordinateSystem ReadProjectedCoordinateSystem(string[] parameters)
        {
            // +proj=tmerc +lat_0=0 +lon_0=-62 +k=0.999500 +x_0=400000 +y_0=0 +ellps=clrk80 +towgs84=725,685,536,0,0,0,0 +units=m +no_defs

            string projParam = ParameterValue(parameters, "+proj");
            string projName = ProjDB.ProjectionNameByP4(projParam);

            List<ProjectionParameter> projParameters = new List<ProjectionParameter>();
            if (projParam.ToLower() == "utm" && projName.ToLower() == "transverse_mercator")
            {
                projParameters.Add(new ProjectionParameter("scale_factor", 0.9996));
                projParameters.Add(new ProjectionParameter("false_easting", 500000.0));
            }
            foreach (string p4param in parameters)
            {
                string val;
                string p4 = Parameter(p4param, out val);

                double v;
                if (!double.TryParse(val,NumberStyles.Number, _nhi, out v)) continue;

                switch (p4)
                {
                    case "+lat_0":
                        projParameters.Add(new ProjectionParameter("latitude_of_origin", v));
                        break;
                    case "+lon_0":
                        projParameters.Add(new ProjectionParameter("longitude_of_origin", v));
                        projParameters.Add(new ProjectionParameter("central_meridian", v));
                        break;
                    case "+k":
                        projParameters.Add(new ProjectionParameter("scale_factor", v));
                        break;
                    case "+x_0":
                        projParameters.Add(new ProjectionParameter("false_easting", v));
                        break;
                    case "+y_0":
                        projParameters.Add(new ProjectionParameter("false_northing", v));
                        break;
                    case "+alpha":
                        projParameters.Add(new ProjectionParameter("azimuth", v));
                        break;
                    case "+lat_1":
                        projParameters.Add(new ProjectionParameter("standard_parallel_1", v));
                        break;
                    case "+lat_2":
                        projParameters.Add(new ProjectionParameter("standard_parallel_2", v));
                        break;
                    case "+zone":
                        projParameters.Add(new ProjectionParameter("Central_Meridian", -177.0 + 6 * (v - 1)));
                        break;
                }
            }

            Projection proj = new Projection(projName, projParameters.ToArray(), "", "", "", "");
            GeographicCoordinateSystem geogrCoordSystem = ReadGeographicCoordinateSystem(parameters);
            
            AxisInfo[] axis={
                    new AxisInfo("Easting", AxisOrientation.East),
                    new AxisInfo("Northing", AxisOrientation.North)
                    };

            ProjectedCoordinateSystem projCoordSystem = new ProjectedCoordinateSystem(
                geogrCoordSystem.HorizontalDatum,
                axis,
                geogrCoordSystem,
                new LinearUnit(1.0, String.Empty, String.Empty, String.Empty, "metre", String.Empty, String.Empty),
                proj);
            projCoordSystem.Name = "Unknown";

            return projCoordSystem;
        }
        private static WGS84ConversionInfo ReadWGS84ConversionInfo(string towgs84)
        {
            WGS84ConversionInfo wgs84ConversionInfo = new WGS84ConversionInfo();
            
            if (towgs84 != null)
            {
                string[] w = towgs84.Split(',');
                if (w.Length > 2)
                {
                    wgs84ConversionInfo.Dx = double.Parse(w[0], _nhi);
                    wgs84ConversionInfo.Dy = double.Parse(w[1], _nhi);
                    wgs84ConversionInfo.Dz = double.Parse(w[2], _nhi);
                }
                if (w.Length > 5)
                {
                    wgs84ConversionInfo.Ex = double.Parse(w[3], _nhi);
                    wgs84ConversionInfo.Ey = double.Parse(w[4], _nhi);
                    wgs84ConversionInfo.Ez = double.Parse(w[5], _nhi);
                }
                if (w.Length > 6)
                    wgs84ConversionInfo.Ppm = double.Parse(w[6], _nhi);
            }

            return wgs84ConversionInfo;
        }

        private static Ellipsoid ReadEllipsoid(string [] parameters, LinearUnit unit)
        {
            string ellps=ParameterValue(parameters,"+ellps");

            if (ellps != null)
            {
                double majorAxis, minorAxis, invFlattening;
                string name = ProjDB.SpheroidByP4(ellps, out majorAxis, out minorAxis, out invFlattening);

                if (name == String.Empty)
                {
                    throw new NotSupportedException(String.Format("Ellipsoid {0} is not implemented.", ellps));
                }

                string a = ParameterValue(parameters, "+a");
                string b = ParameterValue(parameters, "+b");
                if (a != null) majorAxis = double.Parse(a);
                if (b != null) minorAxis = double.Parse(b);
                if (a != null && b != null)
                {
                    if (majorAxis == minorAxis) 
                        invFlattening = 0;
                    else 
                        invFlattening = minorAxis / (majorAxis - minorAxis);
                }

                return new Ellipsoid(majorAxis, minorAxis, invFlattening, true,
                    unit, String.Empty, string.Empty, string.Empty,
                    name, string.Empty, string.Empty);
            }
            else
            {
                string a = ParameterValue(parameters, "+a");
                string b = ParameterValue(parameters, "+b");

                if (a != null && b != null)
                {
                    return new Ellipsoid(
                        double.Parse(a, _nhi),
                        double.Parse(b, _nhi),
                        0, false, unit);
                }
            }
            return null;
        }

        private static PrimeMeridian ReadPrimeMeridian(string[] parameters)
        {
            string pm = ParameterValue(parameters, "+pm");

            if (pm == null) pm = "";
            double longitude;
            string name = ProjDB.PrimeMeridianByP4(pm, out longitude);

            return new PrimeMeridian(
                name,
                new AngularUnit(Math.PI / 180.0),
                longitude);
        }
        private static Unit ReadUnit(string[] parameters)
        {
            return new LinearUnit(1.0, string.Empty, string.Empty, string.Empty, "METER", string.Empty, string.Empty);
        }
    }
}

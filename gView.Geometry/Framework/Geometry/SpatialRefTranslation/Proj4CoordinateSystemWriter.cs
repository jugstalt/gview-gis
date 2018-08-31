using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Proj;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    internal class Proj4CoordinateSystemWriter
    {
        private class P4Parameter
        {
            private string _p,_v;
            public P4Parameter(string p, string v)
            {
                _p = p;
                _v = v;
            }

            public string Parameter
            {
                get { return _p; }
            }
            public string ParameterValue
            {
                get { return _v; }
            }
        }

        private static List<P4Parameter> _p4parameters = new List<P4Parameter>();
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private static object lockThis = new object();

        public static string Write(object obj)
        {
            lock (lockThis)
            {
                _p4parameters.Clear();
                if (obj is GeographicCoordinateSystem)
                {
                    _p4parameters.Add(new P4Parameter("+proj", "longlat"));
                    WriteGeographicCoordinateSystem(obj as GeographicCoordinateSystem);
                }
                if (obj is ProjectedCoordinateSystem)
                {
                    WriteProjectedCoordinateSystem(obj as ProjectedCoordinateSystem);
                }

                return Write();
            }
        }

        private static string Write()
        {
            StringBuilder sb = new StringBuilder();

            foreach (P4Parameter parameter in _p4parameters)
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(parameter.Parameter + "=" + parameter.ParameterValue);
            }
            return sb.ToString();
        }

        private static void WriteGeographicCoordinateSystem(GeographicCoordinateSystem geogrCoordSystem)
        {
            // +proj=longlat +ellps=clrk80 +towgs84=-294.7,-200.1,525.5,0,0,0,0 +no_defs

            #region Ellipsoid
            Ellipsoid ellipsoid = geogrCoordSystem.HorizontalDatum.Ellipsoid;

            double majorAxis,minorAxis,invFlattening;
            string ellps = ProjDB.SpheroidByName(ellipsoid.Name,out majorAxis,out minorAxis,out invFlattening);

            if (ellps != String.Empty &&
                majorAxis == ellipsoid.SemiMajorAxis &&
                minorAxis == ellipsoid.SemiMinorAxis)
            {
                _p4parameters.Add(new P4Parameter("+ellps", ellps));
            }
            else
            {
                _p4parameters.Add(new P4Parameter("+a", ellipsoid.SemiMajorAxis.ToString(_nhi)));
                _p4parameters.Add(new P4Parameter("+b", ellipsoid.SemiMinorAxis.ToString(_nhi)));
            }
            #endregion

            #region WGS84
            WGS84ConversionInfo wgs84 = geogrCoordSystem.HorizontalDatum.WGS84Parameters;
            if (wgs84.IsInUse)
            {
                _p4parameters.Add(new P4Parameter("+towgs84",
                    String.Format("{0},{1},{2},{3},{4},{5},{6}",
                    wgs84.Dx.ToString(_nhi), wgs84.Dy.ToString(_nhi), wgs84.Dz.ToString(_nhi),
                    wgs84.Ex.ToString(_nhi), wgs84.Ey.ToString(_nhi), wgs84.Ez.ToString(_nhi),
                    wgs84.Ppm.ToString(_nhi))));
            }
            #endregion
        }

        private static void WriteProjectedCoordinateSystem(ProjectedCoordinateSystem projCoordSystem)
        {
            string projP4 = ProjDB.ProjectionP4ByName(projCoordSystem.Projection.Name);
            if (projP4 == String.Empty)
                throw new NotImplementedException("Unknown projection " + projCoordSystem.Projection.Name);

            _p4parameters.Add(new P4Parameter("+proj",projP4));

            WriteGeographicCoordinateSystem(projCoordSystem.GeographicCoordinateSystem);

            for (int i = 0; i < projCoordSystem.Projection.NumParameters; i++)
            {
                ProjectionParameter projParameter = projCoordSystem.Projection.GetParameter(i);

                switch (projParameter.Name.ToLower())
                {
                    case "latitude_of_origin":
                    case "latitude_of_center":
                        _p4parameters.Add(new P4Parameter("+lat_0", projParameter.Value.ToString(_nhi)));
                        break;
                    case "central_meridian":
                    case "longitude_of_origin":
                    case "longitude_of_center":
                        _p4parameters.Add(new P4Parameter("+lon_0", projParameter.Value.ToString(_nhi)));
                        break;
                    case "scale_factor":
                        _p4parameters.Add(new P4Parameter("+k", projParameter.Value.ToString(_nhi)));
                        break;
                    case "false_easting":
                        _p4parameters.Add(new P4Parameter("+x_0", projParameter.Value.ToString(_nhi)));
                        break;
                    case "false_northing":
                        _p4parameters.Add(new P4Parameter("+y_0", projParameter.Value.ToString(_nhi)));
                        break;
                    case "azimuth":
                        _p4parameters.Add(new P4Parameter("+alpha", projParameter.Value.ToString(_nhi)));
                        break;
                    case "standard_parallel_1":
                        _p4parameters.Add(new P4Parameter("+lat_1", projParameter.Value.ToString(_nhi)));
                        break;
                    case "standard_parallel_2":
                        _p4parameters.Add(new P4Parameter("+lat_2", projParameter.Value.ToString(_nhi)));
                        break;
                }
            }
        }
    }
}

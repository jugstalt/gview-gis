/*
Copyright 2006 Jerry Huxtable

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*
 * This file was semi-automatically converted from the public-domain USGS PROJ source.
 */
using System;
using GeoAPI.Geometries;
using Proj4Net.Datum;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    /**
    * Transverse Mercator Projection algorithm is taken from the USGS PROJ package.
    */
    public class TransverseMercatorProjection : CylindricalProjection
    {

        // ReSharper disable InconsistentNaming
        private const double FC1 = 1.0;
        private const double FC2 = 0.5;
        private const double FC3 = 0.16666666666666666666;
        private const double FC4 = 0.08333333333333333333;
        private const double FC5 = 0.05;
        private const double FC6 = 0.03333333333333333333;
        private const double FC7 = 0.02380952380952380952;
        private const double FC8 = 0.01785714285714285714;
        // ReSharper restore InconsistentNaming

        private int _utmZone = -1;
        private double _esp;
        private double _ml0;
        private double[] _en;

        public TransverseMercatorProjection()
        {
            Ellipsoid = Ellipsoid.GRS80;
            ProjectionLatitude = ProjectionMath.ToRadians(0);
            ProjectionLongitude = ProjectionMath.ToRadians(0);
            MinLongitude = ProjectionMath.ToRadians(-90);
            MaxLongitude = ProjectionMath.ToRadians(90);
            Initialize();
        }

        /**
        * Set up a projection suitable for State Plane Coordinates.
        */
        public TransverseMercatorProjection(Ellipsoid ellipsoid, double lon_0, double lat_0, double k, double x_0, double y_0)
        {
            Ellipsoid = ellipsoid;
            ProjectionLongitude = lon_0;
            ProjectionLatitude = lat_0;
            ScaleFactor = k;
            FalseEasting = x_0;
            FalseNorthing = y_0;
            Initialize();
        }

        public override Object Clone()
        {
            TransverseMercatorProjection p = (TransverseMercatorProjection)base.Clone();
            if (_en != null)
                p._en = (double[])_en.Clone();
            return p;
        }

        public override Boolean IsRectilinear
        {
            get { return false; }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (_spherical)
            {
                _esp = ScaleFactor;
                _ml0 = .5 * _esp;
            }
            else
            {
                _en = ProjectionMath.enfn(_es);
                _ml0 = ProjectionMath.mlfn(_projectionLatitude, Math.Sin(_projectionLatitude), Math.Cos(_projectionLatitude), _en);
                _esp = _es / (1.0 - _es);
            }
        }

        public int GetRowFromNearestParallel(double latitude)
        {
            int degrees = (int)ProjectionMath.RadiansToDegreesFn(ProjectionMath.NormalizeLatitude(latitude));
            if (degrees < -80 || degrees > 84)
                return 0;
            if (degrees > 80)
                return 24;
            return (degrees + 80) / 8 + 3;
        }

        public int GetZoneFromNearestMeridian(double longitude)
        {
            int zone = (int)Math.Floor((ProjectionMath.NormalizeLongitude(longitude) + Math.PI) * 30.0 / Math.PI) + 1;
            if (zone < 1)
                zone = 1;
            else if (zone > 60)
                zone = 60;
            return zone;
        }

        public int UTMZone
        {
            get { return _utmZone; }
            set
            {
                _utmZone = value;
                _projectionLongitude = (--value + .5) * Math.PI / 30.0 - Math.PI;
                _projectionLatitude = 0.0;
                _scaleFactor = 0.9996;
                _falseEasting = 500000;
                _falseNorthing = _isSouth ? 10000000.0 : 0.0;
                Initialize();
            }
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            if (_spherical)
            {
                double cosphi = Math.Cos(lpphi);
                double b = cosphi * Math.Sin(lplam);

                xy.X = _ml0 * _scaleFactor * Math.Log((1.0 + b) / (1.0 - b));
                double ty = cosphi * Math.Cos(lplam) / Math.Sqrt(1.0 - b * b);
                ty = ProjectionMath.Acos(ty);
                if (lpphi < 0.0)
                    ty = -ty;
                xy.Y = _esp * (ty - _projectionLatitude);
            }
            else
            {
                double al, als, n, t;
                double sinphi = Math.Sin(lpphi);
                double cosphi = Math.Cos(lpphi);
                t = Math.Abs(cosphi) > 1e-10 ? sinphi / cosphi : 0.0;
                t *= t;
                al = cosphi * lplam;
                als = al * al;
                al /= Math.Sqrt(1.0 - _es * sinphi * sinphi);
                n = _esp * cosphi * cosphi;
                xy.X = _scaleFactor * al * (FC1 +
                    FC3 * als * (1.0 - t + n +
                    FC5 * als * (5.0 + t * (t - 18.0) + n * (14.0 - 58.0 * t)
                    + FC7 * als * (61.0 + t * (t * (179.0 - t) - 479.0))
                )));
                xy.Y = _scaleFactor * (ProjectionMath.mlfn(lpphi, sinphi, cosphi, _en) - _ml0 +
                    sinphi * al * lplam * FC2 * (1.0 +
                    FC4 * als * (5.0 - t + n * (9.0 + 4.0 * n) +
                    FC6 * als * (61.0 + t * (t - 58.0) + n * (270.0 - 330 * t)
                    + FC8 * als * (1385.0 + t * (t * (543.0 - t) - 3111.0))
                ))));
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate coord)
        {
            if (_spherical)
            {
                double h = Math.Exp(x / _scaleFactor);
                double g = 0.5 * (h - 1.0 / h);
                h = Math.Cos(_projectionLatitude + y / _scaleFactor);
                coord.Y = ProjectionMath.Asin(Math.Sqrt((1.0 - h * h) / (1.0 + g * g)));
                if (y < 0)
                    coord.Y = -coord.Y;
                coord.X = Math.Atan2(g, h);
            }
            else
            {
                double n, con, cosphi, d, ds, sinphi, t;

                coord.Y = ProjectionMath.inv_mlfn(_ml0 + y / _scaleFactor, _es, _en);
                if (Math.Abs(y) >= ProjectionMath.PiHalf)
                {
                    coord.Y = y < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
                    coord.X = 0.0;
                }
                else
                {
                    sinphi = Math.Sin(coord.Y);
                    cosphi = Math.Cos(coord.Y);
                    t = Math.Abs(cosphi) > 1e-10 ? sinphi / cosphi : 0.0;
                    n = _esp * cosphi * cosphi;
                    d = x * Math.Sqrt(con = 1.0 - _es * sinphi * sinphi) / _scaleFactor;
                    con *= t;
                    t *= t;
                    ds = d * d;
                    coord.Y -= (con * ds / (1.0 - _es)) * FC2 * (1.0 -
                        ds * FC4 * (5.0 + t * (3.0 - 9.0 * n) + n * (1.0 - 4 * n) -
                        ds * FC6 * (61.0 + t * (90.0 - 252.0 * n +
                            45.0 * t) + 46.0 * n
                        - ds * FC8 * (1385.0 + t * (3633.0 + t * (4095.0 + 1574.0 * t)))
                    )));
                    coord.X = d * (FC1 -
                        ds * FC3 * (1.0 + 2.0 * t + n -
                        ds * FC5 * (5.0 + t * (28.0 + 24.0 * t + 8.0 * n) + 6.0 * n
                        - ds * FC7 * (61.0 + t * (662.0 + t * (1320.0 + 720.0 * t)))
                    ))) / cosphi;
                }
            }
            return coord;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            if (_utmZone >= 0)
                return "Universal Tranverse Mercator";
            return "Transverse Mercator";
        }

    }
}

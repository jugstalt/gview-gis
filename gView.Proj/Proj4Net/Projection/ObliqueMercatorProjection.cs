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

using System;
using GeoAPI.Geometries;
using Proj4Net.Datum;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    /// <summary>
    /// Oblique Mercator Projection algorithm is taken from the USGS PROJ package.
    /// </summary>
    public class ObliqueMercatorProjection : CylindricalProjection
    {

        private const double Tolerance = 1.0e-7;

        private double lamc, lam1, phi1, lam2, phi2, Gamma, al, bl, el, singam, cosgam, sinrot, cosrot, u_0;
        private Boolean ellips, rot;

        public ObliqueMercatorProjection()
        {
            Ellipsoid = new Ellipsoid("WGS84", 6378137.0, 0.0, 298.257223563, "WGS 84");//WGS84;
            ProjectionLatitude = ProjectionMath.ToRadians(0);
            ProjectionLongitude = ProjectionMath.ToRadians(0);
            MinLongitude = ProjectionMath.ToRadians(-60);
            MaxLongitude = ProjectionMath.ToRadians(60);
            MinLatitude = ProjectionMath.ToRadians(-80);
            MaxLatitude = ProjectionMath.ToRadians(80);
            Alpha = ProjectionMath.ToRadians(-45);//FIXME
            Initialize();
        }

        /**
        * Set up a projection suitable for State Plane Coordinates.
        */
        public ObliqueMercatorProjection(Ellipsoid ellipsoid, double lon_0, double lat_0, double alpha, double k, double x_0, double y_0)
        {
            Ellipsoid = ellipsoid;
            lamc = lon_0;
            ProjectionLatitude = lat_0;
            Alpha = alpha;
            ScaleFactor = k;
            FalseEasting = x_0;
            FalseNorthing = y_0;
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            double con, com, cosphi0, d, f, h, l, sinphi0, p, j;

            //FIXME-setup rot, alpha, longc,lon/lat1/2
            rot = true;
            lamc = _lonc;

            // true if alpha provided
            int azi = Double.IsNaN(Alpha) ? 0 : 1;
            if (azi != 0)
            { // alpha specified
                if (Math.Abs(Alpha) <= Tolerance ||
                    Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) <= Tolerance ||
                    Math.Abs(Math.Abs(Alpha) - ProjectionMath.PiHalf) <= Tolerance)
                    throw new ProjectionException("Obl 1");
            }
            else
            {
                if (Math.Abs(phi1 - phi2) <= Tolerance ||
                    (con = Math.Abs(phi1)) <= Tolerance ||
                    Math.Abs(con - ProjectionMath.PiHalf) <= Tolerance ||
                    Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) <= Tolerance ||
                    Math.Abs(Math.Abs(phi2) - ProjectionMath.PiHalf) <= Tolerance) throw new ProjectionException("Obl 2");
            }
            com = (_spherical = (EccentricitySquared == 0.0)) ? 1 : Math.Sqrt(_oneEs);
            if (Math.Abs(ProjectionLatitude) > EPS10)
            {
                sinphi0 = Math.Sin(ProjectionLatitude);
                cosphi0 = Math.Cos(ProjectionLatitude);
                if (!Spherical)
                {
                    con = 1.0 - EccentricitySquared * sinphi0 * sinphi0;
                    bl = cosphi0 * cosphi0;
                    bl = Math.Sqrt(1.0 + EccentricitySquared * bl * bl / _oneEs);
                    al = bl * ScaleFactor * com / con;
                    d = bl * com / (cosphi0 * Math.Sqrt(con));
                }
                else
                {
                    bl = 1.0;
                    al = ScaleFactor;
                    d = 1.0 / cosphi0;
                }
                if ((f = d * d - 1.0) <= 0.0)
                    f = 0.0;
                else
                {
                    f = Math.Sqrt(f);
                    if (ProjectionLatitude < 0.0)
                        f = -f;
                }
                el = f += d;
                if (!Spherical)
                    el *= Math.Pow(ProjectionMath.tsfn(ProjectionLatitude, sinphi0, Eccentricity), bl);
                else
                    el *= Math.Tan(.5 * (ProjectionMath.PiHalf - ProjectionLatitude));
            }
            else
            {
                bl = 1.0 / com;
                al = ScaleFactor;
                el = d = f = 1.0;
            }
            if (azi != 0)
            {
                Gamma = Math.Asin(Math.Sin(Alpha) / d);
                ProjectionLongitude = lamc - Math.Asin((.5 * (f - 1.0 / f)) *
                   Math.Tan(Gamma)) / bl;
            }
            else
            {
                if (!Spherical)
                {
                    h = Math.Pow(ProjectionMath.tsfn(phi1, Math.Sin(phi1), Eccentricity), bl);
                    l = Math.Pow(ProjectionMath.tsfn(phi2, Math.Sin(phi2), Eccentricity), bl);
                }
                else
                {
                    h = Math.Tan(.5 * (ProjectionMath.PiHalf - phi1));
                    l = Math.Tan(.5 * (ProjectionMath.PiHalf - phi2));
                }
                f = el / h;
                p = (l - h) / (l + h);
                j = el * el;
                j = (j - l * h) / (j + l * h);
                if ((con = lam1 - lam2) < -Math.PI)
                    lam2 -= ProjectionMath.TwoPI;
                else if (con > Math.PI)
                    lam2 += ProjectionMath.TwoPI;
                ProjectionLongitude = ProjectionMath.NormalizeLongitude(.5 * (lam1 + lam2) - Math.Atan(
                   j * Math.Tan(.5 * bl * (lam1 - lam2)) / p) / bl);
                Gamma = Math.Atan(2.0 * Math.Sin(bl * ProjectionMath.NormalizeLongitude(lam1 - ProjectionLongitude)) /
                   (f - 1.0 / f));
                Alpha = Math.Asin(d * Math.Sin(Gamma));
            }
            singam = Math.Sin(Gamma);
            cosgam = Math.Cos(Gamma);
            //		f = MapMath.param(params, "brot_conv").i ? Gamma : alpha;
            f = Alpha;//FIXME
            sinrot = Math.Sin(f);
            cosrot = Math.Cos(f);
            //		u_0 = MapMath.param(params, "bno_uoff").i ? 0. :
            u_0 = false ? 0.0 ://FIXME
                Math.Abs(al * Math.Atan(Math.Sqrt(d * d - 1.0) / cosrot) / bl);
            if (ProjectionLatitude < 0.0)
                u_0 = -u_0;
        }

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            double con, q, s, ul, us, vl, vs;

            vl = Math.Sin(bl * lam);
            if (Math.Abs(Math.Abs(phi) - ProjectionMath.PiHalf) <= EPS10)
            {
                ul = phi < 0.0 ? -singam : singam;
                us = al * phi / bl;
            }
            else
            {
                q = el / (!Spherical ? Math.Pow(ProjectionMath.tsfn(phi, Math.Sin(phi), Eccentricity), bl)
                    : Math.Tan(.5 * (ProjectionMath.PiHalf - phi)));
                s = .5 * (q - 1.0 / q);
                ul = 2.0 * (s * singam - vl * cosgam) / (q + 1.0 / q);
                con = Math.Cos(bl * lam);
                if (Math.Abs(con) >= Tolerance)
                {
                    us = al * Math.Atan((s * cosgam + vl * singam) / con) / bl;
                    if (con < 0.0)
                        us += Math.PI * al / bl;
                }
                else
                    us = al * bl * lam;
            }
            if (Math.Abs(Math.Abs(ul) - 1.0) <= EPS10) throw new ProjectionException("Obl 3");
            vs = .5 * al * Math.Log((1.0 - ul) / (1.0 + ul)) / bl;
            us -= u_0;
            if (!rot)
            {
                xy.X = us;
                xy.Y = vs;
            }
            else
            {
                xy.X = vs * cosrot + us * sinrot;
                xy.Y = us * cosrot - vs * sinrot;
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            double q, s, ul, us, vl, vs;

            if (!rot)
            {
                us = x;
                vs = y;
            }
            else
            {
                vs = x * cosrot - y * sinrot;
                us = y * cosrot + x * sinrot;
            }
            us += u_0;
            q = Math.Exp(-bl * vs / al);
            s = .5 * (q - 1.0 / q);
            vl = Math.Sin(bl * us / al);
            ul = 2.0 * (vl * cosgam + s * singam) / (q + 1.0 / q);
            if (Math.Abs(Math.Abs(ul) - 1.0) < EPS10)
            {
                lp.X = 0.0;
                lp.Y = ul < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            }
            else
            {
                lp.Y = el / Math.Sqrt((1.0 + ul) / (1.0 - ul));
                if (!Spherical)
                {
                    lp.Y = ProjectionMath.Phi2(Math.Pow(lp.Y, 1.0 / bl), Eccentricity);
                }
                else
                    lp.Y = ProjectionMath.PiHalf - 2.0 * Math.Atan(lp.Y);
                lp.X = -Math.Atan2((s * cosgam -
                    vl * singam), Math.Cos(bl * us / al)) / bl;
            }
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Oblique Mercator";
        }

    }
}
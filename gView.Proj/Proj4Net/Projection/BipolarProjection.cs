using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
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

    public class BipolarProjection : Projection
    {

        private Boolean noskew;

        private const double EPS = 1e-10;
        //private const double EPS10 = 1e-10;
        private const double ONEEPS = 1.000000001;
        private const int NITER = 10;
        private const double lamB = -.34894976726250681539;
        private const double n = .63055844881274687180;
        private const double F = 1.89724742567461030582;
        private const double Azab = .81650043674686363166;
        private const double Azba = 1.82261843856185925133;
        private const double T = 1.27246578267089012270;
        private const double rhoc = 1.20709121521568721927;
        private const double cAzc = .69691523038678375519;
        private const double sAzc = .71715351331143607555;
        private const double C45 = .70710678118654752469;
        private const double S45 = .70710678118654752410;
        private const double C20 = .93969262078590838411;
        private const double S20 = -.34202014332566873287;
        private const double R110 = 1.91986217719376253360;
        private const double R104 = 1.81514242207410275904;

        public BipolarProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(-80);
            MaxLatitude = ProjectionMath.ToRadians(80);
            ProjectionLongitude = ProjectionMath.ToRadians(-90);
            MinLongitude = ProjectionMath.ToRadians(-90);
            MaxLongitude = ProjectionMath.ToRadians(90);
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            double cphi, sphi, tphi, t, al, Az, z, Av, cdlam, sdlam, r;
            Boolean tag;

            cphi = Math.Cos(lpphi);
            sphi = Math.Sin(lpphi);
            cdlam = Math.Cos(sdlam = lamB - lplam);
            sdlam = Math.Sin(sdlam);
            if (Math.Abs(Math.Abs(lpphi) - ProjectionMath.PiHalf) < EPS10)
            {
                Az = lpphi < 0.0 ? Math.PI : 0.0;
                tphi = Double.MaxValue;
            }
            else
            {
                tphi = sphi / cphi;
                Az = Math.Atan2(sdlam, C45 * (tphi - cdlam));
            }
            if (tag = (Az > Azba))
            {
                cdlam = Math.Cos(sdlam = lplam + R110);
                sdlam = Math.Sin(sdlam);
                z = S20 * sphi + C20 * cphi * cdlam;
                if (Math.Abs(z) > 1.0)
                {
                    if (Math.Abs(z) > ONEEPS)
                        throw new ProjectionException("F");
                    else z = z < 0.0 ? -1.0 : 1.0;
                }
                else
                    z = Math.Acos(z);
                if (tphi != Double.MaxValue)
                    Az = Math.Atan2(sdlam, (C20 * tphi - S20 * cdlam));
                Av = Azab;
                coord.Y = rhoc;
            }
            else
            {
                z = S45 * (sphi + cphi * cdlam);
                if (Math.Abs(z) > 1.0)
                {
                    if (Math.Abs(z) > ONEEPS)
                        throw new ProjectionException("F");
                    else z = z < 0.0 ? -1.0 : 1.0;
                }
                else
                    z = Math.Acos(z);
                Av = Azba;
                coord.Y = -rhoc;
            }
            if (z < 0.0) throw new ProjectionException("F");
            r = F * (t = Math.Pow(Math.Tan(.5 * z), n));
            if ((al = .5 * (R104 - z)) < 0.0)
                throw new ProjectionException("F");
            al = (t + Math.Pow(al, n)) / T;
            if (Math.Abs(al) > 1.0)
            {
                if (Math.Abs(al) > ONEEPS)
                    throw new ProjectionException("F");
                else al = al < 0.0 ? -1.0 : 1.0;
            }
            else
                al = Math.Acos(al);
            if (Math.Abs(t = n * (Av - Az)) < al)
                r /= Math.Cos(al + (tag ? t : -t));
            coord.X = r * Math.Sin(t);
            coord.Y += (tag ? -r : r) * Math.Cos(t);
            if (noskew)
            {
                t = coord.X;
                coord.X = -coord.X * cAzc - coord.Y * sAzc;
                coord.Y = -coord.Y * cAzc + t * sAzc;
            }
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            double t, r, rp, rl, al, z = 0, fAz, Az, s, c, Av;
            Boolean neg;
            int i;

            if (noskew)
            {
                t = xyx;
                coord.X = -xyx * cAzc + xyy * sAzc;
                coord.Y = -xyy * cAzc - t * sAzc;
            }
            if (neg = (xyx < 0.0))
            {
                coord.Y = rhoc - xyy;
                s = S20;
                c = C20;
                Av = Azab;
            }
            else
            {
                coord.Y += rhoc;
                s = S45;
                c = C45;
                Av = Azba;
            }
            rl = rp = r = ProjectionMath.Distance(xyx, xyy);
            fAz = Math.Abs(Az = Math.Atan2(xyx, xyy));
            for (i = NITER; i > 0; --i)
            {
                z = 2.0 * Math.Atan(Math.Pow(r / F, 1 / n));
                al = Math.Acos((Math.Pow(Math.Tan(.5 * z), n) +
                   Math.Pow(Math.Tan(.5 * (R104 - z)), n)) / T);
                if (fAz < al)
                    r = rp * Math.Cos(al + (neg ? Az : -Az));
                if (Math.Abs(rl - r) < EPS)
                    break;
                rl = r;
            }
            if (i == 0) throw new ProjectionException("I");
            Az = Av - Az / n;
            coord.Y = Math.Asin(s * Math.Cos(z) + c * Math.Sin(z) * Math.Cos(Az));
            coord.X = Math.Atan2(Math.Sin(Az), c / Math.Tan(z) - s * Math.Cos(Az));
            if (neg)
                coord.X -= R110;
            else
                coord.X = lamB - coord.X;
            return coord;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override void Initialize()
        { // bipc
            base.Initialize();
            //		noskew = pj_param(params, "bns").i;//FIXME
        }

        public override String ToString()
        {
            return "Bipolar Conic of Western Hemisphere";
        }

    }
}
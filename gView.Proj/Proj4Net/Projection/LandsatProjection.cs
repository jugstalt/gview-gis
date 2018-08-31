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
using Proj4Net.Utility;

namespace Proj4Net.Projection
{

    public class LandsatProjection : Projection
    {

        private double a2, a4, b, c1, c3;
        private double q, t, u, w, p22, sa, ca, xj, rlm, rlm2;

        private const double Tolerance = 1e-7;
        private const double PI_HALFPI = 4.71238898038468985766;
        private const double TWOPI_HALFPI = 7.85398163397448309610;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            int l, nn;
            double lamt = 0, xlam, sdsq, c, d, s, lamdp = 0, phidp, lampp, tanph,
                lamtp, cl, sd, sp, fac, sav, tanphi;

            if (lpphi > ProjectionMath.PiHalf)
                lpphi = ProjectionMath.PiHalf;
            else if (lpphi < -ProjectionMath.PiHalf)
                lpphi = -ProjectionMath.PiHalf;
            lampp = lpphi >= 0.0 ? ProjectionMath.PiHalf : PI_HALFPI;
            tanphi = Math.Tan(lpphi);
            for (nn = 0; ; )
            {
                sav = lampp;
                lamtp = lplam + p22 * lampp;
                cl = Math.Cos(lamtp);
                if (Math.Abs(cl) < Tolerance)
                    lamtp -= Tolerance;
                fac = lampp - Math.Sin(lampp) * (cl < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf);
                for (l = 50; l > 0; --l)
                {
                    lamt = lplam + p22 * sav;
                    if (Math.Abs(c = Math.Cos(lamt)) < Tolerance)
                        lamt -= Tolerance;
                    xlam = (_oneEs * tanphi * sa + Math.Sin(lamt) * ca) / c;
                    lamdp = Math.Atan(xlam) + fac;
                    if (Math.Abs(Math.Abs(sav) - Math.Abs(lamdp)) < Tolerance)
                        break;
                    sav = lamdp;
                }
                if (l == 0 || ++nn >= 3 || (lamdp > rlm && lamdp < rlm2))
                    break;
                if (lamdp <= rlm)
                    lampp = TWOPI_HALFPI;
                else if (lamdp >= rlm2)
                    lampp = ProjectionMath.PiHalf;
            }
            if (l != 0)
            {
                sp = Math.Sin(lpphi);
                phidp = ProjectionMath.Asin((_oneEs * ca * sp - sa * Math.Cos(lpphi) *
                    Math.Sin(lamt)) / Math.Sqrt(1.0 - EccentricitySquared * sp * sp));
                tanph = Math.Log(Math.Tan(ProjectionMath.PiFourth + .5 * phidp));
                sd = Math.Sin(lamdp);
                sdsq = sd * sd;
                s = p22 * sa * Math.Cos(lamdp) * Math.Sqrt((1.0 + t * sdsq)
                     / ((1.0 + w * sdsq) * (1.0 + q * sdsq)));
                d = Math.Sqrt(xj * xj + s * s);
                xy.X = b * lamdp + a2 * Math.Sin(2.0 * lamdp) + a4 *
                    Math.Sin(lamdp * 4.0) - tanph * s / d;
                xy.Y = c1 * sd + c3 * Math.Sin(lamdp * 3.0) + tanph * xj / d;
            }
            else
                xy.X = xy.Y = Double.PositiveInfinity;
            return xy;
        }

        /*
            public Point2D.Double projectInverse(double xyx, double xyy, Point2D.Double out) {
                int nn;
                double lamt, sdsq, s, lamdp, phidp, sppsq, dd, sd, sl, fac, scl, sav, spp;

                lamdp = xy.x / b;
                nn = 50;
                do {
                    sav = lamdp;
                    sd = Math.sin(lamdp);
                    sdsq = sd * sd;
                    s = p22 * sa * Math.cos(lamdp) * sqrt((1. + t * sdsq)
                         / ((1. + w * sdsq) * (1. + q * sdsq)));
                    lamdp = xy.x + xy.y * s / xj - a2 * Math.sin(
                        2. * lamdp) - a4 * Math.sin(lamdp * 4.) - s / xj * (
                        c1 * Math.sin(lamdp) + c3 * Math.sin(lamdp * 3.));
                    lamdp /= b;
                } while (Math.abs(lamdp - sav) >= Tolerance && --nn);
                sl = Math.sin(lamdp);
                fac = exp(sqrt(1. + s * s / xj / xj) * (xy.y - 
                    c1 * sl - c3 * Math.sin(lamdp * 3.)));
                phidp = 2. * (Math.atan(fac) - FORTPI);
                dd = sl * sl;
                if (Math.abs(Math.cos(lamdp)) < Tolerance)
                    lamdp -= Tolerance;
                spp = Math.sin(phidp);
                sppsq = spp * spp;
                lamt = Math.atan(((1. - sppsq * rone_es) * Math.tan(lamdp) * 
                    ca - spp * sa * sqrt((1. + q * dd) * (
                    1. - sppsq) - sppsq * u) / Math.cos(lamdp)) / (1. - sppsq 
                    * (1. + u)));
                sl = lamt >= 0. ? 1. : -1.;
                scl = Math.cos(lamdp) >= 0. ? 1. : -1;
                lamt -= HALFPI * (1. - scl) * sl;
                lp.lam = lamt - p22 * lamdp;
                if (Math.abs(sa) < Tolerance)
                    lp.phi = aasin(spp / sqrt(one_es * one_es + es * sppsq));
                else
                    lp.phi = Math.atan((Math.tan(lamdp) * Math.cos(lamt) - ca * Math.sin(lamt)) /
                        (one_es * sa));
                return lp;
            }
        */

        private void seraz0(double lam, double mult)
        {
            double sdsq, h, s, fc, sd, sq, d__1;

            lam *= DTR;
            sd = Math.Sin(lam);
            sdsq = sd * sd;
            s = p22 * sa * Math.Cos(lam) * Math.Sqrt((1.0 + t * sdsq) / ((
                1.0 + w * sdsq) * (1.0 + q * sdsq)));
            d__1 = 1.0 + q * sdsq;
            h = Math.Sqrt((1.0 + q * sdsq) / (1.0 + w * sdsq)) * ((1.0 +
                w * sdsq) / (d__1 * d__1) - p22 * ca);
            sq = Math.Sqrt(xj * xj + s * s);
            b += fc = mult * (h * xj - s * s) / sq;
            a2 += fc * Math.Cos(lam + lam);
            a4 += fc * Math.Cos(lam * 4.0);
            fc = mult * s * (h + xj) / sq;
            c1 += fc * Math.Cos(lam);
            c3 += fc * Math.Cos(lam * 3.0);
        }

        public override void Initialize()
        {
            base.Initialize();
            int land, path;
            double lam, alf, esc, ess;

            //FIXME		land = pj_param(params, "ilsat").i;
            land = 1;
            if (land <= 0 || land > 5)
                throw new ProjectionException("-28");
            //FIXME		path = pj_param(params, "ipath").i;
            path = 120;
            if (path <= 0 || path > (land <= 3 ? 251 : 233))
                throw new ProjectionException("-29");
            if (land <= 3)
            {
                ProjectionLongitude = DTR * 128.87 - ProjectionMath.TwoPI / 251.0 * path;
                p22 = 103.2669323;
                alf = DTR * 99.092;
            }
            else
            {
                ProjectionLongitude = DTR * 129.3 - ProjectionMath.TwoPI / 233.0 * path;
                p22 = 98.8841202;
                alf = DTR * 98.2;
            }
            p22 /= 1440.0;
            sa = Math.Sin(alf);
            ca = Math.Cos(alf);
            if (Math.Abs(ca) < 1e-9)
                ca = 1e-9;
            esc = EccentricitySquared * ca * ca;
            ess = EccentricitySquared * sa * sa;
            w = (1.0 - esc) * _roneEs;
            w = w * w - 1.0;
            q = ess * _roneEs;
            t = ess * (2.0 - EccentricitySquared) * _roneEs * _roneEs;
            u = esc * _roneEs;
            xj = _oneEs * _oneEs * _oneEs;
            rlm = Math.PI * (1.0 / 248.0 + .5161290322580645);
            rlm2 = rlm + ProjectionMath.TwoPI;
            a2 = a4 = b = c1 = c3 = 0.0;
            seraz0(0.0, 1.0);
            for (lam = 9.0; lam <= 81.0001; lam += 18.0)
                seraz0(lam, 4.0);
            for (lam = 18; lam <= 72.0001; lam += 18.0)
                seraz0(lam, 2.0);
            seraz0(90.0, 1.0);
            a2 /= 30.0;
            a4 /= 60.0;
            b /= 30.0;
            c1 /= 15.0;
            c3 /= 45.0;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Landsat";
        }

    }

}
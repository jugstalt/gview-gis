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

    public class VanDerGrintenProjection : Projection
    {

        private const double TOL = 1.0e-10;
        private const double THIRD = .33333333333333333333;
        private const double TWO_THRD = .66666666666666666666;
        private const double C2_27 = .07407407407407407407;
        private const double PI4_3 = 4.18879020478639098458;
        private const double PISQ = 9.86960440108935861869;
        private const double TPISQ = 19.73920880217871723738;
        private const double HPISQ = 4.93480220054467930934;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double al, al2, g, g2, p2;

            p2 = Math.Abs(lpphi / ProjectionMath.PiHalf);
            if ((p2 - TOL) > 1.0) throw new ProjectionException("F");
            if (p2 > 1.0)
                p2 = 1.0;
            if (Math.Abs(lpphi) <= TOL)
            {
                xy.X = lplam;
                xy.Y = 0.0;
            }
            else if (Math.Abs(lplam) <= TOL || Math.Abs(p2 - 1.0) < TOL)
            {
                xy.X = 0.0;
                xy.Y = Math.PI * Math.Tan(.5 * Math.Asin(p2));
                if (lpphi < 0.0) xy.Y = -xy.Y;
            }
            else
            {
                al = .5 * Math.Abs(Math.PI / lplam - lplam / Math.PI);
                al2 = al * al;
                g = Math.Sqrt(1.0 - p2 * p2);
                g = g / (p2 + g - 1.0);
                g2 = g * g;
                p2 = g * (2.0 / p2 - 1.0);
                p2 = p2 * p2;
                xy.X = g - p2; g = p2 + al2;
                xy.X = Math.PI * (al * xy.X + Math.Sqrt(al2 * xy.X * xy.X - g * (g2 - p2))) / g;
                if (lplam < 0.0) xy.X = -xy.X;
                xy.Y = Math.Abs(xy.X / Math.PI);
                xy.Y = 1.0 - xy.Y * (xy.Y + 2.0 * al);
                if (xy.Y < -TOL) throw new ProjectionException("F");
                if (xy.Y < 0.0)
                    xy.Y = 0.0;
                else
                    xy.Y = Math.Sqrt(xy.Y) * (lpphi < 0.0 ? -Math.PI : Math.PI);
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double t, c0, c1, c2, c3, al, r2, r, m, d, ay, x2, y2;

            x2 = xyx * xyx;
            if ((ay = Math.Abs(xyy)) < TOL)
            {
                lp.Y = 0.0;
                t = x2 * x2 + TPISQ * (x2 + HPISQ);
                lp.X = Math.Abs(xyx) <= TOL ? 0.0 :
                   .5 * (x2 - PISQ + Math.Sqrt(t)) / xyx;
                return lp;
            }
            y2 = xyy * xyy;
            r = x2 + y2; r2 = r * r;
            c1 = -Math.PI * ay * (r + PISQ);
            c3 = r2 + ProjectionMath.TwoPI * (ay * r + Math.PI * (y2 + Math.PI * (ay + ProjectionMath.PiHalf)));
            c2 = c1 + PISQ * (r - 3.0 * y2);
            c0 = Math.PI * ay;
            c2 /= c3;
            al = c1 / c3 - THIRD * c2 * c2;
            m = 2.0 * Math.Sqrt(-THIRD * al);
            d = C2_27 * c2 * c2 * c2 + (c0 * c0 - THIRD * c2 * c1) / c3;
            if (((t = Math.Abs(d = 3.0 * d / (al * m))) - TOL) <= 1.0)
            {
                d = t > 1.0 ? (d > 0.0 ? 0.0 : Math.PI) : Math.Acos(d);
                lp.Y = Math.PI * (m * Math.Cos(d * THIRD + PI4_3) - THIRD * c2);
                if (xyy < 0.0) lp.Y = -lp.Y;
                t = r2 + TPISQ * (x2 - y2 + HPISQ);
                lp.X = Math.Abs(xyx) <= TOL ? 0.0 :
                   .5 * (r - PISQ + (t <= 0.0 ? 0.0 : Math.Sqrt(t))) / xyx;
            }
            else
                throw new ProjectionException("I");
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "van der Grinten (I)";
        }

    }
}
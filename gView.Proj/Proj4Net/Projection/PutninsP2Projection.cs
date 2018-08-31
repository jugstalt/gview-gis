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

    public class PutninsP2Projection : Projection
    {

        private const double C_x = 1.89490;
        private const double C_y = 1.71848;
        private const double C_p = 0.6141848493043784;
        //private final static double EPS = 1e-10;
        private const int NITER = 10;
        private const double PI_DIV_3 = 1.0471975511965977;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double p, c, s, V;
            int i;

            p = C_p * Math.Sin(lpphi);
            s = lpphi * lpphi;
            xy.Y *= 0.615709 + s * (0.00909953 + s * 0.0046292);
            for (i = NITER; i > 0; --i)
            {
                c = Math.Cos(lpphi);
                s = Math.Sin(lpphi);
                xy.Y -= V = (lpphi + s * (c - 1.0) - p) /
                    (1.0 + c * (c - 1.0) - s * s);
                if (Math.Abs(V) < EPS10)
                    break;
            }
            if (i == 0)
                xy.Y = lpphi < 0 ? -PI_DIV_3 : PI_DIV_3;
            xy.X = C_x * lplam * (Math.Cos(lpphi) - 0.5);
            xy.Y = C_y * Math.Sin(lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double c;

            lp.Y = ProjectionMath.Asin(xyy / C_y);
            lp.X = xyx / (C_x * ((c = Math.Cos(lp.Y)) - 0.5));
            lp.Y = ProjectionMath.Asin((lp.Y + Math.Sin(lp.Y) * (c - 1.0)) / C_p);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Putnins P2";
        }

    }
}
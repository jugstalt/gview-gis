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


    public class NicolosiProjection : Projection
    {

        //private final static double EPS = 1e-10;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            if (Math.Abs(lplam) < EPS10)
            {
                xy.X = 0;
                xy.Y = lpphi;
            }
            else if (Math.Abs(lpphi) < EPS10)
            {
                xy.X = lplam;
                xy.Y = 0.0;
            }
            else if (Math.Abs(Math.Abs(lplam) - ProjectionMath.PiHalf) < EPS10)
            {
                xy.X = lplam * Math.Cos(lpphi);
                xy.Y = ProjectionMath.PiHalf * Math.Sin(lpphi);
            }
            else if (Math.Abs(Math.Abs(lpphi) - ProjectionMath.PiHalf) < EPS10)
            {
                xy.X = 0;
                xy.Y = lpphi;
            }
            else
            {
                double tb, c, d, m, n, r2, sp;

                tb = ProjectionMath.PiHalf / lplam - lplam / ProjectionMath.PiHalf;
                c = lpphi / ProjectionMath.PiHalf;
                d = (1 - c * c) / ((sp = Math.Sin(lpphi)) - c);
                r2 = tb / d;
                r2 *= r2;
                m = (tb * sp / d - 0.5 * tb) / (1.0 + r2);
                n = (sp / r2 + 0.5 * d) / (1.0 + 1.0 / r2);
                double x = Math.Cos(lpphi);
                x = Math.Sqrt(m * m + x * x / (1.0 + r2));
                xy.X = ProjectionMath.PiHalf * (m + (lplam < 0.0 ? -x : x));
                double y = Math.Sqrt(n * n - (sp * sp / r2 + d * sp - 1.0) /
                    (1.0 + 1.0 / r2));
                xy.Y = ProjectionMath.PiHalf * (n + (lpphi < 0.0 ? y : -y));
            }
            return xy;
        }

        public override String ToString()
        {
            return "Nicolosi Globular";
        }

    }
}
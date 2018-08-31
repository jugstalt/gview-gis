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

    public class McBrydeThomasFlatPolarSine2Projection : Projection
    {

        private const int MaxIter = 10;
        private const double LoopTolerance = 1e-7;
        private const double C1 = 0.45503;
        private const double C2 = 1.36509;
        private const double C3 = 1.41546;
        private const double C_x = 0.22248;
        private const double C_y = 1.44492;
        private const double C1_2 = 0.33333333333333333333333333;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double k, V, t;
            int i;

            k = C3 * Math.Sin(lpphi);
            for (i = MaxIter; i > 0; i--)
            {
                t = lpphi / C2;
                xy.Y -= V = (C1 * Math.Sin(t) + Math.Sin(lpphi) - k) /
                    (C1_2 * Math.Cos(t) + Math.Cos(lpphi));
                if (Math.Abs(V) < LoopTolerance)
                    break;
            }
            t = lpphi / C2;
            xy.X = C_x * lplam * (1.0 + 3.0 * Math.Cos(lpphi) / Math.Cos(t));
            xy.Y = C_y * Math.Sin(t);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double t, s;

            lp.Y = C2 * (t = ProjectionMath.Asin(xyy / C_y));
            lp.X = xyx / (C_x * (1.0 + 3.0 * Math.Cos(lp.Y) / Math.Cos(t)));
            lp.Y = ProjectionMath.Asin((C1 * Math.Sin(t) + Math.Sin(lp.Y)) / C3);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "McBryde-Thomas Flat-Pole Sine (No. 2)";
        }

    }
}
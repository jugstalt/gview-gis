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


    public class NellProjection : Projection
    {

        private const int MaxIter = 10;
        private const double LoopTolerance = 1e-7;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double k, V;
            int i;

            k = 2.0 * Math.Sin(lpphi);
            V = lpphi * lpphi;
            xy.Y *= 1.00371 + V * (-0.0935382 + V * -0.011412);
            for (i = MaxIter; i > 0; --i)
            {
                xy.Y -= V = (lpphi + Math.Sin(lpphi) - k) /
                    (1.0 + Math.Cos(lpphi));
                if (Math.Abs(V) < LoopTolerance)
                    break;
            }
            xy.X = 0.5 * lplam * (1.0 + Math.Cos(lpphi));
            xy.Y = lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double th, s;

            lp.X = 2.0 * xyx / (1.0 + Math.Cos(xyy));
            lp.Y = ProjectionMath.Asin(0.5 * (xyy + Math.Sin(xyy)));
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Nell";
        }

    }
}
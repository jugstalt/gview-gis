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

    public class FoucautSinusoidalProjection : Projection
    {
        private double n, n1;

        private const int MaxIter = 10;
        private const double LoopTolerance = 1e-7;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double t;

            t = Math.Cos(lpphi);
            xy.X = lplam * t / (n + n1 * t);
            xy.Y = n * lpphi + n1 * Math.Sin(lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double V;
            int i;

            if (n != 0)
            {
                lp.Y = xyy;
                for (i = MaxIter; i > 0; --i)
                {
                    lp.Y -= V = (n * lp.Y + n1 * Math.Sin(lp.Y) - xyy) /
                        (n + n1 * Math.Cos(lp.Y));
                    if (Math.Abs(V) < LoopTolerance)
                        break;
                }
                if (i == 0)
                    lp.Y = xyy < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            }
            else
                lp.Y = ProjectionMath.Asin(xyy);
            V = Math.Cos(lp.Y);
            lp.X = xyx * (n + n1 * V) / V;
            return lp;
        }

        public override void Initialize()
        {
            base.Initialize();
            //		n = pj_param(params, "dn").f;
            if (n < 0.0 || n > 1.0)
                throw new ProjectionException("-99");
            n1 = 1.0 - n;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Foucaut Sinusoidal";
        }

    }
}
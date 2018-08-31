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


    public class Eckert2Projection : Projection
    {

        private const double FXC = 0.46065886596178063902;
        private const double FYC = 1.44720250911653531871;
        private const double C13 = 0.33333333333333333333;
        private const double ONEEPS = 1.0000001;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = FXC * lplam * (xy.Y = Math.Sqrt(4.0 - 3.0 * Math.Sin(Math.Abs(lpphi))));
            xy.Y = FYC * (2.0 - xy.Y);
            if (lpphi < 0.0) xy.Y = -xy.Y;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.X = xyx / (FXC * (lp.Y = 2.0 - Math.Abs(xyy) / FYC));
            lp.Y = (4.0 - lp.Y * lp.Y) * C13;
            if (Math.Abs(lp.Y) >= 1.0)
            {
                if (Math.Abs(lp.Y) > ONEEPS) throw new ProjectionException("I");
                else
                    lp.Y = lp.Y < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            }
            else
                lp.Y = Math.Asin(lp.Y);
            if (xyy < 0)
                lp.Y = -lp.Y;
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Eckert II";
        }

    }
}
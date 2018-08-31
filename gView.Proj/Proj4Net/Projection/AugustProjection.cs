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

namespace Proj4Net.Projection
{

    public class AugustProjection : Projection
    {

        private const double M = 1.333333333333333;

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            double t, c1, c, x1, x12, y1, y12;

            t = Math.Tan(.5 * lpphi);
            c1 = Math.Sqrt(1.0 - t * t);
            c = 1.0 + c1 * Math.Cos(lplam *= .5);
            x1 = Math.Sin(lplam) * c1 / c;
            y1 = t / c;
            coord.X = M * x1 * (3.0 + (x12 = x1 * x1) - 3.0 * (y12 = y1 * y1));
            coord.Y = M * y1 * (3.0 + 3.0 * x12 - y12);
            return coord;
        }

        ///<summary>
        /// Returns true if this projection is conformal
        ///</summary>
        public override Boolean IsConformal
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "August Epicycloidal";
        }

    }
}
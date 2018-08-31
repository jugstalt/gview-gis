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

    public class CrasterProjection : Projection
    {

        private const double XM = 0.97720502380583984317;
        private const double RXM = 1.02332670794648848847;
        private const double YM = 3.06998012383946546542;
        private const double RYM = 0.32573500793527994772;
        private const double THIRD = 0.333333333333333333;

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            lpphi *= THIRD;
            coord.X = XM * lplam * (2.0 * Math.Cos(lpphi + lpphi) - 1.0);
            coord.Y = YM * Math.Sin(lpphi);
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            coord.Y = 3.0 * Math.Asin(xyy * RYM);
            coord.X = xyx * RXM / (2.0 * Math.Cos((coord.Y + coord.Y) * THIRD) - 1);
            return coord;
        }

        ///<summary>Returns true if this projection is equal area</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Craster Parabolic (Putnins P4)";
        }

    }
}
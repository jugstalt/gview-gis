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

    public class Eckert5Projection : Projection
    {

        private const double XF = 0.44101277172455148219;
        private const double RXF = 2.26750802723822639137;
        private const double YF = 0.88202554344910296438;
        private const double RYF = 1.13375401361911319568;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = XF * (1.0 + Math.Cos(lpphi)) * lplam;
            xy.Y = YF * lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.X = RXF * xyx / (1.0 + Math.Cos(lp.Y = RYF * xyy));
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Eckert V";
        }

    }
}
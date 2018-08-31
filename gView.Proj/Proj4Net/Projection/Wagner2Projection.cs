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
    public class Wagner2Projection : Projection
    {

        private const double C_x = 0.92483;
        private const double C_y = 1.38725;
        private const double C_p1 = 0.88022;
        private const double C_p2 = 0.88550;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.Y = ProjectionMath.Asin(C_p1 * Math.Sin(C_p2 * lpphi));
            xy.X = C_x * lplam * Math.Cos(lpphi);
            xy.Y = C_y * lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.Y = xyy / C_y;
            lp.X = xyx / (C_x * Math.Cos(lp.Y));
            lp.Y = ProjectionMath.Asin(Math.Sin(lp.Y) / C_p1) / C_p2;
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Wagner II";
        }
    }
}
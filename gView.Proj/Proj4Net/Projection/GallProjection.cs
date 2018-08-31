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
    public class GallProjection : Projection
    {

        private const double YF = 1.70710678118654752440;
        private const double XF = 0.70710678118654752440;
        private const double RYF = 0.58578643762690495119;
        private const double RXF = 1.41421356237309504880;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = XF * lplam;
            xy.Y = YF * Math.Tan(.5 * lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.X = RXF * xyx;
            lp.Y = 2.0 * Math.Atan(xyy * RYF);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Gall (Gall Stereographic)";
        }

    }
}
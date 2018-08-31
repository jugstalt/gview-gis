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

    public class DenoyerProjection : Projection
    {

        public const double C0 = 0.95;
        public const double C1 = -.08333333333333333333;
        public const double C3 = 0.00166666666666666666;
        public const double D1 = 0.9;
        public const double D5 = 0.03;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.Y = lpphi;
            xy.X = lplam;
            double aphi = Math.Abs(lplam);
            xy.X *= Math.Cos((C0 + aphi * (C1 + aphi * aphi * C3)) *
                (lpphi * (D1 + D5 * lpphi * lpphi * lpphi * lpphi)));
            return xy;
        }

        public override Boolean ParallelsAreParallel
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "Denoyer Semi-elliptical";
        }

    }
}
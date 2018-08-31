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

    public class Wagner3Projection : PseudoCylindricalProjection
    {

        private const double TwoThird = 0.6666666666666666666667;

        private double C_x;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = C_x * lplam * Math.Cos(TwoThird * lpphi);
            xy.Y = lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            lp.Y = y;
            lp.X = x / (C_x * Math.Cos(TwoThird * lp.Y));
            return lp;
        }

        public override void Initialize()
        {
            base.Initialize();
            C_x = Math.Cos(TrueScaleLatitude) / Math.Cos(2.0 * TrueScaleLatitude / 3.0);
            EccentricitySquared = 0.0;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Wagner III";
        }

    }
}
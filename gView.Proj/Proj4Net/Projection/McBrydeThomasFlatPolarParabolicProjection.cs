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

    public class McBrydeThomasFlatPolarParabolicProjection : Projection
    {

        private const double CS = .95257934441568037152;
        private const double FXC = .92582009977255146156;
        private const double FYC = 3.40168025708304504493;
        private const double C23 = .66666666666666666666;
        private const double C13 = .33333333333333333333;
        private const double ONEEPS = 1.0000001;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.Y = Math.Asin(CS * Math.Sin(lpphi));
            xy.X = FXC * lplam * (2.0 * Math.Cos(C23 * lpphi) - 1.0);
            xy.Y = FYC * Math.Sin(C13 * lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.Y = xyy / FYC;
            if (Math.Abs(lp.Y) >= 1.0)
            {
                if (Math.Abs(lp.Y) > ONEEPS) throw new ProjectionException("I");
                else lp.Y = (lp.Y < 0.0) ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            }
            else
                lp.Y = Math.Asin(lp.Y);
            lp.X = xyx / (FXC * (2.0 * Math.Cos(C23 * (lp.Y *= 3.0)) - 1.0));
            if (Math.Abs(lp.Y = Math.Sin(lp.Y) / CS) >= 1.0)
            {
                if (Math.Abs(lp.Y) > ONEEPS) throw new ProjectionException("I");
                else lp.Y = (lp.Y < 0.0) ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            }
            else
                lp.Y = Math.Asin(lp.Y);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "McBride-Thomas Flat-Polar Parabolic";
        }

    }
}
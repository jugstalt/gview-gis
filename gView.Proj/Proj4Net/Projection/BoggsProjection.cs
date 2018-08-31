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

    public class BoggsProjection : PseudoCylindricalProjection
    {

        private const int NITER = 20;
        private const double EPS = 1e-7;
        //private const double ONETOL = 1.000001;
        private const double FXC = 2.00276;
        private const double FXC2 = 1.11072;
        private const double FYC = 0.49931;
        private const double FYC2 = 1.41421356237309504880;

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            double theta, th1, c;
            int i;

            theta = lpphi;
            if (Math.Abs(Math.Abs(lpphi) - ProjectionMath.PiHalf) < EPS)
                coord.X = 0.0;
            else
            {
                c = Math.Sin(theta) * Math.PI;
                for (i = NITER; i > 0; --i)
                {
                    theta -= th1 = (theta + Math.Sin(theta) - c) /
                        (1.0 + Math.Cos(theta));
                    if (Math.Abs(th1) < EPS) break;
                }
                theta *= 0.5;
                coord.X = FXC * lplam / (1.0 / Math.Cos(lpphi) + FXC2 / Math.Cos(theta));
            }
            coord.Y = FYC * (lpphi + FYC2 * Math.Sin(theta));
            return coord;
        }

        ///<summary>Returns true if this projection is equal area</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "Boggs Eumorphic";
        }

    }
}
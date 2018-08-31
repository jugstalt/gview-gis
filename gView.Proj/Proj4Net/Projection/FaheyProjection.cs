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

    public class FaheyProjection : Projection
    {

        private const double Tolerance = 1e-6;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.Y = 1.819152 * (xy.X = Math.Tan(0.5 * lpphi));
            xy.X = 0.819152 * lplam * Asqrt(1 - xy.X * xy.X);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.Y = 2.0 * Math.Atan(lp.Y /= 1.819152);
            lp.X = Math.Abs(lp.Y = 1.0 - xyy * xyy) < Tolerance ? 0.0 :
                xyx / (0.819152 * Math.Sqrt(xyy));
            return lp;
        }

        private static double Asqrt(double v)
        {
            return (v <= 0) ? 0.0 : Math.Sqrt(v);
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Fahey";
        }

    }
}
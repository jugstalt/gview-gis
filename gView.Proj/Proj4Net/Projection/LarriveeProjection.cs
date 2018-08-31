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
    public class LarriveeProjection : Projection
    {

        private const double Sixth = .16666666666666666;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = 0.5 * lplam * (1.0 + Math.Sqrt(Math.Cos(lpphi)));
            xy.Y = lpphi / (Math.Cos(0.5 * lpphi) * Math.Cos(Sixth * lplam));
            return xy;
        }

        public override String ToString()
        {
            return "Larrivee";
        }

    }
}
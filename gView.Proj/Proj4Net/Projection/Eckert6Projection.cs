/*
Copyright 2011 Martin Davis

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
    public class Eckert6Projection : PseudoCylindricalProjection
    {

        private const double n = 2.570796326794896619231321691;
        private static readonly double C_y = Math.Sqrt((2)/n);
        private static readonly double C_x = C_y/2;
        private const int MAX_ITER = 8;
        private const double LOOP_TOL = 1e-7;

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            int i;
            var k = n*Math.Sin(phi);
            for (i = MAX_ITER; i > 0;)
            {
                var V = (phi + Math.Sin(phi) - k)/(1 + Math.Cos(phi));
                phi -= V;
                if (Math.Abs(V) < LOOP_TOL)
                {
                    break;
                }
                --i;
            }
            if (i == 0)
            {
                throw new ProjectionException("F_ERROR");
            }
            return xy;
        }
    }
}
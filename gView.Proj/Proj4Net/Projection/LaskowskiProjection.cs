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

    public class LaskowskiProjection : Projection
    {

        private const double a10 = 0.975534;
        private const double a12 = -0.119161;
        private const double a32 = -0.0143059;
        private const double a14 = -0.0547009;
        private const double b01 = 1.00384;
        private const double b21 = 0.0802894;
        private const double b03 = 0.0998909;
        private const double b41 = 0.000199025;
        private const double b23 = -0.0285500;
        private const double b05 = -0.0491032;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double l2, p2;

            l2 = lplam * lplam;
            p2 = lpphi * lpphi;
            xy.X = lplam * (a10 + p2 * (a12 + l2 * a32 + p2 * a14));
            xy.Y = lpphi * (b01 + l2 * (b21 + p2 * b23 + l2 * b41) +
                p2 * (b03 + p2 * b05));
            return xy;
        }

        public override String ToString()
        {
            return "Laskowski";
        }

    }
}
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

    public class CentralCylindricalProjection : CylindricalProjection
    {

        //private double _ap;

        //private final static double EPS10 = 1.e-10;

        public CentralCylindricalProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(-80);
            MaxLatitude = ProjectionMath.ToRadians(80);
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            if (Math.Abs(Math.Abs(lpphi) - ProjectionMath.PiHalf) <= EPS10)
                throw new ProjectionException("F");
            coord.X = lplam;
            coord.Y = Math.Tan(lpphi);
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            coord.Y = Math.Atan(xyy);
            coord.X = xyx;
            return coord;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Central Cylindrical";
        }

    }
}
using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
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

    public class TransverseCentralCylindricalProjection : CylindricalProjection
    {

        public TransverseCentralCylindricalProjection()
        {
            MinLongitude = ProjectionMath.ToRadians(-60);
            MaxLongitude = ProjectionMath.ToRadians(60);
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double b, bt;

            b = Math.Cos(lpphi) * Math.Sin(lplam);
            if ((bt = 1.0 - b * b) < EPS10)
                throw new ProjectionException("F");
            xy.X = b / Math.Sqrt(bt);
            xy.Y = Math.Atan2(Math.Tan(lpphi), Math.Cos(lplam));
            return xy;
        }

        public override Boolean IsRectilinear
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "Transverse Central Cylindrical";
        }

    }
}
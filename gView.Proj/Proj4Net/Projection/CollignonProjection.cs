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

    public class CollignonProjection : Projection
    {

        private const double FXC = 1.12837916709551257390;
        private const double FYC = 1.77245385090551602729;
        private const double ONEEPS = 1.0000001;

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            if ((coord.Y = 1.0 - Math.Sin(lpphi)) <= 0.0)
                coord.Y = 0.0;
            else
                coord.Y = Math.Sqrt(coord.Y);
            coord.X = FXC * lplam * coord.Y;
            coord.Y = FYC * (1.0 - coord.Y);
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            double lpphi = xyy / FYC - 1.0;
            if (Math.Abs(coord.Y = 1.0 - lpphi * lpphi) < 1.0)
                coord.Y = Math.Asin(lpphi);
            else if (Math.Abs(lpphi) > ONEEPS) throw new ProjectionException("I");
            else coord.Y = lpphi < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            if ((coord.X = 1.0 - Math.Sin(lpphi)) <= 0.0)
                coord.X = 0.0;
            else
                coord.X = xyx / (FXC * Math.Sqrt(coord.X));
            coord.Y = lpphi;
            return coord;
        }

        ///<summary>
        /// Returns true if this projection is equal area
        ///</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get
            {
                return true;
            }
        }

        public override String ToString()
        {
            return "Collignon";
        }

    }
}
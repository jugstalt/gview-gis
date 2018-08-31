using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

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
namespace Proj4Net.Projection
{


    public class PutninsP4Projection : Projection
    {

        protected double C_x;
        protected double C_y;

        public PutninsP4Projection()
        {
            C_x = 0.874038744;
            C_y = 3.883251825;
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            lpphi = ProjectionMath.Asin(0.883883476 * Math.Sin(lpphi));
            xy.X = C_x * lplam * Math.Cos(lpphi);
            xy.X /= Math.Cos(lpphi *= 0.333333333333333);
            xy.Y = C_y * Math.Sin(lpphi);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.Y = ProjectionMath.Asin(xyy / C_y);
            lp.X = xyx * Math.Cos(lp.Y) / C_x;
            lp.Y *= 3.0;
            lp.X /= Math.Cos(lp.Y);
            lp.Y = ProjectionMath.Asin(1.13137085 * Math.Sin(lp.Y));
            return lp;
        }

        ///<summary>Returns true if this projection is equal area</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Putnins P4";
        }

    }
}
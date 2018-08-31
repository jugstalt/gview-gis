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


    public class TransverseCylindricalEqualArea : Projection
    {

        private double rk0;

        public TransverseCylindricalEqualArea()
        {
            Initialize();
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = rk0 * Math.Cos(lpphi) * Math.Sin(lplam);
            xy.Y = ScaleFactor * (Math.Atan2(Math.Tan(lpphi), Math.Cos(lplam)) - ProjectionLatitude);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double t;

            lp.Y = xyy * rk0 + ProjectionLatitude;
            lp.X *= ScaleFactor;
            t = Math.Sqrt(1d - xyx * xyx);
            lp.Y = Math.Asin(t * Math.Sin(xyy));
            lp.X = Math.Atan2(xyx, t * Math.Cos(xyy));
            return lp;
        }

        public override void Initialize()
        { // tcea
            base.Initialize();
            rk0 = 1 / ScaleFactor;
        }

        public override Boolean IsRectilinear
        {
            get { return false; }
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
            return "Transverse Cylindrical Equal Area";
        }

    }
}
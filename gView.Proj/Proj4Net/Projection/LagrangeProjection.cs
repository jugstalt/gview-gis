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

    public class LagrangeProjection : Projection
    {

        // Parameters
        private double hrw;
        private double rw = 1.4;
        private double a1;
        private double phi1;

        private const double Tolerance = 1e-10;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double v, c;

            if (Math.Abs(Math.Abs(lpphi) - ProjectionMath.PiHalf) < Tolerance)
            {
                xy.X = 0;
                xy.Y = lpphi < 0 ? -2.0 : 2.0;
            }
            else
            {
                lpphi = Math.Sin(lpphi);
                v = a1 * Math.Pow((1.0 + lpphi) / (1.0 - lpphi), hrw);
                if ((c = 0.5 * (v + 1.0 / v) + Math.Cos(lplam *= rw)) < Tolerance)
                    throw new ProjectionException();
                xy.X = 2.0 * Math.Sin(lplam) / c;
                xy.Y = (v - 1.0 / v) / c;
            }
            return xy;
        }

        public double W
        {
            get { return rw; }
            set { rw = value; }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (rw <= 0)
                throw new ProjectionException("-27");
            hrw = 0.5 * (rw = 1.0 / rw);
            phi1 = ProjectionLatitude1;
            if (Math.Abs(Math.Abs(phi1 = Math.Sin(phi1)) - 1.0) < Tolerance)
                throw new ProjectionException("-22");
            a1 = Math.Pow((1.0 - phi1) / (1.0 + phi1), hrw);
        }

        ///<summary>Returns true if this projection is conformal</summary>
        public override Boolean IsConformal
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "Lagrange";
        }

    }
}
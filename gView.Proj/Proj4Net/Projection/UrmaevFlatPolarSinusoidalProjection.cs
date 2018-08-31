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

    public class UrmaevFlatPolarSinusoidalProjection : Projection
    {

        private const double C_x = 0.8773826753;
        private const double Cy = 1.139753528477;

        private double n = 0.8660254037844386467637231707;// wag1
        private double C_y;

        public UrmaevFlatPolarSinusoidalProjection()
        {
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.Y = ProjectionMath.Asin(n * Math.Sin(lpphi));
            xy.X = C_x * lplam * Math.Cos(lpphi);
            xy.Y = C_y * lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            xyy /= C_y;
            lp.Y = ProjectionMath.Asin(Math.Sin(xyy) / n);
            lp.X = xyx / (C_x * Math.Cos(xyy));
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override void Initialize()
        { // urmfps
            base.Initialize();
            if (n <= 0.0 || n > 1.0)
                throw new ProjectionException("-40");
            C_y = Cy / n;
        }

        // Properties
        public double N
        {
            get
            {
                return n;
            }
            set
            {
                n = value;
            }
        }
        public override String ToString()
        {
            return "Urmaev Flat-Polar Sinusoidal";
        }

    }
}
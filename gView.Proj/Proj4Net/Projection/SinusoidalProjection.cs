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

    public class SinusoidalProjection : PseudoCylindricalProjection
    {

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            xy.X = lam * Math.Cos(phi);
            xy.Y = phi;
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            lp.X = x / Math.Cos(y);
            lp.Y = y;
            return lp;
        }

        public double GetWidth(double y)
        {
            return ProjectionMath.NormalizeLongitude(Math.PI) * Math.Cos(y);
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if this projection is equal area
        /// </summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Sinusoidal";
        }

    }
}
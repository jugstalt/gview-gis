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


    public class HammerProjection : PseudoCylindricalProjection
    {

        private double _w = 0.5;
        private double _m = 1;
        private double _rm;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double cosphi, d;

            d = Math.Sqrt(2.0 / (1.0 + (cosphi = Math.Cos(lpphi)) * Math.Cos(lplam *= _w)));
            xy.X = _m * d * cosphi * Math.Sin(lplam);
            xy.Y = _rm * d * Math.Sin(lpphi);
            return xy;
        }

        public override void Initialize()
        {
            base.Initialize();
            if ((_w = Math.Abs(_w)) <= 0.0)
                throw new ProjectionException("-27");
            else
                _w = 0.5;
            if ((_m = Math.Abs(_m)) <= 0.0)
                throw new ProjectionException("-27");
            else
                _m = 1.0;
            _rm = 1.0 / _m;
            _m /= _w;
            EccentricitySquared = 0.0;
        }

        // Properties

        public double W
        {
            get { return _w; }
            set { _w = value; }
        }

        public double M
        {
            get { return _m; }
            set
            {
                _m = value;
                _rm = 1d / _m;
            }
        }

        ///<summary>
        /// Gets if this projection is equal area</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Hammer & Eckert-Greifendorff";
        }

    }
}
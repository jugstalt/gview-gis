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

    /*
     * This file was semi-automatically converted from the public-domain USGS PROJ source.
     */
using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    public class MercatorProjection : CylindricalProjection
    {

        public MercatorProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(-85);
            MaxLatitude = ProjectionMath.ToRadians(85);
        }

        public override Coordinate Project(double lam, double phi, Coordinate coord)
        {
            if (_spherical)
            {
                coord.X = ScaleFactor * lam;
                coord.Y = ScaleFactor * Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5 * phi));
            }
            else
            {
                coord.X = ScaleFactor * lam;
                coord.Y = -ScaleFactor * Math.Log(ProjectionMath.tsfn(phi, Math.Sin(phi), _e));
            }
            return coord;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate coord)
        {
            if (_spherical)
            {
                coord.Y = ProjectionMath.PiHalf - 2.0 * Math.Atan(Math.Exp(-y / ScaleFactor));
                coord.X = x / ScaleFactor;
            }
            else
            {
                coord.Y = ProjectionMath.Phi2(Math.Exp(-y / ScaleFactor), _e);
                coord.X = x / ScaleFactor;
            }
            return coord;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override Boolean IsRectilinear
        {
            get { return true; }
        }

        /**
         * Returns the ESPG code for this projection, or 0 if unknown.
         */
        public int EPSGCode
        {
            get { return 9804; }
        }

        public override String ToString()
        {
            return "Mercator";
        }

    }
}
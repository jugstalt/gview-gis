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


    public class Eckert1Projection : Projection
    {

        private const double FC = .92131773192356127802;
        private const double RP = .31830988618379067154;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            xy.X = FC * lplam * (1.0 - RP * Math.Abs(lpphi));
            xy.Y = FC * lpphi;
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            lp.Y = xyy / FC;
            lp.X = xyx / (FC * (1.0 - RP * Math.Abs(lp.Y)));
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Eckert I";
        }

    }
}
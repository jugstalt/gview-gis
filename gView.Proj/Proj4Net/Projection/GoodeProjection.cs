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

    public class GoodeProjection : Projection
    {

        private const double Y_COR = 0.05280;
        private const double PHI_LIM = .71093078197902358062;

        private readonly SinusoidalProjection _sinu = new SinusoidalProjection();
        private readonly MolleweideProjection _moll = new MolleweideProjection();

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            if (Math.Abs(lpphi) <= PHI_LIM)
                xy = _sinu.Project(lplam, lpphi, xy);
            else
            {
                xy = _moll.Project(lplam, lpphi, xy);
                xy.Y -= lpphi >= 0.0 ? Y_COR : -Y_COR;
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            if (Math.Abs(xyy) <= PHI_LIM)
                lp = _sinu.ProjectInverse(xyx, xyy, lp);
            else
            {
                xyy += xyy >= 0.0 ? Y_COR : -Y_COR;
                lp = _moll.ProjectInverse(xyx, xyy, lp);
            }
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Goode Homolosine";
        }

    }
}
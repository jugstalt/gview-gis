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

    public class Wagner7Projection : Projection
    {

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double theta, ct, D;

            theta = Math.Asin(xy.Y = 0.90630778703664996 * Math.Sin(lpphi));
            xy.X = 2.66723 * (ct = Math.Cos(theta)) * Math.Sin(lplam /= 3.0);
            xy.Y *= 1.24104 * (D = 1 / (Math.Sqrt(0.5 * (1 + ct * Math.Cos(lplam)))));
            xy.X *= D;
            return xy;
        }

        ///<summary> Returns true if this projection is equal area</summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Wagner VII";
        }

    }
}
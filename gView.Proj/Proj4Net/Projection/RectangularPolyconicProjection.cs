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

    public class RectangularPolyconicProjection : Projection
    {

        private double phi0;
        private double phi1;
        private double fxa;
        private double fxb;
        private Boolean mode;

        private const double EPS = 1e-9;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double fa;

            if (mode)
                fa = Math.Tan(lplam * fxb) * fxa;
            else
                fa = 0.5 * lplam;
            if (Math.Abs(lpphi) < EPS)
            {
                xy.Y = fa + fa;
                xy.Y = -phi0;
            }
            else
            {
                xy.Y = 1.0 / Math.Tan(lpphi);
                xy.X = Math.Sin(fa = 2.0 * Math.Atan(fa * Math.Sin(lpphi))) * xy.Y;
                xy.Y = lpphi - phi0 + (1.0 - Math.Cos(fa)) * xy.Y;
            }
            return xy;
        }

        public override void Initialize()
        { // rpoly
            base.Initialize();
            /*FIXME
                    if ((mode = (phi1 = Math.abs(pj_param(params, "rlat_ts").f)) > EPS)) {
                        fxb = 0.5 * Math.sin(phi1);
                        fxa = 0.5 / fxb;
                    }
            */
        }

        public override String ToString()
        {
            return "Rectangular Polyconic";
        }

    }
}
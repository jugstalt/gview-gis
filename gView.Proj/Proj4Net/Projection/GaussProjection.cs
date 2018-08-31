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
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    public class GaussProjection : Projection
    {
        private const int MAX_ITER = 20;
        private const double DEL_TOL = 1.0e-14;

        private double C;
        private double K;
        //private double e;
        protected double rc;
        protected double phic0;
        private double ratexp;

        public GaussProjection()
        {
            //Initialize();
        }

        public override GeoAPI.Geometries.Coordinate Project(double x, double y, GeoAPI.Geometries.Coordinate dst)
        {
            dst.Y = 2.0*Math.Atan(K*Math.Pow(Math.Tan(0.5*y + ProjectionMath.PiFourth), C)*srat(_e*Math.Sin(y), ratexp)) -
                    ProjectionMath.PiHalf;
            dst.X = C*x;
            return dst;
        }

        public override GeoAPI.Geometries.Coordinate Project(GeoAPI.Geometries.Coordinate src,
                                                              GeoAPI.Geometries.Coordinate dst)
        {
            return Project(src.X, src.Y, dst);
        }

        public override GeoAPI.Geometries.Coordinate ProjectInverse(double x, double y,
                                                                     GeoAPI.Geometries.Coordinate dst)
        {
            var lon = x/C;
            var lat = y;
            var num = Math.Pow(Math.Tan(0.5*lat + ProjectionMath.PiFourth)/K, 1.0/C);
            int i;
            for (i = MAX_ITER; i > 0; --i)
            {
                lat = 2.0 * Math.Atan(num * srat(_e * Math.Sin(y), -0.5 * _e)) - ProjectionMath.PiHalf;
                if (Math.Abs(lat - y) < DEL_TOL) break;
                y = lat;
            }
            /* convergence failed */
            if (i <= 0)
            {
                throw new ProjectionException(this, Resources.Error.Err_17);
            }
            dst.X = lon;
            dst.Y = lat;
            return dst;
        }

        public override void Initialize()
        {
            base.Initialize();
            var sphi = Math.Sin(_projectionLatitude);
            var cphi = Math.Cos(_projectionLatitude);
            cphi *= cphi;
            rc = Math.Sqrt(1.0 - _es)/(1.0 - _es*sphi*sphi);
            C = Math.Sqrt(1.0 + _es*cphi*cphi/(1.0 - _es));
            phic0 = Math.Asin(sphi/C);
            ratexp = 0.5 * C * _e;
            K = Math.Tan(0.5*phic0 + ProjectionMath.PiFourth)/
                (Math.Pow(Math.Tan(0.5 * _projectionLatitude + ProjectionMath.PiFourth), C) * srat(_e * sphi, ratexp));
        }

        private static double srat(double esinp, double exp)
        {
            return Math.Pow((1.0 - esinp)/(1.0 + esinp), exp);
        }

        public override bool HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Gauss";
        }
    }
}
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

    public class CassiniProjection : Projection
    {

        private double _m0;
        private double _n;
        private double _t;
        private double _a1;
        private double _c;
        private double _r;
        private double _dd;
        private double _d2;
        private double _a2;
        private double _tn;
        private double[] _en;

        //private const double EPS10 = 1e-10;
        private const double C1 = .16666666666666666666;
        private const double C2 = .00833333333333333333;
        private const double C3 = .04166666666666666666;
        private const double C4 = .33333333333333333333;
        private const double C5 = .06666666666666666666;

        public CassiniProjection()
        {
            ProjectionLatitude = ProjectionMath.ToRadians(0);
            ProjectionLongitude = ProjectionMath.ToRadians(0);
            MinLongitude = ProjectionMath.ToRadians(-90);
            MaxLongitude = ProjectionMath.ToRadians(90);
            Initialize();
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            if (_spherical)
            {
                xy.X = Math.Asin(Math.Cos(lpphi) * Math.Sin(lplam));
                xy.Y = Math.Atan2(Math.Tan(lpphi), Math.Cos(lplam)) - ProjectionLatitude;
            }
            else
            {
                xy.Y = ProjectionMath.mlfn(lpphi, _n = Math.Sin(lpphi), _c = Math.Cos(lpphi), _en);
                _n = 1.0 / Math.Sqrt(1.0 - _es * _n * _n);
                _tn = Math.Tan(lpphi); _t = _tn * _tn;
                _a1 = lplam * _c;
                _c *= _es * _c / (1 - _es);
                _a2 = _a1 * _a1;
                xy.X = _n * _a1 * (1.0 - _a2 * _t *
                    (C1 - (8.0 - _t + 8.0 * _c) * _a2 * C2));
                xy.Y -= _m0 - _n * _tn * _a2 *
                    (.5 + (5.0 - _t + 6.0 * _c) * _a2 * C3);
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            if (_spherical)
            {
                coord.Y = Math.Asin(Math.Asin(_dd = xyy + ProjectionLatitude) * Math.Cos(xyx));
                coord.X = Math.Atan2(Math.Tan(xyx), Math.Cos(_dd));
            }
            else
            {
                double ph1;

                ph1 = ProjectionMath.inv_mlfn(_m0 + xyy, _es, _en);
                _tn = Math.Tan(ph1); _t = _tn * _tn;
                _n = Math.Sin(ph1);
                _r = 1.0 / (1.0 - _es * _n * _n);
                _n = Math.Sqrt(_r);
                _r *= (1.0 - _es) * _n;
                _dd = xyx / _n;
                _d2 = _dd * _dd;
                coord.Y = ph1 - (_n * _tn / _r) * _d2 *
                    (.5 - (1.0 + 3.0 * _t) * _d2 * C3);
                coord.X = _dd * (1.0 + _t * _d2 *
                    (-C4 + (1.0 + 3.0 * _t) * _d2 * C5)) / Math.Cos(ph1);
            }
            return coord;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!_spherical)
            {
                if ((_en = ProjectionMath.enfn(_es)) == null)
                    throw new ProjectionException();
                _m0 = ProjectionMath.mlfn(ProjectionLatitude, Math.Sin(ProjectionLatitude), Math.Cos(ProjectionLatitude), _en);
            }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        /**
         * Returns the ESPG code for this projection, or 0 if unknown.
         */
        public int EPSGCode
        {
            get { return 9806; }
        }

        public override String ToString()
        {
            return "Cassini";
        }

    }
}
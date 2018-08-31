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

    public class BonneProjection : Projection
    {
        private double _phi1;
        private double _cphi1;
        private double _am1;
        private double _m1;
        private double[] _en;

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            if (_spherical)
            {
                double E, rh;

                rh = _cphi1 + _phi1 - lpphi;
                if (Math.Abs(rh) > EPS10)
                {
                    coord.X = rh * Math.Sin(E = lplam * Math.Cos(lpphi) / rh);
                    coord.Y = _cphi1 - rh * Math.Cos(E);
                }
                else
                    coord.X = coord.Y = 0.0;
            }
            else
            {
                double E = Math.Sin(lpphi), c = Math.Cos(lpphi);

                double rh = _am1 + _m1 - ProjectionMath.mlfn(lpphi, E , c , _en);
                E = c * lplam / (rh * Math.Sqrt(1.0 - _es * E * E));
                coord.X = rh * Math.Sin(E);
                coord.Y = _am1 - rh * Math.Cos(E);
            }
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            if (_spherical)
            {
                double rh = ProjectionMath.Distance(xyx, coord.Y = _cphi1 - xyy);
                coord.Y = _cphi1 + _phi1 - rh;
                if (Math.Abs(coord.Y) > ProjectionMath.PiHalf) throw new ProjectionException("I");
                if (Math.Abs(Math.Abs(coord.Y) - ProjectionMath.PiHalf) <= EPS10)
                    coord.X = 0.0;
                else
                    coord.X = rh * Math.Atan2(xyx, xyy) / Math.Cos(coord.Y);
            }
            else
            {
                double s;

                double rh = ProjectionMath.Distance(xyx, coord.Y = _am1 - xyy);
                coord.Y = ProjectionMath.inv_mlfn(_am1 + _m1 - rh, _es, _en);
                if ((s = Math.Abs(coord.Y)) < ProjectionMath.PiHalf)
                {
                    s = Math.Sin(coord.Y);
                    coord.X = rh * Math.Atan2(xyx, xyy) *
                       Math.Sqrt(1.0 - _es * s * s) / Math.Cos(coord.Y);
                }
                else if (Math.Abs(s - ProjectionMath.PiHalf) <= EPS10)
                    coord.X = 0.0;
                else throw new ProjectionException("I");
            }
            return coord;
        }

        /**
         * Returns true if this projection is equal area
         */
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override void Initialize()
        {
            base.Initialize();

            double c;

            //_phi1 = pj_param(params, "rlat_1").f;
            _phi1 = ProjectionMath.PiHalf;
            if (Math.Abs(_phi1) < EPS10)
                throw new ProjectionException("-23");
            if (!_spherical)
            {
                _en = ProjectionMath.enfn(_es);
                _m1 = ProjectionMath.mlfn(_phi1, _am1 = Math.Sin(_phi1),
                    c = Math.Cos(_phi1), _en);
                _am1 = c / (Math.Sqrt(1.0 - _es * _am1 * _am1) * _am1);
            }
            else
            {
                if (Math.Abs(_phi1) + EPS10 >= ProjectionMath.PiHalf)
                    _cphi1 = 0.0;
                else
                    _cphi1 = 1.0 / Math.Tan(_phi1);
            }
        }

        public override String ToString()
        {
            return "Bonne";
        }

    }
}
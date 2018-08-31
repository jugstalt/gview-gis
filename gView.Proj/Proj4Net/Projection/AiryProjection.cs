using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
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

    public class AiryProjection : Projection
    {

        private double _poleHalfPi;
        private double _sinph0;
        private double _cosph0;
        private double _cb;
        private int _mode;
        private Boolean _noCut = true;	/* do not cut at hemisphere limit */

// ReSharper disable InconsistentNaming
        private const double EPS = 1.0e-10;
        private const int N_POLE = 0;
        private const int S_POLE = 1;
        private const int EQUIT = 2;
        private const int OBLIQ = 3;
// ReSharper restore InconsistentNaming

        public AiryProjection()
        {
            _minLatitude = ProjectionMath.ToRadians(-60);
            _maxLatitude = ProjectionMath.ToRadians(60);
            _minLongitude = ProjectionMath.ToRadians(-90);
            _maxLongitude = ProjectionMath.ToRadians(90);
            Initialize();
        }

        protected new void CopyParams(Projection to)
        {
            AiryProjection toAiry = to as AiryProjection;
            if ( toAiry == null )
                throw new ArgumentException("to");

            base.CopyParams(to);

            toAiry._cosph0 = _cosph0;
            toAiry._poleHalfPi = _poleHalfPi;
            toAiry._sinph0 = _sinph0;
            toAiry._cosph0 = _cosph0;
            toAiry._cb = _cb;
            toAiry._mode = _mode;
            toAiry._noCut = _noCut;

        }

        public override object Clone()
        {
            AiryProjection clone = new AiryProjection();
            CopyParams(clone);
            return clone;
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            double cosphi, sinphi, t, s, Krho, cosz;

            double sinlam = Math.Sin(lplam);
            double coslam = Math.Cos(lplam);

            switch (_mode)
            {
                case EQUIT:
                case OBLIQ:
                    sinphi = Math.Sin(lpphi);
                    cosphi = Math.Cos(lpphi);
                    cosz = cosphi*coslam;
                    if (_mode == OBLIQ)
                        cosz = _sinph0*sinphi + _cosph0*cosz;
                    if (!_noCut && cosz < -EPS)
                        throw new ProjectionException("F");
                    s = 1.0 - cosz;
                    if (Math.Abs(s) > EPS)
                    {
                        t = 0.5*(1.0 + cosz);
                        Krho = -Math.Log(t)/s - _cb/t;
                    }
                    else
                        Krho = 0.5 - _cb;
                    coord.X = Krho*cosphi*sinlam;
                    if (_mode == OBLIQ)
                        coord.Y = Krho*(_cosph0*sinphi -
                                        _sinph0*cosphi*coslam);
                    else
                        coord.Y = Krho*sinphi;
                    break;
                case S_POLE:
                case N_POLE:
                    coord.Y = Math.Abs(_poleHalfPi - lpphi);
                    if (!_noCut && (lpphi - EPS) > ProjectionMath.PiHalf)
                        throw new ProjectionException("F");
                    if ((coord.Y *= 0.5) > EPS)
                    {
                        t = Math.Tan(lpphi);
                        Krho = -2.0*(Math.Log(Math.Cos(lpphi))/t + t*_cb);
                        coord.X = Krho*sinlam;
                        coord.Y = Krho*coslam;
                        if (_mode == N_POLE)
                            coord.Y = -coord.Y;
                    }
                    else
                        coord.X = coord.Y = 0.0;
                    break;
            }
            return coord;
        }

        public override void Initialize()
        { // airy
            base.Initialize();

            //		_noCut = pj_param(params, "bno_cut").i;
            //		beta = 0.5 * (MapMath.HALFPI - pj_param(params, "rlat_b").f);
            _noCut = false;//FIXME
            double beta = 0.5 * (ProjectionMath.PiHalf - 0);
            if (Math.Abs(beta) < EPS)
                _cb = -0.5;
            else
            {
                _cb = 1.0 / Math.Tan(beta);
                _cb *= _cb * Math.Log(Math.Cos(beta));
            }
            if (Math.Abs(Math.Abs(_projectionLatitude) - ProjectionMath.PiHalf) < EPS)
                if (_projectionLatitude < 0.0)
                {
                    _poleHalfPi = -ProjectionMath.PiHalf;
                    _mode = S_POLE;
                }
                else
                {
                    _poleHalfPi = ProjectionMath.PiHalf;
                    _mode = N_POLE;
                }
            else
            {
                if (Math.Abs(_projectionLatitude) < EPS)
                    _mode = EQUIT;
                else
                {
                    _mode = OBLIQ;
                    _sinph0 = Math.Sin(_projectionLatitude);
                    _cosph0 = Math.Cos(_projectionLatitude);
                }
            }
        }

        public override String ToString()
        {
            return "Airy";
        }

    }
}
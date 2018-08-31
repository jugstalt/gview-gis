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

    public class AlbersProjection : Projection
    {

        //private const double EPS10 = 1.e-10;
        private const double Tolerance7 = 1.0E-7;
        private double _ec;
        private double _n;
        private double _c;
        private double _dd;
        private double _n2;
        private double _rho0;
        private double _phi1;
        private double _phi2;
        private double[] _en;

        private const int NumIterations = 15;
        private const double Epsilon = 1.0e-7;
        private const double Tolerance = 1.0e-10;

        //protected double projectionLatitude1 = MapMath.degToRad(45.5);
        //protected double projectionLatitude2 = MapMath.degToRad(29.5);

        public AlbersProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(80);
            ProjectionLatitude1 = ProjectionMath.ToRadians(45.5);
            ProjectionLatitude2 = ProjectionMath.ToRadians(29.5);
            Initialize();
        }

        private static double phi1_(double qs, double Te, double Tone_es)
        {
            int i;
            double Phi, sinpi, cospi, con, com, dphi;

            Phi = Math.Asin(0.5 * qs);
            if (Te < Epsilon)
                return (Phi);
            i = NumIterations;
            do
            {
                sinpi = Math.Sin(Phi);
                cospi = Math.Cos(Phi);
                con = Te * sinpi;
                com = 1.0 - con * con;
                dphi = .5 * com * com / cospi * (qs / Tone_es -
                   sinpi / com + .5 / Te * Math.Log((1.0 - con) /
                   (1.0 + con)));
                Phi += dphi;
            } while (Math.Abs(dphi) > Tolerance && --i != 0);
            return (i != 0 ? Phi : Double.MaxValue);
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
        {
            double rho;
            if ((rho = _c - (!_spherical ? _n * ProjectionMath.Qsfn(Math.Sin(lpphi), _e, _oneEs) : _n2 * Math.Sin(lpphi))) < 0.0)
                throw new ProjectionException("F");
            rho = _dd * Math.Sqrt(rho);
            coord.X = rho * Math.Sin(lplam *= _n);
            coord.Y = _rho0 - rho * Math.Cos(lplam);
            return coord;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate coord)
        {
            double rho;
            if ((rho = ProjectionMath.Distance(xyx, xyy = _rho0 - xyy)) != 0)
            {
                double lpphi, lplam;
                if (_n < 0.0)
                {
                    rho = -rho;
                    xyx = -xyx;
                    xyy = -xyy;
                }
                lpphi = rho / _dd;
                if (!_spherical)
                {
                    lpphi = (_c - lpphi * lpphi) / _n;
                    if (Math.Abs(_ec - Math.Abs(lpphi)) > Tolerance7)
                    {
                        if ((lpphi = phi1_(lpphi, _e, _oneEs)) == Double.MaxValue)
                            throw new ProjectionException("I");
                    }
                    else
                        lpphi = lpphi < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
                }
                else if (Math.Abs(coord.Y = (_c - lpphi * lpphi) / _n2) <= 1.0)
                    lpphi = Math.Asin(lpphi);
                else
                    lpphi = lpphi < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
                lplam = Math.Atan2(xyx, xyy) / _n;
                coord.X = lplam;
                coord.Y = lpphi;
            }
            else
            {
                coord.X = 0.0;
                coord.Y = _n > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
            }
            return coord;
        }

        public override void Initialize()
        {
            base.Initialize();
            double cosphi, sinphi;
            Boolean secant;

            _phi1 = ProjectionLatitude1;
            _phi2 = ProjectionLatitude2;

            if (Math.Abs(_phi1 + _phi2) < EPS10)
                throw new ProjectionException("-21");
            _n = sinphi = Math.Sin(_phi1);
            cosphi = Math.Cos(_phi1);
            secant = Math.Abs(_phi1 - _phi2) >= EPS10;
            //spherical = es > 0.0;
            if (!_spherical)
            {
                double ml1, m1;

                if ((_en = ProjectionMath.enfn(_es)) == null)
                    throw new ProjectionException("0");
                m1 = ProjectionMath.msfn(sinphi, cosphi, _es);
                ml1 = ProjectionMath.Qsfn(sinphi, _e, _oneEs);
                if (secant)
                {
                    /* secant cone */
                    double ml2, m2;

                    sinphi = Math.Sin(_phi2);
                    cosphi = Math.Cos(_phi2);
                    m2 = ProjectionMath.msfn(sinphi, cosphi, _es);
                    ml2 = ProjectionMath.Qsfn(sinphi, _e, _oneEs);
                    _n = (m1 * m1 - m2 * m2) / (ml2 - ml1);
                }
                _ec = 1.0 - .5 * _oneEs * Math.Log((1.0 - _e) /
                                               (1.0 + _e)) / _e;
                _c = m1 * m1 + _n * ml1;
                _dd = 1.0 / _n;
                _rho0 = _dd * Math.Sqrt(_c - _n * ProjectionMath.Qsfn(Math.Sin(ProjectionLatitude),
                                                                  _e, _oneEs));
            }
            else
            {
                if (secant) _n = .5 * (_n + Math.Sin(_phi2));
                _n2 = _n + _n;
                _c = cosphi * cosphi + _n2 * sinphi;
                _dd = 1.0 / _n;
                _rho0 = _dd * Math.Sqrt(_c - _n2 * Math.Sin(ProjectionLatitude));
            }
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

        /**
         * Returns the ESPG code for this projection, or 0 if unknown.
         */
        public int EPSGCode
        {
            get { return 9822; }
        }

        public override String ToString()
        {
            return "Albers Equal Area";
        }
    }

}
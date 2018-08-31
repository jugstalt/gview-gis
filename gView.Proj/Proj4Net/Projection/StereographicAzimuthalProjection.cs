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
using Proj4Net;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{

    public class StereographicAzimuthalProjection : AzimuthalProjection
    {

        private const double Tolerance = 1.0e-8;

        private double _akm1;

        public StereographicAzimuthalProjection() : this(ProjectionMath.ToRadians(90.0), ProjectionMath.ToRadians(0.0))
        {

        }

        public StereographicAzimuthalProjection(double projectionLatitude, double projectionLongitude)
            : base(projectionLatitude, projectionLongitude)
        {
            Initialize();
        }

        public void SetupUPS(AzimuthalMode pole)
        {
            ProjectionLatitude = (pole == AzimuthalMode.SouthPole) ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            ProjectionLongitude = 0.0;
            ScaleFactor = 0.994;
            FalseEasting = 2000000.0;
            FalseNorthing = 2000000.0;
            TrueScaleLatitude = ProjectionMath.PiHalf;
            Initialize();
        }

        public override void Initialize()
        {
            double t;

            base.Initialize();
            if (Math.Abs((t = Math.Abs(ProjectionLatitude)) - ProjectionMath.PiHalf) < EPS10)
                _mode = ProjectionLatitude < 0.0 ? AzimuthalMode.SouthPole : AzimuthalMode.NorthPole;
            else
                _mode = t > EPS10 ? AzimuthalMode.Oblique : AzimuthalMode.Equator;
            TrueScaleLatitude = Math.Abs(TrueScaleLatitude);
            if (! Spherical)
            {
                double X;

                switch (Mode)
                {
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Math.Abs(TrueScaleLatitude - ProjectionMath.PiHalf) < EPS10)
                            _akm1 = 2.0*ScaleFactor/
                                    Math.Sqrt(Math.Pow(1 + Eccentricity, 1 + Eccentricity)*
                                              Math.Pow(1 - Eccentricity, 1 - Eccentricity));
                        else
                        {
                            _akm1 = Math.Cos(TrueScaleLatitude)/
                                    ProjectionMath.tsfn(TrueScaleLatitude, t = Math.Sin(TrueScaleLatitude), Eccentricity);
                            t *= Eccentricity;
                            _akm1 /= Math.Sqrt(1.0 - t*t);
                        }
                        break;
                    case AzimuthalMode.Equator:
                        _akm1 = 2.0*ScaleFactor;
                        break;
                    case AzimuthalMode.Oblique:
                        t = Math.Sin(ProjectionLatitude);
                        X = 2.0*Math.Atan(ssfn(ProjectionLatitude, t, Eccentricity)) - ProjectionMath.PiHalf;
                        t *= Eccentricity;
                        _akm1 = 2.0*ScaleFactor*Math.Cos(ProjectionLatitude)/Math.Sqrt(1.0 - t*t);
                        _sinphi0 = Math.Sin(X);
                        _cosphi0 = Math.Cos(X);
                        break;
                }
            }
            else
            {
                switch (Mode)
                {
                    case AzimuthalMode.Oblique:
                    case AzimuthalMode.Equator:
                        if (Mode == AzimuthalMode.Oblique)
                        {
                            _sinphi0 = Math.Sin(ProjectionLatitude);
                            _cosphi0 = Math.Cos(ProjectionLatitude);
                        }
                        _akm1 = 2.0*ScaleFactor;
                        break;
                    case AzimuthalMode.SouthPole:
                    case AzimuthalMode.NorthPole:
                        _akm1 = Math.Abs(TrueScaleLatitude - ProjectionMath.PiHalf) >= EPS10
                                    ? Math.Cos(TrueScaleLatitude)/
                                      Math.Tan(ProjectionMath.PiFourth - .5*TrueScaleLatitude)
                                    : 2.0*ScaleFactor;
                        break;
                }
            }
        }

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            double coslam = Math.Cos(lam);
            double sinlam = Math.Sin(lam);
            double sinphi = Math.Sin(phi);

            if (Spherical)
            {
                double cosphi = Math.Cos(phi);

                switch (Mode)
                {
                    case AzimuthalMode.Equator:
                        xy.Y = 1.0 + cosphi*coslam;
                        if (xy.Y <= EPS10)
                            throw new ProjectionException();
                        xy.X = (xy.Y = _akm1/xy.Y)*cosphi*sinlam;
                        xy.Y *= sinphi;
                        break;
                    case AzimuthalMode.Oblique:
                        xy.Y = 1.0 + _sinphi0*sinphi + _cosphi0*cosphi*coslam;
                        if (xy.Y <= EPS10)
                            throw new ProjectionException();
                        xy.X = (xy.Y = _akm1/xy.Y)*cosphi*sinlam;
                        xy.Y *= _cosphi0*sinphi - _sinphi0*cosphi*coslam;
                        break;
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Mode == AzimuthalMode.NorthPole)
                        {
                            coslam = - coslam;
                            phi = - phi;
                        }
                        if (Math.Abs(phi - ProjectionMath.PiHalf) < Tolerance)
                            throw new ProjectionException();
                        xy.X = sinlam*(xy.Y = _akm1*Math.Tan(ProjectionMath.PiFourth + .5*phi));
                        xy.Y *= coslam;
                        break;
                }
            }
            else
            {
                double sinX = 0, cosX = 0, X, A;

                if (Mode == AzimuthalMode.Oblique || Mode == AzimuthalMode.Equator)
                {
                    sinX = Math.Sin(X = 2.0*Math.Atan(ssfn(phi, sinphi, Eccentricity)) - ProjectionMath.PiHalf);
                    cosX = Math.Cos(X);
                }
                switch (Mode)
                {
                    case AzimuthalMode.Oblique:
                        A = _akm1/(_cosphi0*(1.0 + _sinphi0*sinX + _cosphi0*cosX*coslam));
                        xy.Y = A*(_cosphi0*sinX - _sinphi0*cosX*coslam);
                        xy.X = A*cosX;
                        break;
                    case AzimuthalMode.Equator:
                        A = 2.0*_akm1/(1.0 + cosX*coslam);
                        xy.Y = A*sinX;
                        xy.X = A*cosX;
                        break;
                    case AzimuthalMode.SouthPole:
                    case AzimuthalMode.NorthPole:
                        if (Mode == AzimuthalMode.SouthPole)
                        {
                            phi = -phi;
                            coslam = -coslam;
                            sinphi = -sinphi;
                        }
                        xy.X = _akm1*ProjectionMath.tsfn(phi, sinphi, Eccentricity);
                        xy.Y = - xy.X*coslam;
                        break;
                }
                xy.X = xy.X*sinlam;
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            if (Spherical)
            {
                double c, rh, sinc, cosc;

                sinc = Math.Sin(c = 2.0*Math.Atan((rh = ProjectionMath.Distance(x, y))/_akm1));
                cosc = Math.Cos(c);
                lp.X = 0.0;
                switch (Mode)
                {
                    case AzimuthalMode.Equator:
                        if (Math.Abs(rh) <= EPS10)
                            lp.Y = 0.0;
                        else
                            lp.Y = Math.Asin(y*sinc/rh);
                        if (cosc != 0.0 || x != 0.0)
                            lp.X = Math.Atan2(x*sinc, cosc*rh);
                        break;
                    case AzimuthalMode.Oblique:
                        if (Math.Abs(rh) <= EPS10)
                            lp.Y = ProjectionLatitude;
                        else
                            lp.Y = Math.Asin(cosc*_sinphi0 + y*sinc*_cosphi0/rh);
                        if ((c = cosc - _sinphi0*Math.Sin(lp.Y)) != 0.0 || x != 0.0)
                            lp.X = Math.Atan2(x*sinc*_cosphi0, c*rh);
                        break;
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Mode == AzimuthalMode.NorthPole) y = -y;
                        if (Math.Abs(rh) <= EPS10)
                            lp.Y = ProjectionLatitude;
                        else
                            lp.Y = Math.Asin(Mode == AzimuthalMode.SouthPole ? -cosc : cosc);
                        lp.X = (x == 0.0 && y == 0.0) ? 0.0 : Math.Atan2(x, y);
                        break;
                }
            }
            else
            {
                double cosphi, sinphi, tp, phi_l, rho, halfe, halfpi;

                rho = ProjectionMath.Distance(x, y);
                switch (Mode)
                {
                    case AzimuthalMode.Oblique:
                    case AzimuthalMode.Equator:
                    default: // To prevent the compiler complaining about uninitialized vars.
                        cosphi = Math.Cos(tp = 2.0*Math.Atan2(rho*_cosphi0, _akm1));
                        sinphi = Math.Sin(tp);
                        phi_l = Math.Asin(cosphi*_sinphi0 + (y*sinphi*_cosphi0/rho));
                        tp = Math.Tan(.5*(ProjectionMath.PiHalf + phi_l));
                        x *= sinphi;
                        y = rho*_cosphi0*cosphi - y*_sinphi0*sinphi;
                        halfpi = ProjectionMath.PiHalf;
                        halfe = .5*Eccentricity;
                        break;
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Mode == AzimuthalMode.NorthPole) y = -y;
                        phi_l = ProjectionMath.PiHalf - 2.0*Math.Atan(tp = - rho/_akm1);
                        halfpi = -ProjectionMath.PiHalf;
                        halfe = -.5*Eccentricity;
                        break;
                }
                for (int i = 8; i-- != 0; phi_l = lp.Y)
                {
                    sinphi = Eccentricity*Math.Sin(phi_l);
                    lp.Y = 2.0*Math.Atan(tp*Math.Pow((1.0 + sinphi)/(1.0 - sinphi), halfe)) - halfpi;
                    if (Math.Abs(phi_l - lp.Y) < EPS10)
                    {
                        if (Mode == AzimuthalMode.SouthPole)
                            lp.Y = -lp.Y;
                        lp.X = (x == 0.0 && y == 0.0) ? 0.0 : Math.Atan2(x, y);
                        return lp;
                    }
                }
                throw new ConvergenceFailureException("Iteration didn't converge");
            }
            return lp;
        }

        ///<summary>Returns true if this projection is conformal</summary>
        public override Boolean IsConformal
        {
            get { return true; }
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        private double ssfn(double phit, double sinphi, double eccen)
        {
            sinphi *= eccen;
            return Math.Tan(.5*(ProjectionMath.PiHalf + phit))*
                   Math.Pow((1.0 - sinphi)/(1.0 + sinphi), .5*eccen);
        }

        public override String ToString()
        {
            return "Stereographic Azimuthal";
        }

    }
}
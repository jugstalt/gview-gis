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

    public class EquidistantAzimuthalProjection : AzimuthalProjection
    {

        private const double Tolerance = 1.0e-8;

        private double[] _en;
        private double _m1;
        private double _n1;
        private double _mp;
        private double _he;
        private double _g;
        //private double sinphi0, cosphi0;

        public EquidistantAzimuthalProjection()
            : this(ProjectionMath.ToRadians(90.0), ProjectionMath.ToRadians(0.0))
        {
        }

        public EquidistantAzimuthalProjection(double projectionLatitude, double projectionLongitude)
            : base(projectionLatitude, projectionLongitude)
        {
            Initialize();
        }

        public override Object Clone()
        {
            EquidistantAzimuthalProjection p = (EquidistantAzimuthalProjection)base.Clone();
            if (_en != null)
                p._en = (double[])_en.Clone();
            return p;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) < EPS10)
            {
                _mode = ProjectionLatitude < 0.0 ? AzimuthalMode.SouthPole : AzimuthalMode.NorthPole;
                _sinphi0 = ProjectionLatitude < 0.0 ? -1.0 : 1.0;
                _cosphi0 = 0.0;
            }
            else if (Math.Abs(ProjectionLatitude) < EPS10)
            {
                _mode = AzimuthalMode.Equator;
                _sinphi0 = 0.0;
                _cosphi0 = 1.0;
            }
            else
            {
                _mode = AzimuthalMode.Oblique;
                _sinphi0 = Math.Sin(ProjectionLatitude);
                _cosphi0 = Math.Cos(ProjectionLatitude);
            }
            if (!Spherical)
            {
                _en = ProjectionMath.enfn(EccentricitySquared);
                switch (Mode)
                {
                    case AzimuthalMode.NorthPole:
                        _mp = ProjectionMath.mlfn(ProjectionMath.PiHalf, 1.0, 0.0, _en);
                        break;
                    case AzimuthalMode.SouthPole:
                        _mp = ProjectionMath.mlfn(-ProjectionMath.PiHalf, -1.0, 0.0, _en);
                        break;
                    case AzimuthalMode.Equator:
                    case AzimuthalMode.Oblique:
                        _n1 = 1.0 / Math.Sqrt(1.0 - EccentricitySquared * _sinphi0 * _sinphi0);
                        _g = _sinphi0 * (_he = Eccentricity / Math.Sqrt(_oneEs));
                        _he *= _cosphi0;
                        break;
                }
            }
        }

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            if (Spherical)
            {
                double coslam, cosphi, sinphi;

                sinphi = Math.Sin(phi);
                cosphi = Math.Cos(phi);
                coslam = Math.Cos(lam);
                switch (Mode)
                {
                    case AzimuthalMode.Equator:
                    case AzimuthalMode.Oblique:
                        if (Mode == AzimuthalMode.Equator)
                            xy.Y = cosphi * coslam;
                        else
                            xy.Y = _sinphi0 * sinphi + _cosphi0 * cosphi * coslam;
                        if (Math.Abs(Math.Abs(xy.Y) - 1.0) < Tolerance)
                            if (xy.Y < 0.0)
                                throw new ProjectionException();
                            else
                                xy.X = xy.Y = 0.0;
                        else
                        {
                            xy.Y = Math.Acos(xy.Y);
                            xy.Y /= Math.Sin(xy.Y);
                            xy.X = xy.Y * cosphi * Math.Sin(lam);
                            xy.Y *= (Mode == AzimuthalMode.Equator) ? sinphi :
                                _cosphi0 * sinphi - _sinphi0 * cosphi * coslam;
                        }
                        break;
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Mode == AzimuthalMode.NorthPole)
                        {
                            phi = -phi;
                            coslam = -coslam;
                        }
                        if (Math.Abs(phi - ProjectionMath.PiHalf) < EPS10)
                            throw new ProjectionException();
                        xy.X = (xy.Y = (ProjectionMath.PiHalf + phi)) * Math.Sin(lam);
                        xy.Y *= coslam;
                        break;
                }
            }
            else
            {
                double coslam, cosphi, sinphi, rho, s, H, H2, c, Az, t, ct, st, cA, sA;

                coslam = Math.Cos(lam);
                cosphi = Math.Cos(phi);
                sinphi = Math.Sin(phi);
                switch (Mode)
                {
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        if (Mode == AzimuthalMode.NorthPole) coslam = -coslam;
                        xy.X = (rho = Math.Abs(_mp - ProjectionMath.mlfn(phi, sinphi, cosphi, _en))) *
                            Math.Sin(lam);
                        xy.Y = rho * coslam;
                        break;
                    case AzimuthalMode.Equator:
                    case AzimuthalMode.Oblique:
                        if (Math.Abs(lam) < EPS10 && Math.Abs(phi - ProjectionLatitude) < EPS10)
                        {
                            xy.X = xy.Y = 0.0;
                            break;
                        }
                        t = Math.Atan2(_oneEs * sinphi + EccentricitySquared * _n1 * _sinphi0 *
                            Math.Sqrt(1.0 - EccentricitySquared * sinphi * sinphi), cosphi);
                        ct = Math.Cos(t); st = Math.Sin(t);
                        Az = Math.Atan2(Math.Sin(lam) * ct, _cosphi0 * st - _sinphi0 * coslam * ct);
                        cA = Math.Cos(Az); sA = Math.Sin(Az);
                        s = ProjectionMath.Asin(Math.Abs(sA) < Tolerance ?
                            (_cosphi0 * st - _sinphi0 * coslam * ct) / cA :
                            Math.Sin(lam) * ct / sA);
                        H = _he * cA;
                        H2 = H * H;
                        c = _n1 * s * (1.0 + s * s * (-H2 * (1.0 - H2) / 6.0 +
                            s * (_g * H * (1.0 - 2.0 * H2 * H2) / 8.0 +
                            s * ((H2 * (4.0 - 7.0 * H2) - 3.0 * _g * _g * (1.0 - 7.0 * H2)) /
                            120.0 - s * _g * H / 48.0))));
                        xy.X = c * sA;
                        xy.Y = c * cA;
                        break;
                }
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            if (Spherical)
            {
                double cosc, c_rh, sinc;

                if ((c_rh = ProjectionMath.Distance(x, y)) > Math.PI)
                {
                    if (c_rh - EPS10 > Math.PI)
                        throw new ProjectionException();
                    c_rh = Math.PI;
                }
                else if (c_rh < EPS10)
                {
                    lp.Y = ProjectionLatitude;
                    lp.X = 0.0;
                    return lp;
                }
                if (Mode == AzimuthalMode.Oblique || Mode == AzimuthalMode.Equator)
                {
                    sinc = Math.Sin(c_rh);
                    cosc = Math.Cos(c_rh);
                    if (Mode == AzimuthalMode.Equator)
                    {
                        lp.Y = ProjectionMath.Asin(y * sinc / c_rh);
                        x *= sinc;
                        y = cosc * c_rh;
                    }
                    else
                    {
                        lp.Y = ProjectionMath.Asin(cosc * _sinphi0 + y * sinc * _cosphi0 /
                            c_rh);
                        y = (cosc - _sinphi0 * Math.Sin(lp.Y)) * c_rh;
                        x *= sinc * _cosphi0;
                    }
                    lp.X = y == 0.0 ? 0.0 : Math.Atan2(x, y);
                }
                else if (Mode == AzimuthalMode.NorthPole)
                {
                    lp.Y = ProjectionMath.PiHalf - c_rh;
                    lp.X = Math.Atan2(x, -y);
                }
                else
                {
                    lp.Y = c_rh - ProjectionMath.PiHalf;
                    lp.X = Math.Atan2(x, y);
                }
            }
            else
            {
                double c, Az, cosAz, A, B, D, E, F, psi, t;
                int i;

                if ((c = ProjectionMath.Distance(x, y)) < EPS10)
                {
                    lp.Y = ProjectionLatitude;
                    lp.X = 0.0;
                    return (lp);
                }
                if (Mode == AzimuthalMode.Oblique || Mode == AzimuthalMode.Equator)
                {
                    cosAz = Math.Cos(Az = Math.Atan2(x, y));
                    t = _cosphi0 * cosAz;
                    B = EccentricitySquared * t / _oneEs;
                    A = -B * t;
                    B *= 3.0 * (1.0 - A) * _sinphi0;
                    D = c / _n1;
                    E = D * (1.0 - D * D * (A * (1.0 + A) / 6.0 + B * (1.0 + 3.0 * A) * D / 24.0));
                    F = 1.0 - E * E * (A / 2.0 + B * E / 6.0);
                    psi = ProjectionMath.Asin(_sinphi0 * Math.Cos(E) + t * Math.Sin(E));
                    lp.X = ProjectionMath.Asin(Math.Sin(Az) * Math.Sin(E) / Math.Cos(psi));
                    if ((t = Math.Abs(psi)) < EPS10)
                        lp.Y = 00.0;
                    else if (Math.Abs(t - ProjectionMath.PiHalf) < 0.0)
                        lp.Y = ProjectionMath.PiHalf;
                    else
                        lp.Y = Math.Atan((1.0 - EccentricitySquared * F * _sinphi0 / Math.Sin(psi)) * Math.Tan(psi) / _oneEs);
                }
                else
                {
                    lp.Y = ProjectionMath.inv_mlfn(Mode == AzimuthalMode.NorthPole ? _mp - c : _mp + c, EccentricitySquared, _en);
                    lp.X = Math.Atan2(x, Mode == AzimuthalMode.NorthPole ? -y : y);
                }
            }
            return lp;
        }

        /*
        public Shape getBoundingShape() {
            double r = ProjectionMath.HALFPI * a;
            return new Ellipse2D.Double( -r, -r, 2*r, 2*r );
        }
        */

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Equidistant Azimuthal";
        }

    }

}
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

    public class PolyconicProjection : Projection
    {

        private double _ml0;
        private double[] _en;

        private const double Tolerance = 1e-10;
        private const double CONV = 1e-10;
        private const int N_ITER = 10;
        private const int I_ITER = 20;
        private const double ITOL = 1.0e-12;

        public PolyconicProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(80);
            MinLongitude = ProjectionMath.ToRadians(-60);
            MaxLongitude = ProjectionMath.ToRadians(60);
            Initialize();
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            if (Spherical)
            {
                double cot, E;

                if (Math.Abs(lpphi) <= Tolerance)
                {
                    xy.X = lplam;
                    xy.Y = _ml0;
                }
                else
                {
                    cot = 1.0 / Math.Tan(lpphi);
                    xy.X = Math.Sin(E = lplam * Math.Sin(lpphi)) * cot;
                    xy.Y = lpphi - ProjectionLatitude + cot * (1.0 - Math.Cos(E));
                }
            }
            else
            {
                double ms, sp, cp;

                if (Math.Abs(lpphi) <= Tolerance)
                {
                    xy.X = lplam;
                    xy.Y = -_ml0;
                }
                else
                {
                    sp = Math.Sin(lpphi);
                    ms = Math.Abs(cp = Math.Cos(lpphi)) > Tolerance ? ProjectionMath.msfn(sp, cp, EccentricitySquared) / sp : 0.0;
                    xy.X = ms * Math.Sin(xy.X *= sp);
                    xy.Y = (ProjectionMath.mlfn(lpphi, sp, cp, _en) - _ml0) + ms * (1.0 - Math.Cos(lplam));
                }
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double lpphi;
            if (Spherical)
            {
                double B, dphi, tp;
                int i;

                if (Math.Abs(lpphi = ProjectionLatitude + xyy) <= Tolerance)
                {
                    lp.X = xyx; lp.Y = 0.0;
                }
                else
                {
                    lpphi = xyy;
                    B = xyx * xyx + xyy * xyy;
                    i = N_ITER;
                    do
                    {
                        tp = Math.Tan(lpphi);
                        lpphi -= (dphi = (xyy * (lpphi * tp + 1.0) - lpphi -
                                 0.5 * (lpphi * lpphi + B) * tp) /
                            ((lpphi - xyy) / tp - 1.0));
                    } while (Math.Abs(dphi) > CONV && --i > 0);
                    if (i == 0) throw new ProjectionException("I");
                    lp.X = Math.Asin(xyx * Math.Tan(lpphi)) / Math.Sin(lpphi);
                    lp.Y = lpphi;
                }
            }
            else
            {
                xyy += _ml0;
                if (Math.Abs(xyy) <= Tolerance) { lp.X = xyx; lp.Y = 0.0; }
                else
                {
                    double r, c, sp, cp, s2ph, ml, mlb, mlp, dPhi;
                    int i;

                    r = xyy * xyy + xyx * xyx;
                    for (lpphi = xyy, i = I_ITER; i > 0; --i)
                    {
                        sp = Math.Sin(lpphi);
                        s2ph = sp * (cp = Math.Cos(lpphi));
                        if (Math.Abs(cp) < ITOL)
                            throw new ProjectionException("I");
                        c = sp * (mlp = Math.Sqrt(1.0 - EccentricitySquared * sp * sp)) / cp;
                        ml = ProjectionMath.mlfn(lpphi, sp, cp, _en);
                        mlb = ml * ml + r;
                        mlp = (1.0 / EccentricitySquared) / (mlp * mlp * mlp);
                        lpphi += (dPhi =
                            (ml + ml + c * mlb - 2.0 * xyy * (c * ml + 1.0)) / (
                            EccentricitySquared * s2ph * (mlb - 2.0 * xyy * ml) / c +
                            2.0 * (xyy - ml) * (c * mlp - 1.0 / s2ph) - mlp - mlp));
                        if (Math.Abs(dPhi) <= ITOL)
                            break;
                    }
                    if (i == 0)
                        throw new ProjectionException("I");
                    c = Math.Sin(lpphi);
                    lp.X = Math.Asin(xyx * Math.Tan(lpphi) * Math.Sqrt(1.0 - EccentricitySquared * c * c)) / Math.Sin(lpphi);
                    lp.Y = lpphi;
                }
            }
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override void Initialize()
        {
            base.Initialize();
            _spherical = true;//FIXME
            if (!Spherical)
            {
                _en = ProjectionMath.enfn(EccentricitySquared);
                if (_en == null)
                    throw new ProjectionException("E");
                _ml0 = ProjectionMath.mlfn(ProjectionLatitude, Math.Sin(ProjectionLatitude), Math.Cos(ProjectionLatitude), _en);
            }
            else
            {
                _ml0 = -ProjectionLatitude;
            }
        }

        public override String ToString()
        {
            return "Polyconic (American)";
        }

    }
}
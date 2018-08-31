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

    public class SimpleConicProjection : ConicProjection
    {

        public enum ConicTypes
        {
            Euler = 0,
            Murdoch1,
            Murdoch2,
            Murdoch3,
            PerspectiveConic,
            Tissot,
            Vitkovsky1
        }

        private double _n;
        private double _rhoC;
        private double _rho0;
        private double _sig;
        private double _c1, _c2;
        private readonly ConicTypes _type;

        /*
        public final static int EULER = 0;
        public final static int MURD1 = 1;
        public final static int MURD2 = 2;
        public final static int MURD3 = 3;
        public final static int PCONIC = 4;
        public final static int TISSOT = 5;
        public final static int VITK1 = 6;
        private final static double EPS10 = 1.e-10;
        private final static double EPS = 1e-10;
        */
        public SimpleConicProjection()
            : this(ConicTypes.Euler)
        {
        }

        public SimpleConicProjection(ConicTypes type)
        {
            _type = type;
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(80);
        }

        public override String ToString()
        {
            return "Simple Conic";
        }

        public ConicTypes ConicType
        {
            get { return _type; }
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double rho;

            switch (ConicType)
            {
                case ConicTypes.Murdoch2:
                    rho = _rhoC + Math.Tan(_sig - lpphi);
                    break;
                case ConicTypes.PerspectiveConic:
                    rho = _c2 * (_c1 - Math.Tan(lpphi));
                    break;
                default:
                    rho = _rhoC - lpphi;
                    break;
            }
            xy.X = rho * Math.Sin(lplam *= _n);
            xy.Y = _rho0 - rho * Math.Cos(lplam);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double rho;

            rho = ProjectionMath.Distance(xyx, lp.Y = _rho0 - xyy);
            if (_n < 0.0)
            {
                rho = -rho;
                lp.X = -xyx;
                lp.Y = -xyy;
            }
            lp.X = Math.Atan2(xyx, xyy) / _n;
            switch (ConicType)
            {
                case ConicTypes.PerspectiveConic:
                    lp.Y = Math.Atan(_c1 - rho / _c2) + _sig;
                    break;
                case ConicTypes.Murdoch2:
                    lp.Y = _sig - Math.Atan(rho - _rhoC);
                    break;
                default:
                    lp.Y = _rhoC - rho;
                    break;
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
            double cs, dummy;

            /* get common factors for simple conics */
            double p1, p2, d, s;
            int err = 0;

            /*FIXME
                    if (!pj_param(params, "tlat_1").i ||
                        !pj_param(params, "tlat_2").i) {
                        err = -41;
                    } else {
                        p1 = pj_param(params, "rlat_1").f;
                        p2 = pj_param(params, "rlat_2").f;
                        *del = 0.5 * (p2 - p1);
                        _sig = 0.5 * (p2 + p1);
                        err = (Math.abs(*del) < EPS10 || Math.abs(_sig) < EPS10) ? -42 : 0;
                        *del = *del;
                    }
            */
            p1 = ProjectionMath.ToRadians(30);//FIXME
            p2 = ProjectionMath.ToRadians(60);//FIXME
            var del = 0.5 * (p2 - p1);
            _sig = 0.5 * (p2 + p1);
            err = (Math.Abs(del) < EPS10 || Math.Abs(_sig) < EPS10) ? -42 : 0;
            del = del;

            if (err != 0)
                throw new ProjectionException("Error " + err);

            switch (ConicType)
            {
                case ConicTypes.Tissot:
                    _n = Math.Sin(_sig);
                    cs = Math.Cos(del);
                    _rhoC = _n / cs + cs / _n;
                    _rho0 = Math.Sqrt((_rhoC - 2 * Math.Sin(ProjectionLatitude)) / _n);
                    break;
                case ConicTypes.Murdoch1:
                    _rhoC = Math.Sin(del) / (del * Math.Tan(_sig)) + _sig;
                    _rho0 = _rhoC - ProjectionLatitude;
                    _n = Math.Sin(_sig);
                    break;
                case ConicTypes.Murdoch2:
                    _rhoC = (cs = Math.Sqrt(Math.Cos(del))) / Math.Tan(_sig);
                    _rho0 = _rhoC + Math.Tan(_sig - ProjectionLatitude);
                    _n = Math.Sin(_sig) * cs;
                    break;
                case ConicTypes.Murdoch3:
                    _rhoC = del / (Math.Tan(_sig) * Math.Tan(del)) + _sig;
                    _rho0 = _rhoC - ProjectionLatitude;
                    _n = Math.Sin(_sig) * Math.Sin(del) * Math.Tan(del) / (del * del);
                    break;
                case ConicTypes.Euler:
                    _n = Math.Sin(_sig) * Math.Sin(del) / del;
                    del *= 0.5;
                    _rhoC = del / (Math.Tan(del) * Math.Tan(_sig)) + _sig;
                    _rho0 = _rhoC - ProjectionLatitude;
                    break;
                case ConicTypes.PerspectiveConic:
                    _n = Math.Sin(_sig);
                    _c2 = Math.Cos(del);
                    _c1 = 1.0 / Math.Tan(_sig);
                    if (Math.Abs(del = ProjectionLatitude - _sig) - EPS10 >= ProjectionMath.PiHalf)
                        throw new ProjectionException("-43");
                    _rho0 = _c2 * (_c1 - Math.Tan(del));
                    MaxLatitude = ProjectionMath.ToRadians(60);//FIXME
                    break;
                case ConicTypes.Vitkovsky1:
                    _n = (cs = Math.Tan(del)) * Math.Sin(_sig) / del;
                    _rhoC = del / (cs * Math.Tan(_sig)) + _sig;
                    _rho0 = _rhoC - ProjectionLatitude;
                    break;
            }
        }
    }
}
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
    public class PerspectiveProjection : Projection
    {

        public enum Modes
        {
            NorthPole, SouthPole, Equit, Oblique
        }
        private double height;
        private double psinph0;
        private double pcosph0;
        private double p;
        private double rp;
        private double pn1;
        private double pfact;
        private double h;
        private double cg;
        private double sg;
        private double sw;
        private double cw;
        private Modes mode;
        private int tilt;

        /*
        private final static int N_POLE = 0;
        private final static int S_POLE = 1;
        private final static int EQUIT = 2;
        private final static int OBLIQ = 3;
        */
        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double sinphi = Math.Sin(lpphi);
            double cosphi = Math.Cos(lpphi);
            double coslam = Math.Cos(lplam);
            switch (mode)
            {
                case Modes.Oblique:
                    xy.Y = psinph0 * sinphi + pcosph0 * cosphi * coslam;
                    break;
                case Modes.Equit:
                    xy.Y = cosphi * coslam;
                    break;
                case Modes.SouthPole:
                    xy.Y = -sinphi;
                    break;
                case Modes.NorthPole:
                    xy.Y = sinphi;
                    break;
            }
            //		if (xy.y < rp)
            //			throw new ProjectionException("");
            xy.Y = pn1 / (p - xy.Y);
            xy.X = xy.Y * cosphi * Math.Sin(lplam);
            switch (mode)
            {
                case Modes.Oblique:
                    xy.Y *= (pcosph0 * sinphi -
                             psinph0 * cosphi * coslam);
                    break;
                case Modes.Equit:
                    xy.Y *= sinphi;
                    break;
                case Modes.NorthPole:
                case Modes.SouthPole:
                    if (mode == Modes.NorthPole) coslam = -coslam;
                    xy.Y *= cosphi * coslam;
                    break;
            }
            if (tilt != 0)
            {
                double yt, ba;

                yt = xy.Y * cg + xy.X * sg;
                ba = 1.0 /
                (yt * sw * h + cw);
                xy.X = (xy.X * cg - xy.Y * sg) * cw * ba;
                xy.Y = yt * ba;
            }
            return xy;
        }

        public override Boolean HasInverse
        {
            get
            {
                return false;
            }
        }

        /*FIXME
        INVERSE(s_inverse); /* spheroid * /
            double  rh, cosz, sinz;

            if (tilt) {
                double bm, bq, yt;

                yt = 1./(pn1 - xy.y * sw);
                bm = pn1 * xy.x * yt;
                bq = pn1 * xy.y * cw * yt;
                xy.x = bm * cg + bq * sg;
                xy.y = bq * cg - bm * sg;
            }
            rh = hypot(xy.x, xy.y);
            if ((sinz = 1. - rh * rh * pfact) < 0.) I_ERROR;
            sinz = (p - Math.sqrt(sinz)) / (pn1 / rh + rh / pn1);
            cosz = sqrt(1. - sinz * sinz);
            if (fabs(rh) <= EPS10) {
                lp.lam = 0.;
                lp.phi = phi0;
            } else {
                switch (mode) {
                case OBLIQ:
                    lp.phi = Math.asin(cosz * sinph0 + xy.y * sinz * cosph0 / rh);
                    xy.y = (cosz - sinph0 * sin(lp.phi)) * rh;
                    xy.x *= sinz * cosph0;
                    break;
                case EQUIT:
                    lp.phi = Math.asin(xy.y * sinz / rh);
                    xy.y = cosz * rh;
                    xy.x *= sinz;
                    break;
                case N_POLE:
                    lp.phi = Math.asin(cosz);
                    xy.y = -xy.y;
                    break;
                case S_POLE:
                    lp.phi = - Math.asin(cosz);
                    break;
                }
                lp.lam = Math.atan2(xy.x, xy.y);
            }
            return (lp);
        }
        */

        public override void Initialize()
        {
            base.Initialize();
            mode = Modes.Equit;
            height = EquatorRadius;
            tilt = 0;
            //		if ((height = pj_param(params, "dh").f) <= 0.) E_ERROR(-30);
            /*
                    if (fabs(fabs(phi0) - Math.HALFPI) < EPS10)
                        mode = phi0 < 0. ? S_POLE : N_POLE;
                    else if (fabs(phi0) < EPS10)
                        mode = EQUIT;
                    else {
                        mode = OBLIQ;
                        psinph0 = Math.sin(phi0);
                        pcosph0 = Math.cos(phi0);
                    }
            */
            pn1 = height / EquatorRadius; /* normalize by radius */
            p = 1.0 + pn1;
            rp = 1.0 / p;
            h = 1.0 / pn1;
            pfact = (p + 1.0) * h;
            EccentricitySquared = 0.0;
        }
        /*FIXME
        ENTRY0(nsper)
            tilt = 0;
        ENDENTRY(setup(P))
        ENTRY0(tpers)
            double omega, gamma;

            omega = pj_param(params, "dtilt").f * DEG_TO_RAD;
            gamma = pj_param(params, "dazi").f * DEG_TO_RAD;
            tilt = 1;
            cg = cos(gamma); sg = sin(gamma);
            cw = cos(omega); sw = sin(omega);
        ENDENTRY(setup(P))
        */

        public override String ToString()
        {
            return "Perspective";
        }

    }
}
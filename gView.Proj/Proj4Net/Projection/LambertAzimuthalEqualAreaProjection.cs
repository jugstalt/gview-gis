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

/*
 * This file was semi-automatically converted from the public-domain USGS PROJ source.
 */

    public class LambertAzimuthalEqualAreaProjection : Projection
    {

        //private static final int N_POLE = 0;
        //private static final int S_POLE = 1;
        //private static final int EQUIT = 2;
        //private static final int OBLIQ = 3;

        private AzimuthalMode mode = 0;
        private double phi0;
        private double sinb1;
        private double cosb1;
        private double xmf;
        private double ymf;
        private double mmf;
        private double qp;
        private double dd;
        private double rq;
        private double[] apa;
        private double sinph0;
        private double cosph0;

        public LambertAzimuthalEqualAreaProjection()
            : this(false)
        {
        }

        public LambertAzimuthalEqualAreaProjection(bool south)
        {
            //minLatitude = Math.toRadians(0);
            //maxLatitude = Math.toRadians(90);
            //projectionLatitude1 = south ? -ProjectionMath.QUARTERPI : ProjectionMath.QUARTERPI;
            //projectionLatitude2 = south ? -ProjectionMath.HALFPI : ProjectionMath.HALFPI;
            //initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            double t;

            phi0 = _projectionLatitude;

            if (Math.Abs((t = Math.Abs(phi0)) - ProjectionMath.PiHalf) < ProjectionMath.EPS10)
            {
                mode = phi0 < 0.0 ? AzimuthalMode.SouthPole : AzimuthalMode.NorthPole;
            }
            else if (Math.Abs(t) < ProjectionMath.EPS10)
            {
                mode = AzimuthalMode.Equator;
            }
            else
            {
                mode = AzimuthalMode.Oblique;
            }
            if (!_spherical)
            {
                double sinphi;

                _e = Math.Sqrt(_es);
                qp = ProjectionMath.Qsfn(1.0, _e, _oneEs);
                mmf = 0.5/(1.0 - _es);
                apa = ProjectionMath.AuthSet(_es);
                switch (mode)
                {
                    case AzimuthalMode.NorthPole:
                    case AzimuthalMode.SouthPole:
                        dd = 1.0;
                        break;
                    case AzimuthalMode.Equator:
                        dd = 1.0/(rq = Math.Sqrt(.5*qp));
                        xmf = 1.0;
                        ymf = .5*qp;
                        break;
                    case AzimuthalMode.Oblique:
                        rq = Math.Sqrt(.5*qp);
                        sinphi = Math.Sin(phi0);
                        sinb1 = ProjectionMath.Qsfn(sinphi, _e, _oneEs)/qp;
                        cosb1 = Math.Sqrt(1.0 - sinb1*sinb1);
                        dd = Math.Cos(phi0)/(Math.Sqrt(1.0 - _es*sinphi*sinphi)*
                                             rq*cosb1);
                        ymf = (xmf = rq)/dd;
                        xmf *= dd;
                        break;
                }
            }
            else
            {
                if (mode == AzimuthalMode.Oblique)
                {
                    sinph0 = Math.Sin(phi0);
                    cosph0 = Math.Cos(phi0);
                }
            }
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate dst)
        {
            if (_spherical)
            {
                ProjectSpherical(lplam, lpphi, dst);
            }
            else
            {
                ProjectNonSpherical(lplam, lpphi, dst);
            }
            return dst;
        }

        protected void ProjectSpherical(double lplam, double lpphi, Coordinate dst)
        {
            double coslam, cosphi, sinphi;

            sinphi = Math.Sin(lpphi);
            cosphi = Math.Cos(lpphi);
            coslam = Math.Cos(lplam);
            switch (mode)
            {
                case AzimuthalMode.Equator:
                case AzimuthalMode.Oblique:
                    if (mode == AzimuthalMode.Equator)
                        dst.Y = 1.0 + cosphi*coslam;
                    else
                        dst.Y = 1.0 + sinph0*sinphi + cosph0*cosphi*coslam;

                    if (dst.Y <= EPS10) throw new ProjectionException("F");
                    dst.X = (dst.Y = Math.Sqrt(2.0/dst.Y))*cosphi*Math.Sin(lplam);
                    dst.Y *= mode == AzimuthalMode.Equator
                                 ? sinphi
                                 : cosph0*sinphi - sinph0*cosphi*coslam;
                    break;

                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (mode == AzimuthalMode.NorthPole)
                        coslam = -coslam;
                    if (Math.Abs(lpphi + phi0) < EPS10) throw new ProjectionException("F");
                    ;
                    dst.Y = ProjectionMath.PiFourth - lpphi*.5;
                    dst.Y = 2.0*(mode == AzimuthalMode.SouthPole ? Math.Cos(dst.Y) : Math.Sin(dst.Y));
                    dst.X = dst.Y*Math.Sin(lplam);
                    dst.Y *= coslam;
                    break;
            }
        }

        protected void ProjectNonSpherical(double lplam, double lpphi, Coordinate dst)
        {
            double coslam, sinlam, sinphi, q, sinb = 0.0, cosb = 0.0, b = 0.0;

            coslam = Math.Cos(lplam);
            sinlam = Math.Sin(lplam);
            sinphi = Math.Sin(lpphi);
            q = ProjectionMath.Qsfn(sinphi, _e, _oneEs);
            if (mode == AzimuthalMode.Oblique || mode == AzimuthalMode.Equator)
            {
                sinb = q/qp;
                cosb = Math.Sqrt(1.0 - sinb*sinb);
            }
            switch (mode)
            {
                case AzimuthalMode.Oblique:
                    b = 1.0 + sinb1*sinb + cosb1*cosb*coslam;
                    break;
                case AzimuthalMode.Equator:
                    b = 1.0 + cosb*coslam;
                    break;
                case AzimuthalMode.NorthPole:
                    b = ProjectionMath.PiHalf + lpphi;
                    q = qp - q;
                    break;
                case AzimuthalMode.SouthPole:
                    b = lpphi - ProjectionMath.PiHalf;
                    q = qp + q;
                    break;
            }
            if (Math.Abs(b) < EPS10) throw new ProjectionException("F");

            switch (mode)
            {
                case AzimuthalMode.Oblique:
                case AzimuthalMode.Equator:
                    if (mode == AzimuthalMode.Oblique)
                    {
                        dst.Y = ymf*(b = Math.Sqrt(2.0/b))
                                *(cosb1*sinb - sinb1*cosb*coslam);
                    }
                    else
                    {
                        dst.Y = (b = Math.Sqrt(2.0/(1.0 + cosb*coslam)))*sinb*ymf;
                    }
                    dst.X = xmf*b*cosb*sinlam;
                    break;
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (q >= 0.0)
                    {
                        dst.X = (b = Math.Sqrt(q))*sinlam;
                        dst.Y = coslam*(mode == AzimuthalMode.SouthPole ? b : -b);
                    }
                    else
                        dst.X = dst.Y = 0.0;
                    break;
            }
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate dst)
        {
            if (_spherical)
            {
                ProjectInverseSpherical(xyx, xyy, dst);
            }
            else
            {
                ProjectInverseNonSpherical(xyx, xyy, dst);
            }
            return dst;
        }

        protected void ProjectInverseSpherical(double xyx, double xyy, Coordinate dst)
        {
            double cosz = 0.0, rh, sinz = 0.0;
            double lpphi, lplam;

            rh = ProjectionMath.Hypot(xyx, xyy);
            if ((lpphi = rh*.5) > 1.0) throw new ProjectionException("I_ERROR");
            lpphi = 2.0*Math.Asin(lpphi);
            if (mode == AzimuthalMode.Oblique || mode == AzimuthalMode.Equator)
            {
                sinz = Math.Sin(lpphi);
                cosz = Math.Cos(lpphi);
            }
            switch (mode)
            {
                case AzimuthalMode.Equator:
                    lpphi = Math.Abs(rh) <= EPS10 ? 0.0 : Math.Asin(xyy*sinz/rh);
                    xyx *= sinz;
                    xyy = cosz*rh;
                    break;
                case AzimuthalMode.Oblique:
                    lpphi = Math.Abs(rh) <= EPS10
                                ? phi0
                                : Math.Asin(cosz*sinph0 + xyy*sinz*cosph0/rh);
                    xyx *= sinz*cosph0;
                    xyy = (cosz - Math.Sin(lpphi)*sinph0)*rh;
                    break;
                case AzimuthalMode.NorthPole:
                    xyy = -xyy;
                    lpphi = ProjectionMath.PiHalf - lpphi;
                    break;
                case AzimuthalMode.SouthPole:
                    lpphi -= ProjectionMath.PiHalf;
                    break;
            }
            lplam = (xyy == 0.0 && (mode == AzimuthalMode.Equator || mode == AzimuthalMode.Oblique))
                        ? 0.0
                        : Math.Atan2(xyx, xyy);
            dst.X = lplam;
            dst.Y = lpphi;
        }

        protected void ProjectInverseNonSpherical(double xyx, double xyy, Coordinate dst)
        {
            double lpphi, lplam;
            double cCe, sCe, q, rho, ab = 0.0;

            switch (mode)
            {
                case AzimuthalMode.Equator:
                case AzimuthalMode.Oblique:
                    if ((rho = ProjectionMath.Hypot(xyx /= dd, xyy *= dd)) < EPS10)
                    {
                        lplam = 0.0;
                        lpphi = phi0;
                        dst.X = lplam;
                        dst.Y = lpphi;
                        return;
                    }
                    cCe = Math.Cos(sCe = 2.0*Math.Asin(.5*rho/rq));
                    xyx *= (sCe = Math.Sin(sCe));
                    if (mode == AzimuthalMode.Oblique)
                    {
                        q = qp*(ab = cCe*sinb1 + xyy*sCe*cosb1/rho);
                        xyy = rho*cosb1*cCe - xyy*sinb1*sCe;
                    }
                    else
                    {
                        q = qp*(ab = xyy*sCe/rho);
                        xyy = rho*cCe;
                    }
                    break;
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (mode == AzimuthalMode.NorthPole)
                        xyy = -xyy;
                    if (0 == (q = (xyx*xyx + xyy*xyy)))
                    {
                        lplam = 0.0;
                        lpphi = phi0;
                        dst.X = lplam;
                        dst.Y = lpphi;
                        return;
                    }
                    /*
        q = P->qp - q;
        */
                    ab = 1.0 - q/qp;
                    if (mode == AzimuthalMode.SouthPole)
                        ab = - ab;
                    break;
            }
            lplam = Math.Atan2(xyx, xyy);
            lpphi = ProjectionMath.AuthLat(Math.Asin(ab), apa);
            dst.X = lplam;
            dst.Y = lpphi;
        }

        /// <summary>
        /// Gets if this projection is equal area
        /// </summary>
        public override bool IsEqualArea
        {
            get { return true; }
        }

        public override bool HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Lambert Azimuthal Equal Area";
        }

    }
}
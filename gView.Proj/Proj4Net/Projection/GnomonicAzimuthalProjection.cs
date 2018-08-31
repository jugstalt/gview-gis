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

    public class GnomonicAzimuthalProjection : AzimuthalProjection
    {

        public GnomonicAzimuthalProjection()
            : this(ProjectionMath.ToRadians(90.0), ProjectionMath.ToRadians(0.0))
        {
        }

        public GnomonicAzimuthalProjection(double projectionLatitude, double projectionLongitude)
            : base(projectionLatitude, projectionLongitude)
        {
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(90);
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            double sinphi = Math.Sin(phi);
            double cosphi = Math.Cos(phi);
            double coslam = Math.Cos(lam);

            switch (Mode)
            {
                case AzimuthalMode.Equator:
                    xy.Y = cosphi * coslam;
                    break;
                case AzimuthalMode.Oblique:
                    xy.Y = _sinphi0 * sinphi + _cosphi0 * cosphi * coslam;
                    break;
                case AzimuthalMode.SouthPole:
                    xy.Y = -sinphi;
                    break;
                case AzimuthalMode.NorthPole:
                    xy.Y = sinphi;
                    break;
            }
            if (Math.Abs(xy.Y) <= EPS10)
                throw new ProjectionException();
            xy.X = (xy.Y = 1.0 / xy.Y) * cosphi * Math.Sin(lam);

            switch (Mode)
            {
                case AzimuthalMode.Equator:
                    xy.Y *= sinphi;
                    break;
                case AzimuthalMode.Oblique:
                    xy.Y *= _cosphi0 * sinphi - _sinphi0 * cosphi * coslam;
                    break;
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (Mode == AzimuthalMode.NorthPole) coslam = -coslam;
                    xy.Y *= cosphi * coslam;
                    break;
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            double rh, cosz, sinz;

            rh = ProjectionMath.Distance(x, y);
            sinz = Math.Sin(lp.Y = Math.Atan(rh));
            cosz = Math.Sqrt(1.0 - sinz * sinz);
            if (Math.Abs(rh) <= EPS10)
            {
                lp.Y = ProjectionLatitude;
                lp.X = 0.0;
            }
            else
            {
                switch (Mode)
                {
                    case AzimuthalMode.Oblique:
                        lp.Y = cosz * _sinphi0 + y * sinz * _cosphi0 / rh;
                        if (Math.Abs(lp.Y) >= 1.0)
                            lp.Y = lp.Y > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
                        else
                            lp.Y = Math.Asin(lp.Y);
                        y = (cosz - _sinphi0 * Math.Sin(lp.Y)) * rh;
                        x *= sinz * _cosphi0;
                        break;
                    case AzimuthalMode.Equator:
                        lp.Y = y * sinz / rh;
                        if (Math.Abs(lp.Y) >= 1.0)
                            lp.Y = lp.Y > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
                        else
                            lp.Y = Math.Asin(lp.Y);
                        y = cosz * rh;
                        x *= sinz;
                        break;
                    case AzimuthalMode.SouthPole:
                        lp.Y -= ProjectionMath.PiHalf;
                        break;
                    case AzimuthalMode.NorthPole:
                        lp.Y = ProjectionMath.PiHalf - lp.Y;
                        y = -y;
                        break;
                }
                lp.X = Math.Atan2(x, y);
            }
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Gnomonic Azimuthal";
        }

    }
}
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
using Proj4Net.Datum;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{

    public class LambertConformalConicProjection : ConicProjection
    {

        private double n;
        private double rho0;
        private double c;

        public LambertConformalConicProjection()
        {
            MinLatitude = ProjectionMath.ToRadians(0);
            MaxLatitude = ProjectionMath.ToRadians(80.0);
            ProjectionLatitude = ProjectionMath.PiFourth;
            ProjectionLatitude1 = 0;
            ProjectionLatitude2 = 0;
            Initialize();
        }

       /**
        * Set up a projection suitable for State Place Coordinates.
        */
        public LambertConformalConicProjection(Ellipsoid ellipsoid, double lon_0, double lat_1, double lat_2, double lat_0, double x_0, double y_0)
        {
            Ellipsoid = ellipsoid;
            ProjectionLongitude = lon_0;
            ProjectionLatitude = lat_0;
            ScaleFactor = 1.0;
            FalseEasting = x_0;
            FalseNorthing = y_0;
            ProjectionLatitude1 = lat_1;
            ProjectionLatitude2 = lat_2;
            Initialize();
        }

        public override Coordinate Project(double x, double y, Coordinate coord)
        {
            double rho;
            if (Math.Abs(Math.Abs(y) - ProjectionMath.PiHalf) < 1e-10)
                rho = 0.0;
            else
            {
                rho = c * (Spherical ?
                    Math.Pow(Math.Tan(ProjectionMath.PiFourth + .5 * y), -n) :
                      Math.Pow(ProjectionMath.tsfn(y, Math.Sin(y), Eccentricity), n));
            }
            coord.X = ScaleFactor * (rho * Math.Sin(x *= n));
            coord.Y = ScaleFactor * (rho0 - rho * Math.Cos(x));
            return coord;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate coord)
        {
            x /= ScaleFactor;
            y /= ScaleFactor;
            double rho = ProjectionMath.Distance(x, y = rho0 - y);
            if (rho != 0)
            {
                if (n < 0.0)
                {
                    rho = -rho;
                    x = -x;
                    y = -y;
                }
                if (Spherical)
                    coord.Y = 2.0 * Math.Atan(Math.Pow(c / rho, 1.0 / n)) - ProjectionMath.PiHalf;
                else
                    coord.Y = ProjectionMath.Phi2(Math.Pow(rho / c, 1.0 / n), Eccentricity);
                coord.X = Math.Atan2(x, y) / n;
            }
            else
            {
                coord.X = 0.0;
                coord.Y = n > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
            }
            return coord;
        }

        public override void Initialize()
        {
            base.Initialize();
            double cosphi, sinphi;
            Boolean secant;

            if (ProjectionLatitude1 == 0)
                ProjectionLatitude1 = ProjectionLatitude2 = ProjectionLatitude;

            if (Math.Abs(ProjectionLatitude1 + ProjectionLatitude2) < 1e-10)
                throw new ProjectionException();
            n = sinphi = Math.Sin(ProjectionLatitude1);
            cosphi = Math.Cos(ProjectionLatitude1);
            secant = Math.Abs(ProjectionLatitude1 - ProjectionLatitude2) >= 1e-10;
            _spherical = (EccentricitySquared == 0.0);
            if (!Spherical)
            {
                double ml1, m1;

                m1 = ProjectionMath.msfn(sinphi, cosphi, EccentricitySquared);
                ml1 = ProjectionMath.tsfn(ProjectionLatitude1, sinphi, Eccentricity);
                if (secant)
                {
                    n = Math.Log(m1 /
                       ProjectionMath.msfn(sinphi = Math.Sin(ProjectionLatitude2), Math.Cos(ProjectionLatitude2), EccentricitySquared));
                    n /= Math.Log(ml1 / ProjectionMath.tsfn(ProjectionLatitude2, sinphi, Eccentricity));
                }
                c = (rho0 = m1 * Math.Pow(ml1, -n) / n);
                rho0 *= (Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) < 1e-10) ? 0.0 :
                    Math.Pow(ProjectionMath.tsfn(ProjectionLatitude, Math.Sin(ProjectionLatitude), Eccentricity), n);
            }
            else
            {
                if (secant)
                    n = Math.Log(cosphi / Math.Cos(ProjectionLatitude2)) /
                       Math.Log(Math.Tan(ProjectionMath.PiFourth + .5 * ProjectionLatitude2) /
                       Math.Tan(ProjectionMath.PiFourth + .5 * ProjectionLatitude1));
                c = cosphi * Math.Pow(Math.Tan(ProjectionMath.PiFourth + .5 * ProjectionLatitude1), n) / n;
                rho0 = (Math.Abs(Math.Abs(ProjectionLatitude) - ProjectionMath.PiHalf) < 1e-10) ? 0.0 :
                    c * Math.Pow(Math.Tan(ProjectionMath.PiFourth + .5 * ProjectionLatitude), -n);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this projection is conformal
        /// </summary>
        public override Boolean IsConformal
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this projection has an inverse
        /// </summary>
        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Lambert Conformal Conic";
        }

    }

}
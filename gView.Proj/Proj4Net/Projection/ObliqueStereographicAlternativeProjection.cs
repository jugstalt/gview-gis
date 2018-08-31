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

using System;

namespace Proj4Net.Projection
{
    public class ObliqueStereographicAlternativeProjection : GaussProjection
    {

        private double sinc0;
        private double cosc0;
        private double R2;

        public ObliqueStereographicAlternativeProjection()
        {
            Initialize();
        }

        public /*override*/ GeoAPI.Geometries.Coordinate ProjectOld(double x, double y, GeoAPI.Geometries.Coordinate dst)
        {
            base.Project(x, y, dst );
            double px = dst.X;
            double py = dst.Y;
            double sinc = Math.Sin(py);
            double cosc = Math.Cos(py);
            double cosl = Math.Cos(px);
            double k = _scaleFactor*R2/(1.0 + sinc0*sinc + cosc0*cosc*cosl);
            dst.X = k*cosc*Math.Sin(px);
            dst.Y = k*(this.cosc0*sinc - this.sinc0*cosc*cosl);
            return dst;
        }

        public override GeoAPI.Geometries.Coordinate Project(double lplamIn, double lpphiIn, GeoAPI.Geometries.Coordinate dst)
        {
            base.Project(lplamIn, lpphiIn, dst);
            double lplam = dst.X;
            double lpphi = dst.Y;
            double sinc = Math.Sin(lpphi);
            double cosc = Math.Cos(lpphi);
            double cosl = Math.Cos(lplam);
            double k = _scaleFactor*R2/(1.0 + sinc0*sinc + cosc0*cosc*cosl);
            dst.X = k*cosc*Math.Sin(lplam);
            dst.Y = k*(cosc0*sinc - sinc0*cosc*cosl);
            return dst;
        }

        public override GeoAPI.Geometries.Coordinate ProjectInverse(double x, double y, GeoAPI.Geometries.Coordinate dst)
        {
            double xyx = x/_scaleFactor;
            double xyy = y/_scaleFactor;
            double rho = Math.Sqrt(xyx*xyx + xyy*xyy);
            double lpphi;
            double lplam;
            if (rho != 0)
            {
                double c = 2.0*Math.Atan2(rho, R2);
                double sinc = Math.Sin(c);
                double cosc = Math.Cos(c);
                lpphi = Math.Asin(cosc*sinc0 + xyy*sinc*cosc0/rho);
                lplam = Math.Atan2(xyx*sinc, rho*cosc0*cosc -
                                             xyy*sinc0*sinc);
            }
            else
            {
                lpphi = phic0;
                lplam = 0.0;
            }
            return base.ProjectInverse(lplam, lpphi, dst);
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override void Initialize()
        {
            base.Initialize();
            sinc0 = Math.Sin(phic0);
            cosc0 = Math.Cos(phic0);
            R2 = 2.0*rc;
        }

        public override String ToString()
        {
            return "Oblique Stereographic Alternative";
        }

    }
}
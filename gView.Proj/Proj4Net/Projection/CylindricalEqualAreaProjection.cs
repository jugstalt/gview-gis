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

    public class CylindricalEqualAreaProjection : Projection
    {

        private double _qp;
        private double[] _apa;
        private double trueScaleLatitude;

        public CylindricalEqualAreaProjection()
            : this(0.0, 0.0, 0.0)
        {
        }

        public CylindricalEqualAreaProjection(double projectionLatitude, double projectionLongitude, double trueScaleLatitude)
        {
            ProjectionLatitude = projectionLatitude;
            ProjectionLongitude = projectionLongitude;
            TrueScaleLatitude = trueScaleLatitude;
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            double t = TrueScaleLatitude;

            ScaleFactor = Math.Cos(t);
            if (EccentricitySquared != 0)
            {
                t = Math.Sin(t);
                ScaleFactor /= Math.Sqrt(1.0 - EccentricitySquared * t * t);
                _apa = ProjectionMath.AuthSet(EccentricitySquared);
                _qp = ProjectionMath.Qsfn(1.0, Eccentricity, _oneEs);
            }
        }

        public override Coordinate Project(double lam, double phi, Coordinate xy)
        {
            if (Spherical)
            {
                xy.X = ScaleFactor * lam;
                xy.Y = Math.Sin(phi) / ScaleFactor;
            }
            else
            {
                xy.X = ScaleFactor * lam;
                xy.Y = 0.5 * ProjectionMath.Qsfn(Math.Sin(phi), Eccentricity, _oneEs) / ScaleFactor;
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double x, double y, Coordinate lp)
        {
            if (Spherical)
            {
                double t;

                if ((t = Math.Abs(y *= ScaleFactor)) - EPS10 <= 1.0)
                {
                    if (t >= 1.0)
                        lp.Y = y < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
                    else
                        lp.Y = Math.Asin(y);
                    lp.X = x / ScaleFactor;
                }
                else throw new ProjectionException();
            }
            else
            {
                lp.Y = ProjectionMath.AuthLat(Math.Asin(2.0 * y * ScaleFactor / _qp), _apa);
                lp.X = x / ScaleFactor;
            }
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override Boolean IsRectilinear
        {
            get { return true; }
        }

    }

}
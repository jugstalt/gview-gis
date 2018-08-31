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

    public class SineTangentSeriesProjection : ConicProjection
    {
        private readonly double _cX;
        private readonly double _cY;
        private readonly double _cP;
        private readonly Boolean _tanMode;

        protected SineTangentSeriesProjection(double p, double q, Boolean mode)
        {
            EccentricitySquared = 0.0;
            _cX = q / p;
            _cY = p;
            _cP = 1 / q;
            _tanMode = mode;
            Initialize();
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double c;

            xy.X = _cX * lplam * Math.Cos(lpphi);
            xy.Y = _cY;
            lpphi *= _cP;
            c = Math.Cos(lpphi);
            if (_tanMode)
            {
                xy.X *= c * c;
                xy.Y *= Math.Tan(lpphi);
            }
            else
            {
                xy.X /= c;
                xy.Y *= Math.Sin(lpphi);
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double c;

            xyy /= _cY;
            c = Math.Cos(lp.Y = _tanMode ? Math.Atan(xyy) : ProjectionMath.Asin(xyy));
            lp.Y /= _cP;
            lp.X = xyx / (_cX * Math.Cos(lp.Y /= _cP));
            if (_tanMode)
                lp.X /= c * c;
            else
                lp.X *= c;
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

    }
}
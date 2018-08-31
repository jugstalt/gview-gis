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

    public class Eckert4Projection : Projection
    {

        private const double C_x = .42223820031577120149;
        private const double C_y = 1.32650042817700232218;
        private const double RC_y = .75386330736002178205;
        private const double C_p = 3.57079632679489661922;
        private const double RC_p = .28004957675577868795;
        private const double EPS = 1e-7;
        private const int NITER = 6;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double p, V, s, c;
            int i;

            p = C_p * Math.Sin(lpphi);
            V = lpphi * lpphi;
            lpphi *= 0.895168 + V * (0.0218849 + V * 0.00826809);
            for (i = NITER; i > 0; --i)
            {
                c = Math.Cos(lpphi);
                s = Math.Sin(lpphi);
                lpphi -= V = (lpphi + s * (c + 2.0) - p) /
                    (1.0 + c * (c + 2.0) - s * s);
                if (Math.Abs(V) < EPS)
                    break;
            }
            if (i == 0)
            {
                xy.X = C_x * lplam;
                xy.Y = lpphi < 0.0 ? -C_y : C_y;
            }
            else
            {
                xy.X = C_x * lplam * (1.0 + Math.Cos(lpphi));
                xy.Y = C_y * Math.Sin(lpphi);
            }
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double c;

            lp.Y = ProjectionMath.Asin(xyy / C_y);
            lp.X = xyx / (C_x * (1.0 + (c = Math.Cos(lp.Y))));
            lp.Y = ProjectionMath.Asin((lp.Y + Math.Sin(lp.Y) * (c + 2.0)) / C_p);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if this projection is equal area
        /// </summary>
        public override Boolean IsEqualArea
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Eckert IV";
        }

    }
}
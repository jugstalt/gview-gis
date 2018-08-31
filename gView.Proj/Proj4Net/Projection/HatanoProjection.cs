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

    public class HatanoProjection : Projection
    {

        private const int NITER = 20;
        private const double EPS = 1e-7;
        private const double ONETOL = 1.000001;
        private const double CN = 2.67595;
        private const double CS = 2.43763;
        private const double RCN = 0.37369906014686373063;
        private const double RCS = 0.41023453108141924738;
        private const double FYCN = 1.75859;
        private const double FYCS = 1.93052;
        private const double RYCN = 0.56863737426006061674;
        private const double RYCS = 0.51799515156538134803;
        private const double FXC = 0.85;
        private const double RXC = 1.17647058823529411764;

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double th1, c;
            int i;

            c = Math.Sin(lpphi) * (lpphi < 0.0 ? CS : CN);
            for (i = NITER; i > 0; --i)
            {
                lpphi -= th1 = (lpphi + Math.Sin(lpphi) - c) / (1.0 + Math.Cos(lpphi));
                if (Math.Abs(th1) < EPS) break;
            }
            xy.X = FXC * lplam * Math.Cos(lpphi *= .5);
            xy.Y = Math.Sin(lpphi) * (lpphi < 0.0 ? FYCS : FYCN);
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double th;

            th = xyy * (xyy < 0.0 ? RYCS : RYCN);
            if (Math.Abs(th) > 1.0)
                if (Math.Abs(th) > ONETOL) throw new ProjectionException("I");
                else th = th > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
            else
                th = Math.Asin(th);
            lp.X = RXC * xyx / Math.Cos(th);
            th += th;
            lp.Y = (th + Math.Sin(th)) * (xyy < 0.0 ? RCS : RCN);
            if (Math.Abs(lp.Y) > 1.0)
                if (Math.Abs(lp.Y) > ONETOL) throw new ProjectionException("I");
                else lp.Y = lp.Y > 0.0 ? ProjectionMath.PiHalf : -ProjectionMath.PiHalf;
            else
                lp.Y = Math.Asin(lp.Y);
            return lp;
        }

        public override Boolean HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Hatano Asymmetrical Equal Area";
        }

    }

}
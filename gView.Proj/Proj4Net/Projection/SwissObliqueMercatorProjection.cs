/*
Copyright 2006 Martin Davis

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
     * This file was converted from the PROJ.4 source.
     */
    /**
    * Swiss Oblique Mercator Projection algorithm is taken from the USGS PROJ.4 package.
    */

    public class SwissObliqueMercatorProjection : Projection
    {

        private const int NITER = 6;

        private double K, c, hlf_e, kR, cosp0, sinp0;
        private double phi0;

        public SwissObliqueMercatorProjection()
        {
            //initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            double cp, phip0, sp;

            phi0 = _projectionLatitude;

            hlf_e = 0.5*_e;
            cp = Math.Cos(phi0);
            cp *= cp;
            c = Math.Sqrt(1 + _es*cp*cp*_roneEs);
            sp = Math.Sin(phi0);
            cosp0 = Math.Cos(phip0 = Math.Asin(sinp0 = sp/c));
            sp *= _e;
            K = Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5*phip0)) - c*(
                                                                                Math.Log(
                                                                                    Math.Tan(ProjectionMath.PiFourth +
                                                                                             0.5*phi0)) - hlf_e*
                                                                                Math.Log((1.0 + sp)/(1.0 - sp)));
            kR = _scaleFactor*Math.Sqrt(_oneEs)/(1.0 - sp*sp);
        }

        public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
        {
            double phip, lamp, phipp, lampp, sp, cp;

            sp = _e*Math.Sin(lpphi);
            phip = 2.0*Math.Atan(Math.Exp(c*(
                                                Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5*lpphi)) -
                                                hlf_e*Math.Log((1.0 + sp)/(1.0 - sp)))
                                          + K)) - ProjectionMath.PiHalf;
            lamp = c*lplam;
            cp = Math.Cos(phip);
            phipp = Math.Asin(cosp0*Math.Sin(phip) - sinp0*cp*Math.Cos(lamp));
            lampp = Math.Asin(cp*Math.Sin(lamp)/Math.Cos(phipp));
            xy.X = kR*lampp;
            xy.Y = kR*Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5*phipp));
            return xy;
        }

        public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
        {
            double phip, lamp, phipp, lampp, cp, esp, con, delp;
            int i;
            double lplam, lpphi;

            phipp = 2.0*(Math.Atan(Math.Exp(xyy/kR)) - ProjectionMath.PiFourth);
            lampp = xyx/kR;
            cp = Math.Cos(phipp);
            phip = Math.Asin(cosp0*Math.Sin(phipp) + sinp0*cp*Math.Cos(lampp));
            lamp = Math.Asin(cp*Math.Sin(lampp)/Math.Cos(phip));
            con = (K - Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5*phip)))/c;
            for (i = NITER; i != 0; --i)
            {
                esp = _e*Math.Sin(phip);
                delp = (con + Math.Log(Math.Tan(ProjectionMath.PiFourth + 0.5*phip)) - hlf_e*
                        Math.Log((1.0 + esp)/(1.0 - esp)))*
                       (1.0 - esp*esp)*Math.Cos(phip)*_roneEs;
                phip -= delp;
                if (Math.Abs(delp) < ProjectionMath.EPS10)
                    break;
            }
            if (i != 0)
            {
                lpphi = phip;
                lplam = lamp/c;
            }
            else
            {
                throw new ProjectionException("I_ERROR");
            }
            lp.X = lplam;
            lp.Y = lpphi;

            return lp;
        }

        public override bool HasInverse
        {
            get { return true; }
        }

        public override String ToString()
        {
            return "Swiss Oblique Mercator";
        }

    }
}
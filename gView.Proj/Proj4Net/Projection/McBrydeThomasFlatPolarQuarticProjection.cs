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

public class McBrydeThomasFlatPolarQuarticProjection : Projection {

	private const int NITER = 20;
	private const double EPS = 1e-7;
	private const double ONETOL = 1.000001;
	private const double C = 1.70710678118654752440;
	private const double RC = 0.58578643762690495119;
	private const double FYC = 1.87475828462269495505;
	private const double RYC = 0.53340209679417701685;
	private const double FXC = 0.31245971410378249250;
	private const double RXC = 3.20041258076506210122;

	public override Coordinate Project(double lplam, double lpphi, Coordinate xy) {
		double th1, c;
		int i;

		c = C * Math.Sin(lpphi);
		for (i = NITER; i > 0; --i) {
			xy.Y -= th1 = (Math.Sin(.5*lpphi) + Math.Sin(lpphi) - c) /
				(.5*Math.Cos(.5*lpphi)  + Math.Cos(lpphi));
			if (Math.Abs(th1) < EPS) break;
		}
		xy.X = FXC * lplam * (1.0 + 2.0 * Math.Cos(lpphi)/Math.Cos(0.5 * lpphi));
		xy.Y = FYC * Math.Sin(0.5 * lpphi);
		return xy;
	}

	public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp) {
		double t = 0;

		double lpphi = RYC * xyy;
		if (Math.Abs(lpphi) > 1.0) {
			if (Math.Abs(lpphi) > ONETOL)	throw new ProjectionException("I");
			else if (lpphi < 0.0) { t = -1.0; lpphi = -Math.PI; }
			else { t = 1.0; lpphi = Math.PI; }
		} else
			lpphi = 2.0 * Math.Asin(t = lpphi);
		lp.X = RXC * xyx / (1.0 + 2.0 * Math.Cos(lpphi)/Math.Cos(0.5 * lpphi));
		lpphi = RC * (t + Math.Sin(lpphi));
		if (Math.Abs(lpphi) > 1.0)
			if (Math.Abs(lpphi) > ONETOL)
				throw new ProjectionException("I");
			else
				lpphi = lpphi < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
		else
			lpphi = Math.Asin(lpphi);
		lp.Y = lpphi;
		return lp;
	}

	public override Boolean HasInverse
    {
		get
		{
		    return true;
		}
	}

    public override bool IsEqualArea
    {
        get
        {
            return true;
        }
    }

	public override String ToString() {
		return "McBryde-Thomas Flat-Polar Quartic";
	}

}
}
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


public class LoximuthalProjection : PseudoCylindricalProjection {

	private const double FC = .92131773192356127802;
	private const double RP = .31830988618379067154;
	private const double EPS = 1e-8;
	
	private double phi1;
	private double cosphi1;
	private double tanphi1;

	public LoximuthalProjection() {
		phi1 = ProjectionMath.ToRadians(40.0);//FIXME - param
		cosphi1 = Math.Cos(phi1);
		tanphi1 = Math.Tan(ProjectionMath.PiFourth + 0.5 * phi1);
	}

	public override Coordinate Project(double lplam, double lpphi, Coordinate xy) 
    {
		double x;
		double y = lpphi - phi1;
		if (y < EPS)
			x = lplam * cosphi1;
		else {
			x = ProjectionMath.PiFourth + 0.5 * lpphi;
			if (Math.Abs(x) < EPS || Math.Abs(Math.Abs(x) - ProjectionMath.PiHalf) < EPS)
				x = 0.0;
			else
				x = lplam * y / Math.Log( Math.Tan(x) / tanphi1 );
		}
		xy.X = x;
		xy.Y = y;
		return xy;
	}

	public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp) {
		double latitude = xyy + phi1;
		double longitude;
		if (Math.Abs(xyy) < EPS)
			longitude = xyx / cosphi1;
		else if (Math.Abs( longitude = ProjectionMath.PiFourth + 0.5 * xyy ) < EPS ||
			Math.Abs(Math.Abs(xyx) -ProjectionMath.PiHalf) < EPS)
			longitude = 0.0;
		else
			longitude = xyx * Math.Log( Math.Tan(longitude) / tanphi1 ) / xyy;

		lp.X = longitude;
		lp.Y = latitude;
		return lp;
	}

	public override Boolean HasInverse {
		get {return true;}
	}

	public override String ToString() {
		return "Loximuthal";
	}

}
}
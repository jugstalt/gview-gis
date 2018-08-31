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

namespace Proj4Net.Projection
{


public class AitoffProjection : PseudoCylindricalProjection
{
	
	protected const int Aitoff = 0;
	protected const int Winkel = 1;

	private readonly Boolean _winkel;
	private double _cosphi1;

	public AitoffProjection() {
	}

	public AitoffProjection(int type, double projectionLatitude) {
		ProjectionLatitude = projectionLatitude;
		_winkel = type == Winkel;
	}

	public override Coordinate Project(double lplam, double lpphi, Coordinate coord)
    {
		double c = 0.5 * lplam;
		double d = Math.Acos(Math.Cos(lpphi) * Math.Cos(c));

		if (d != 0) {
			coord.X = 2.0 * d * Math.Cos(lpphi) * Math.Sin(c) * (coord.Y = 1.0 / Math.Sin(d));
			coord.Y *= d * Math.Sin(lpphi);
		} else
			coord.X = coord.Y = 0.0;
		if (_winkel) {
			coord.X = (coord.X + lplam * _cosphi1) * 0.5;
			coord.Y = (coord.Y + lpphi) * 0.5;
		}
		return coord;
	}

	public override void Initialize()
    {
		base.Initialize();
		if (_winkel) {
//FIXME
//			if (pj_param(P->params, "tlat_1").i)
//				if ((_cosphi1 = Math.cos(pj_param(P->params, "rlat_1").f)) == 0.)
//					throw new IllegalArgumentException("-22")
//			else /* 50d28' or acos(2/pi) */
				_cosphi1 = 0.636619772367581343;
		}
	}
	
	public override Boolean HasInverse
    {
		get { return false; }
    }

	public override String ToString()
	{
	    return _winkel ? "Winkel Tripel" : "Aitoff";
	}

}

}
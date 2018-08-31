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


public class PutninsP5Projection : Projection {

	protected double A;
	protected double B;

	private const double C = 1.01346;
	private const double D = 1.2158542;

	public PutninsP5Projection() {
		A = 2;
		B = 1;
	}

    public override Coordinate Project(double lplam, double lpphi, Coordinate xy)
    {
		xy.X = C * lplam * (A - B * Math.Sqrt(1.0 + D * lpphi * lpphi));
		xy.Y = C * lpphi;
		return xy;
	}

    public override Coordinate ProjectInverse(double xyx, double xyy, Coordinate lp)
    {
		lp.Y = xyy / C;
		lp.X = xyx / (C * (A - B * Math.Sqrt(1.0 + D * lp.Y * lp.Y)));
		return lp;
	}

	public override Boolean HasInverse {
        get { return true; }
	}

	public override String ToString() {
		return "Putnins P5";
	}

}
}
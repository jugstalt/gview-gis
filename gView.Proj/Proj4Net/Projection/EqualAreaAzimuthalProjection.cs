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
using Proj4Net.Utility;

namespace Proj4Net.Projection
{


public class EqualAreaAzimuthalProjection : AzimuthalProjection {

	private double _sinb1;
	private double _cosb1;
	private double _xmf;
	private double _ymf;
	private double _mmf;
	private double _qp;
	private double _dd;
	private double _rq;
	private double[] _apa;

	public EqualAreaAzimuthalProjection() {
		Initialize();
	}

	public override  Object Clone() {
		EqualAreaAzimuthalProjection p = (EqualAreaAzimuthalProjection)base.Clone();
		if (_apa != null)
			p._apa = (double[])_apa.Clone();
		return p;
	}
	
	public override void Initialize()
    {
		base.Initialize();
		if (Spherical) {
			if (_mode == AzimuthalMode.Oblique) {
				_sinphi0 = Math.Sin(ProjectionLatitude);
				_cosphi0 = Math.Cos(ProjectionLatitude);
			}
		} else {
			double sinphi;

			_qp = ProjectionMath.Qsfn(1.0, Eccentricity, _oneEs);
			_mmf = .5 / (1.0 - EccentricitySquared);
			_apa = ProjectionMath.AuthSet(EccentricitySquared);
			switch (_mode)
            {
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
				_dd = 1.0;
				break;
                case AzimuthalMode.Equator:
				_dd = 1.0 / (_rq = Math.Sqrt(.5 * _qp));
				_xmf = 1.0;
				_ymf = .5 * _qp;
				break;
                case AzimuthalMode.Oblique:
				_rq = Math.Sqrt(.5 * _qp);
				sinphi = Math.Sin(ProjectionLatitude);
				_sinb1 = ProjectionMath.Qsfn(sinphi, Eccentricity, _oneEs) / _qp;
				_cosb1 = Math.Sqrt(1.0 - _sinb1 * _sinb1);
				_dd = Math.Cos(ProjectionLatitude) / (Math.Sqrt(1.0 - EccentricitySquared * sinphi * sinphi) *
				   _rq * _cosb1);
				_ymf = (_xmf = _rq) / _dd;
				_xmf *= _dd;
				break;
			}
		}
	}

    public ProjCoordinate project(double lam, double phi, ProjCoordinate xy)
    {
        if (Spherical)
        {
            double coslam, cosphi, sinphi;

            sinphi = Math.Sin(phi);
            cosphi = Math.Cos(phi);
            coslam = Math.Cos(lam);
            switch (Mode)
            {
                case AzimuthalMode.Equator:
                    xy.Y = 1.0 + cosphi*coslam;
                    if (xy.Y <= EPS10) throw new ProjectionException();
                    xy.X = (xy.Y = Math.Sqrt(2.0/xy.Y))*cosphi*Math.Sin(lam);
                    xy.Y *= Mode == AzimuthalMode.Equator
                                ? sinphi
                                :
                                    _cosphi0*sinphi - _sinphi0*cosphi*coslam;
                    break;
                case AzimuthalMode.Oblique:
                    xy.Y = 1.0
                           + _sinphi0*sinphi + _cosphi0*cosphi*coslam;
                    if (xy.Y <= EPS10) throw new ProjectionException();
                    xy.X = (xy.Y = Math.Sqrt(2.0/xy.Y))
                           *cosphi*Math.Sin(lam);
                    xy.Y *= Mode == AzimuthalMode.Equator
                                ? sinphi
                                :
                                    _cosphi0*sinphi - _sinphi0*cosphi*coslam;
                    break;
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (Mode == AzimuthalMode.NorthPole) coslam = -coslam;
                    if (Math.Abs(phi + ProjectionLatitude) < EPS10) throw new ProjectionException();
                    xy.Y = ProjectionMath.PiFourth - phi*.5;
                    xy.Y = 2.0
                           *(Mode == AzimuthalMode.SouthPole ? Math.Cos(xy.Y) : Math.Sin(xy.Y));
                    xy.X = xy.Y*Math.Sin(lam);
                    xy.Y *= coslam;
                    break;
            }
        }
        else
        {
            double coslam, sinlam, sinphi, q, sinb = 0, cosb = 0, b = 0;

            coslam = Math.Cos(lam);
            sinlam = Math.Sin(lam);
            sinphi = Math.Sin(phi);
            q = ProjectionMath.Qsfn(sinphi, Eccentricity, _oneEs);
            if (Mode == AzimuthalMode.Oblique || Mode == AzimuthalMode.Equator)
            {
                sinb = q/_qp;
                cosb = Math.Sqrt(1.0 - sinb*sinb);
            }
            switch (Mode)
            {
                case AzimuthalMode.Oblique:
                    b = 1.0 + _sinb1*sinb + _cosb1*cosb*coslam;
                    break;
                case AzimuthalMode.Equator:
                    b = 1.0 + cosb*coslam;
                    break;
                case AzimuthalMode.NorthPole:
                    b = ProjectionMath.PiHalf + phi;
                    q = _qp - q;
                    break;
                case AzimuthalMode.SouthPole:
                    b = phi - ProjectionMath.PiHalf;
                    q = _qp + q;
                    break;
            }
            if (Math.Abs(b) < EPS10) throw new ProjectionException();
            switch (Mode)
            {
                case AzimuthalMode.Oblique:
                    xy.Y = _ymf*(b = Math.Sqrt(2.0/b))
                           *(_cosb1*sinb - _sinb1*cosb*coslam);
                    xy.X = _xmf*b*cosb*sinlam;
                    break;
                case AzimuthalMode.Equator:
                    xy.Y = (b = Math.Sqrt(2.0/(1.0 + cosb*coslam)))*sinb*_ymf;
                    xy.X = _xmf*b*cosb*sinlam;
                    break;
                case AzimuthalMode.NorthPole:
                case AzimuthalMode.SouthPole:
                    if (q >= 0.0)
                    {
                        xy.X = (b = Math.Sqrt(q))*sinlam;
                        xy.Y = coslam*(Mode == AzimuthalMode.SouthPole ? b : -b);
                    }
                    else
                        xy.X = xy.Y = 0.0;
                    break;
            }
        }
        return xy;
    }

    public ProjCoordinate projectInverse(double x, double y, ProjCoordinate lp) {
		if (Spherical) {
			double  cosz = 0, rh, sinz = 0;

			rh = ProjectionMath.Distance(x, y);
			if ((lp.Y = rh * .5 ) > 1.0) throw new ProjectionException();
			lp.Y = 2.0 * Math.Asin(lp.Y);
			if (Mode == AzimuthalMode.Oblique || Mode == AzimuthalMode.Equator) {
				sinz = Math.Sin(lp.Y);
				cosz = Math.Cos(lp.Y);
			}
			switch (Mode) {
			case AzimuthalMode.Equator:
				lp.Y = Math.Abs(rh) <= EPS10 ? 0.0 : Math.Asin(y * sinz / rh);
				x *= sinz;
				y = cosz * rh;
				break;
			case AzimuthalMode.Oblique:
				lp.Y = Math.Abs(rh) <= EPS10 ? ProjectionLatitude :
				   Math.Asin(cosz * _sinphi0 + y * sinz * _cosphi0 / rh);
				x *= sinz * _cosphi0;
				y = (cosz - Math.Sin(lp.Y) * _sinphi0) * rh;
				break;
			case AzimuthalMode.NorthPole:
				y = -y;
				lp.Y = ProjectionMath.PiHalf - lp.Y;
				break;
			case AzimuthalMode.SouthPole:
				lp.Y -= ProjectionMath.PiHalf;
				break;
			}
			lp.X = (y == 0.0 && (Mode == AzimuthalMode.Equator || Mode == AzimuthalMode.Oblique)) ?
				0.0 : Math.Atan2(x, y);
		} else {
			double cCe, sCe, q, rho, ab = 0;

			switch (Mode) {
			case AzimuthalMode.Equator:
			case AzimuthalMode.Oblique:
				if ((rho = ProjectionMath.Distance(x /= _dd, y *=  _dd)) < EPS10) {
					lp.X = 0.0;
					lp.Y = ProjectionLatitude;
					return (lp);
				}
				cCe = Math.Cos(sCe = 2.0 * Math.Asin(.5 * rho / _rq));
				x *= (sCe = Math.Sin(sCe));
				if (Mode == AzimuthalMode.Oblique) {
					q = _qp * (ab = cCe * _sinb1 + y * sCe * _cosb1 / rho);
					y = rho * _cosb1 * cCe - y * _sinb1 * sCe;
				} else {
					q = _qp * (ab = y * sCe / rho);
					y = rho * cCe;
				}
				break;
			case AzimuthalMode.NorthPole:
			case AzimuthalMode.SouthPole:
				if (Mode == AzimuthalMode.NorthPole) y = -y;
				if ((q = (x * x + y * y)) == 0) {
					lp.X = 0.0;
					lp.Y = ProjectionLatitude;
					return lp;
				}
				ab = 1.0 - q / _qp;
				if (Mode == AzimuthalMode.SouthPole)
					ab = - ab;
				break;
			}
			lp.X = Math.Atan2(x, y);
			lp.Y = ProjectionMath.AuthLat(Math.Asin(ab), _apa);
		}
		return lp;
	}

    /*
	public Shape getBoundingShape() {
		double r = 1.414 * a;
		return new Ellipse2D.Double( -r, -r, 2*r, 2*r );
	}
    */
	/**
	 * Returns true if this projection is equal area
	 */
	public override Boolean IsEqualArea {
		get{return true;}
	}

	public override Boolean HasInverse {
		get{return true;}
	}

	public override String ToString() {
		return "Lambert Equal Area Azimuthal";
	}

}
}
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

namespace Proj4Net.Utility
{

    public class ProjectionMath
    {

        public const double Pi = Math.PI;
        public const double PiHalf = Math.PI / 2.0;
        [Obsolete("Use PiHalf")] public const double PIHalf = PiHalf;
        public const double PiFourth = Math.PI/4.0;
        [Obsolete("Use PiFourth")] public const double PIFourth = PiFourth;
        [Obsolete("Use PiFourth")]
        public const double FORTPI = PiFourth;

        public const double TwoPI = Math.PI * 2.0;
        public const double RadiansToDegrees = 180.0 / Math.PI;
        public const double DegreesToRadians = Math.PI / 180.0;
        //public readonly static Rectangle2D WorldBoundsRad = new Rectangle2D.Double(-Math.PI, -Math.PI/2, Math.PI*2, Math.PI);
        //public readonly static Rectangle2D WorldBounds = new Rectangle2D.Double(-180, -90, 360, 180);

	    public const double EPS10 = 1.0e-10;

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">radians</param>
        /// <returns>degrees</returns>
        public static double ToDegrees(double radians) { return radians * RadiansToDegrees; }

        /// <summary>
        /// converts degrees to radians
        /// </summary>
        /// <param name="degrees">degrees</param>
        /// <returns>returns</returns>
        public static double ToRadians(double degrees) { return degrees * DegreesToRadians; }

        ///<summary>Degree versions of trigonometric function: Math.Sin</summary>
        public static double Sind(double v)
        {
            return Math.Sin(v * DegreesToRadians);
        }

        ///<summary>Degree versions of trigonometric function: Math.Cos</summary>
        public static double Cosd(double v)
        {
            return Math.Cos(v * DegreesToRadians);
        }

        ///<summary>Degree versions of trigonometric function: Math.Tan</summary>
        public static double Tand(double v)
        {
            return Math.Tan(v * DegreesToRadians);
        }

        ///<summary>Degree versions of trigonometric function: Math.Asin</summary>
        public static double Asind(double v)
        {
            return Math.Asin(v) * RadiansToDegrees;
        }

        ///<summary>Degree versions of trigonometric function: Math.Acos</summary>
        public static double Acosd(double v)
        {
            return Math.Acos(v) * RadiansToDegrees;
        }

        ///<summary>Degree versions of trigonometric function: Math.Atan</summary>
        public static double Atand(double v)
        {
            return Math.Atan(v) * RadiansToDegrees;
        }

        ///<summary>Degree versions of trigonometric function: Math.Atan2</summary>
        public static double Atan2d(double y, double x)
        {
            return Math.Atan2(y, x) * RadiansToDegrees;
        }

        ///<summary>Input range limited versions of trigonometric function: Math.Asin [-1.0, 1.0]</summary>
        public static double Asin(double v)
        {
            if (Math.Abs(v) > 1.0d)
                return v < 0.0 ? -ProjectionMath.PiHalf : ProjectionMath.PiHalf;
            return Math.Asin(v);
        }

        ///<summary>Input range limited versions of trigonometric function: Math.Acos [-1.0, 1.0]</summary>
        public static double Acos(double v)
        {
            if (Math.Abs(v) > 1.0d)
                return v < 0.0 ? Pi : 0.0;
            return Math.Acos(v);
        }

        public static double Sqrt(double v)
        {
            return v < 0.0 ? 0.0 : Math.Sqrt(v);
        }

        public static double Distance(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// Length of the Hypothenuse
        /// </summary>
        /// <param name="x">x-ordinate</param>
        /// <param name="y">y-ordinate</param>
        /// <returns>Length of hypothenuse</returns>
        public static double Hypot(double x, double y)
        {
            if (x < 0.0)
                x = -x;
            else if (x == 0.0)
                return y < 0.0 ? -y : y;
            if (y < 0.0)
                y = -y;
            else if (y == 0.0)
                return x;
            if (x < y)
            {
                x /= y;
                return y * Math.Sqrt(1.0 + x * x);
            }
            y /= x;
            return x * Math.Sqrt(1.0 + y * y);
        }
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }
        */
        /// <summary>
        /// Truncates remainder
        /// </summary>
        /// <param name="v">value to truncate</param>
        /// <returns>truncated value of v</returns>
        /// <example>Trunc(12.5) == 12;</example>
        /// <example>Trunc(-12.5) == -12;</example>
        public static double Trunc(double v)
        {
            return v < 0.0 ? Math.Ceiling(v) : Math.Floor(v);
        }

        public static double Frac(double v)
        {
            return v - Trunc(v);
        }

        [Obsolete("Use ToRadians(Double value)")]
        public static double DegreesToRadiansFn(double v)
        {
            return v * DegreesToRadians;
        }

        [Obsolete("Use ToDegrees(Double value)")]
        public static double RadiansToDegreesFn(double v)
        {
            return v * RadiansToDegrees;
        }

        ///<summary>
        /// Converts degrees, minutes and seconds to angle in radians
        ///</summary>
        ///<param name="d">degree value (for negative angles, negative)</param>
        ///<param name="m">minutes value</param>
        /// <param name="s">seconds value</param>
        public static double DegreesMinutesSecondsToRadians(double d, double m, double s)
        {
            return DegreesMinutesSecondsToDegrees(d, m, s) * DegreesToRadians;
        }

        ///<summary>
        /// Converts degrees, minutes and seconds to angle in radians
        ///</summary>
        ///<param name="d">degree value (for negative angles, negative)</param>
        ///<param name="m">minutes value</param>
        /// <param name="s">seconds value</param>
        public static double DegreesMinutesSecondsToDegrees(double d, double m, double s)
        {
            if (d >= 0)
                return (d + m / 60 + s / 3600);
            return (d - m / 60 - s / 3600);
        }

        /// <summary>
        /// Normalizes angle to range of [-HalfPi, HalfPi]
        /// </summary>
        /// <param name="angle">angle to normalize in radians</param>
        /// <returns>normalized angle in radians</returns>
        public static double NormalizeLatitude(double angle)
        {
            if (Double.IsInfinity(angle) || Double.IsNaN(angle))
                throw new ArgumentException("Infinite latitude", "angle");
            while (angle > ProjectionMath.PiHalf)
                angle -= Math.PI;
            while (angle < -ProjectionMath.PiHalf)
                angle += Math.PI;
            return angle;
            //		return Math.IEEEremainder(angle, Math.PI);
        }

        /// <summary>
        /// Normalizes angle to range of [-PI, PI]
        /// </summary>
        /// <param name="angle">angle to normalize in radians</param>
        /// <returns>normalized angle in radians</returns>
        public static double NormalizeLongitude(double angle)
        {
            if (Double.IsInfinity(angle) || Double.IsNaN(angle))
                throw new ArgumentException("Infinite longitude", "angle");
            while (angle > Math.PI)
                angle -= TwoPI;
            while (angle < -Math.PI)
                angle += TwoPI;
            return angle;
            //		return Math.IEEEremainder(angle, Math.PI);
        }

        public static double NormalizeAngle(double angle)
        {
            if (Double.IsInfinity(angle) || Double.IsNaN(angle))
                throw new ArgumentException("Infinite angle", "angle");
            while (angle > TwoPI)
                angle -= TwoPI;
            while (angle < 0)
                angle += TwoPI;
            return angle;
        }

        /*
            public static void latLongToXYZ(Point2D.Double ll, Point3D xyz) {
                double c = Math.cos(ll.y);
                xyz.x = c * Math.cos(ll.x);
                xyz.y = c * Math.sin(ll.x);
                xyz.z = Math.sin(ll.y);
            }

            public static void xyzToLatLong(Point3D xyz, Point2D.Double ll) {
                ll.y = MapMath.asin(xyz.z);
                ll.x = MapMath.atan2(xyz.y, xyz.x);
            }
        */

        public static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double dlat = Math.Sin((lat2 - lat1) / 2);
            double dlon = Math.Sin((lon2 - lon1) / 2);
            double r = Math.Sqrt(dlat * dlat + Math.Cos(lat1) * Math.Cos(lat2) * dlon * dlon);
            return 2.0 * Math.Asin(r);
        }

        public static double SphericalAzimuth(double lat0, double lon0, double lat, double lon)
        {
            double diff = lon - lon0;
            double coslat = Math.Cos(lat);

            return Math.Atan2(
                coslat * Math.Sin(diff),
                (Math.Cos(lat0) * Math.Sin(lat) -
                Math.Sin(lat0) * coslat * Math.Cos(diff))
            );
        }

        public static Boolean SameSigns(double a, double b)
        {
            return a < 0 == b < 0;
        }

        public static Boolean SameSigns(int a, int b)
        {
            return a < 0 == b < 0;
        }

        public static double TakeSign(double a, double b)
        {
            a = Math.Abs(a);
            if (b < 0)
                return -a;
            return a;
        }

        public static int TakeSign(int a, int b)
        {
            a = Math.Abs(a);
            if (b < 0)
                return -a;
            return a;
        }
        /*
          public static double distance(Point2D.Double a, Point2D.Double b) {
            return distance(a.x-b.x, a.y-b.y);
          }

            public const int DONT_INTERSECT = 0;
            public const int DO_INTERSECT = 1;
            public const int COLLINEAR = 2;

            public static int intersectSegments(Point2D.Double aStart, Point2D.Double aEnd, Point2D.Double bStart, Point2D.Double bEnd, Point2D.Double p) {
                double a1, a2, b1, b2, c1, c2;
                double r1, r2, r3, r4;
                double denom, offset, num;

                a1 = aEnd.y-aStart.y;
                b1 = aStart.x-aEnd.x;
                c1 = aEnd.x*aStart.y - aStart.x*aEnd.y;
                r3 = a1*bStart.x + b1*bStart.y + c1;
                r4 = a1*bEnd.x + b1*bEnd.y + c1;

                if (r3 != 0 && r4 != 0 && sameSigns(r3, r4))
                    return DONT_INTERSECT;

                a2 = bEnd.y-bStart.y;
                b2 = bStart.x-bEnd.x;
                c2 = bEnd.x*bStart.y-bStart.x*bEnd.y;
                r1 = a2*aStart.x + b2*aStart.y + c2;
                r2 = a2*aEnd.x + b2*aEnd.y + c2;

                if (r1 != 0 && r2 != 0 && sameSigns(r1, r2))
                    return DONT_INTERSECT;

                denom = a1*b2 - a2*b1;
                if (denom == 0)
                    return COLLINEAR;

                offset = denom < 0 ? -denom/2 : denom/2;

                num = b1*c2 - b2*c1;
                p.x = (num < 0 ? num-offset : num+offset) / denom;

                num = a2*c1 - a1*c2;
                p.y = (num < 0 ? num-offset : num+offset) / denom;

                return DO_INTERSECT;
            }

          /*
            public static double dot(Point2D.Double a, Point2D.Double b) {
                return a.x*b.x + a.y*b.y;
            }
	
            public static Point2D.Double perpendicular(Point2D.Double a) {
                return new Point2D.Double(-a.y, a.x);
            }
	
            public static Point2D.Double add(Point2D.Double a, Point2D.Double b) {
                return new Point2D.Double(a.x+b.x, a.y+b.y);
            }
	
            public static Point2D.Double subtract(Point2D.Double a, Point2D.Double b) {
                return new Point2D.Double(a.x-b.x, a.y-b.y);
            }
	
            public static Point2D.Double multiply(Point2D.Double a, Point2D.Double b) {
                return new Point2D.Double(a.x*b.x, a.y*b.y);
            }
	
            public static double cross(Point2D.Double a, Point2D.Double b) {
                return a.x*b.y - b.x*a.y;
            }
  
          public static void normalize(Point2D.Double a) {
            double d = distance(a.x, a.y);
            a.x /= d;
            a.y /= d;
          }
  
          public static void negate(Point2D.Double a) {
            a.x = -a.x;
            a.y = -a.y;
          }
  

        */

        public static double Cross(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }

        public static double LongitudeDistance(double l1, double l2)
        {
            return Math.Min(
                Math.Abs(l1 - l2),
                ((l1 < 0) ? l1 + Math.PI : Math.PI - l1) + ((l2 < 0) ? l2 + Math.PI : Math.PI - l2)
            );
        }

        public static double GeocentricLatitude(double lat, double flatness)
        {
            double f = 1.0 - flatness;
            return Math.Atan((f * f) * Math.Tan(lat));
        }

        public static double GeographicLatitude(double lat, double flatness)
        {
            double f = 1.0 - flatness;
            return Math.Atan(Math.Tan(lat) / (f * f));
        }

        public static double tsfn(double phi, double sinphi, double e)
        {
            sinphi *= e;
            return (Math.Tan(.5 * (ProjectionMath.PiHalf - phi)) /
               Math.Pow((1d - sinphi) / (1d + sinphi), .5 * e));
        }

        public static double msfn(double sinphi, double cosphi, double es)
        {
            return cosphi / Math.Sqrt(1.0 - es * sinphi * sinphi);
        }

        private const int NumberOfIteratations = 15;

        public static double Phi2(double ts, double e)
        {
            double dphi;

            double eccnth = .5 * e;
            double phi = ProjectionMath.PiHalf - 2d * Math.Atan(ts);
            int i = NumberOfIteratations;
            do
            {
                double con = e * Math.Sin(phi);
                dphi = PiHalf - 2d * Math.Atan(ts * Math.Pow((1d - con) / (1d + con), eccnth)) - phi;
                phi += dphi;
            } while (Math.Abs(dphi) > 1e-10 && --i != 0);
            if (i <= 0)
                throw new ConvergenceFailureException("Computation of phi2 failed to converage after " + NumberOfIteratations + " iterations");
            return phi;
        }

        private const double C00 = 1.0;
        private const double C02 = .25;
        private const double C04 = .046875;
        private const double C06 = .01953125;
        private const double C08 = .01068115234375;
        private const double C22 = .75;
        private const double C44 = .46875;
        private const double C46 = .01302083333333333333;
        private const double C48 = .00712076822916666666;
        private const double C66 = .36458333333333333333;
        private const double C68 = .00569661458333333333;
        private const double C88 = .3076171875;
        private const int MaxIter = 10;

        public static double[] enfn(double es)
        {
            double t;
            double[] en = new double[5];
            en[0] = C00 - es * (C02 + es * (C04 + es * (C06 + es * C08)));
            en[1] = es * (C22 - es * (C04 + es * (C06 + es * C08)));
            en[2] = (t = es * es) * (C44 - es * (C46 + es * C48));
            en[3] = (t *= es) * (C66 - es * C68);
            en[4] = t * es * C88;
            return en;
        }

        public static double mlfn(double phi, double sphi, double cphi, double[] en)
        {
            cphi *= sphi;
            sphi *= sphi;
            return en[0] * phi - cphi * (en[1] + sphi * (en[2] + sphi * (en[3] + sphi * en[4])));
        }

        public static double inv_mlfn(double arg, double es, double[] en)
        {
            double s, t, phi, k = 1d / (1d - es);

            phi = arg;
            for (int i = MaxIter; i != 0; i--)
            {
                s = Math.Sin(phi);
                t = 1d - es * s * s;
                phi -= t = (mlfn(phi, s, Math.Cos(phi), en) - arg) * (t * Math.Sqrt(t)) * k;
                if (Math.Abs(t) < 1e-11)
                    return phi;
            }
            return phi;
        }

        private const double P00 = .33333333333333333333;
        private const double P01 = .17222222222222222222;
        private const double P02 = .10257936507936507936;
        private const double P10 = .06388888888888888888;
        private const double P11 = .06640211640211640211;
        private const double P20 = .01641501294219154443;

        public static double[] AuthSet(double es)
        {
            double[] apa = new double[3];
            apa[0] = es * P00;
            double t = es * es;
            apa[0] += t * P01;
            apa[1] = t * P10;
            t *= es;
            apa[0] += t * P02;
            apa[1] += t * P11;
            apa[2] = t * P20;
            return apa;
        }

        public static double AuthLat(double beta, double[] APA)
        {
            double t = beta + beta;
            return (beta + APA[0] * Math.Sin(t) + APA[1] * Math.Sin(t + t) + APA[2] * Math.Sin(t + t + t));
        }

        public static double Qsfn(double sinphi, double e, double oneEs)
        {
            if (e >= 1.0e-7)
            {
                double con = e * sinphi;
                return (oneEs * (sinphi / (1d - con * con) -
                   (.5 / e) * Math.Log((1d - con) / (1d + con))));
            }

            return (sinphi + sinphi);
        }

        /// <summary>
        /// C# translation of "Nice Numbers for Graph Labels" by Paul Heckbert from "Graphics Gems", Academic Press, 1990
        /// </summary>
        /// <param name="x"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public static double NiceNumber(double x, Boolean round)
        {
            int expv;				/* exponent of x */
            double f;				/* fractional part of x */
            double nf;				/* nice, rounded fraction */

            expv = (int)Math.Floor(Math.Log(x) / Math.Log(10));
            f = x / Math.Pow(10d, expv);		/* between 1 and 10 */
            if (round)
            {
                if (f < 1.5)
                    nf = 1d;
                else if (f < 3d)
                    nf = 2d;
                else if (f < 7d)
                    nf = 5d;
                else
                    nf = 10d;
            }
            else if (f <= 1d)
                nf = 1d;
            else if (f <= 2d)
                nf = 2d;
            else if (f <= 5d)
                nf = 5d;
            else
                nf = 10d;
            return nf * Math.Pow(10d, expv);
        }

        public static double ArcSecondsToRadians(double microSeconds)
        {
            const double arcSecondsToRadians = 4.848136811095359935899141023e-6;
            return microSeconds * arcSecondsToRadians;
        }

        public static double ArcMicroSecondsToRadians(double microSeconds)
        {
            const double arcSecondsToRadians = 4.848136811095359935899141023e-12;
            return microSeconds * arcSecondsToRadians;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        [Obsolete("Use NormalizeAngle")]
        public static double Adjlon(double lon)
        {
            if (Math.Abs(lon) <= Math.PI) return (lon);
            lon += Math.PI;  /* adjust to 0..2pi rad */
            lon -= 2 * Math.PI * Math.Floor(lon / (2 * Math.PI)); /* remove integral # of 'revolutions'*/
            lon -= Math.PI;  /* adjust back to -pi..pi rad */
            return (lon);
        }
    }
}
using System;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Datum
{
    /**
     *  Provides conversions between Geodetic coordinates 
     *  (latitude, longitude in radians and height in meters) 
     *  and Geocentric coordinates
     *  (X, Y, Z) in meters.
     *  <p>
     *  Provenance: Ported from GEOCENTRIC by the U.S. Army Topographic Engineering Center via PROJ.4
     *    
     * @author Martin Davis
     *
     */
    ///<summary>
    ///</summary>
    public class GeocentricConverter
    {
        /*
         * 
         * REFERENCES
         *    
         *    An Improved Algorithm for Geocentric to Geodetic Coordinate Conversion,
         *    Ralph Toms, February 1996  UCRL-JC-123138.
         *    
         *    Further information on GEOCENTRIC can be found in the Reuse Manual.
         *
         *    GEOCENTRIC originated from : U.S. Army Topographic Engineering Center
         *                                 Geospatial Information Division
         *                                 7701 Telegraph Road
         *                                 Alexandria, VA  22310-3864
         *
         * LICENSES
         *
         *    None apply to this component.
         *
         * RESTRICTIONS
         *
         *    GEOCENTRIC has no restrictions.
         */

        readonly double _a;
        readonly double _b;
        readonly double _a2;
        readonly double _b2;
        readonly double _e2;
        double _ep2;

        public GeocentricConverter(Ellipsoid ellipsoid):this(ellipsoid.A, ellipsoid.B)
        {
            
        }
        public GeocentricConverter(double a, double b)
        {
            _a = a;
            _b = b;
            _a2 = a * a;
            _b2 = b * b;
            _e2 = (_a2 - _b2) / _a2;
            _ep2 = (_a2 - _b2) / _b2;
        }

        /**
         * Converts geodetic coordinates
         * (latitude, longitude, and height) to geocentric coordinates (X, Y, Z),
         * according to the current ellipsoid parameters.
         *
         *    Latitude  : Geodetic latitude in radians                     (input)
         *    Longitude : Geodetic longitude in radians                    (input)
         *    Height    : Geodetic height, in meters                       (input)
         *    
         *    X         : Calculated Geocentric X coordinate, in meters    (output)
         *    Y         : Calculated Geocentric Y coordinate, in meters    (output)
         *    Z         : Calculated Geocentric Z coordinate, in meters    (output)
         *
         */
        public void ConvertGeodeticToGeocentric(Coordinate p)
        {
            double longitude = p.X;
            double latitude = p.Y;
            double height = CoordinateChecker.HasValidZOrdinate(p) ? p.Z : 0;   //Z value not always supplied
            double X;  // output
            double Y;
            double Z;

            double Rn;            /*  Earth radius at location  */
            double Sin_Lat;       /*  Math.sin(Latitude)  */
            double Sin2_Lat;      /*  Square of Math.sin(Latitude)  */
            double Cos_Lat;       /*  Math.cos(Latitude)  */

            /*
            ** Don't blow up if Latitude is just a little out of the value
            ** range as it may just be a rounding issue.  Also removed longitude
            ** test, it should be wrapped by Math.cos() and Math.sin().  NFW for PROJ.4, Sep/2001.
            */
            if (latitude < -ProjectionMath.PiHalf && latitude > -1.001 * ProjectionMath.PiHalf)
            {
                latitude = -ProjectionMath.PiHalf;
            }
            else if (latitude > ProjectionMath.PiHalf && latitude < 1.001 * ProjectionMath.PiHalf)
            {
                latitude = ProjectionMath.PiHalf;
            }
            else if ((latitude < -ProjectionMath.PiHalf) || (latitude > ProjectionMath.PiHalf))
            {
                /* Latitude out of range */
                //throw new IllegalStateException();
                throw new ArgumentException("Latitude is out of range: " + latitude, "p.Y");
            }

            if (longitude > ProjectionMath.Pi) longitude -= (2 * ProjectionMath.Pi);
            Sin_Lat = Math.Sin(latitude);
            Cos_Lat = Math.Cos(latitude);
            Sin2_Lat = Sin_Lat * Sin_Lat;
            Rn = _a / (Math.Sqrt(1.0e0 - _e2 * Sin2_Lat));
            X = (Rn + height) * Cos_Lat * Math.Cos(longitude);
            Y = (Rn + height) * Cos_Lat * Math.Sin(longitude);
            Z = ((Rn * (1 - _e2)) + height) * Sin_Lat;

            p.X = X;
            p.Y = Y;
            p.Z = Z;
        }

        public void ConvertGeocentricToGeodetic(Coordinate p)
        {
            ConvertGeocentricToGeodeticIter(p);
        }

        public void ConvertGeocentricToGeodeticIter(Coordinate p)
        {
            /* local defintions and variables */
            /* end-criterium of loop, accuracy of sin(Latitude) */
            const double genau = 1.0E-12;
            const double genau2 = (genau * genau);
            const int maxiter = 30;

            double P;        /* distance between semi-minor axis and location */
            double RR;       /* distance between center and location */
            double CT;       /* sin of geocentric latitude */
            double ST;       /* cos of geocentric latitude */
            double RX;
            double RK;
            double RN;       /* Earth radius at location */
            double CPHI0;    /* cos of start or old geodetic latitude in iterations */
            double SPHI0;    /* sin of start or old geodetic latitude in iterations */
            double CPHI;     /* cos of searched geodetic latitude */
            double SPHI;     /* sin of searched geodetic latitude */
            double SDPHI;    /* end-criterium: addition-theorem of sin(Latitude(iter)-Latitude(iter-1)) */
            Boolean At_Pole;     /* indicates location is in polar region */
            int iter;        /* # of continous iteration, max. 30 is always enough (s.a.) */

            double X = p.X;
            double Y = p.Y;
            double Z = CoordinateChecker.HasValidZOrdinate(p) ? p.Z : 0d;   //Z value not always supplied
            double longitude;
            double latitude;
            double height;

            At_Pole = false;
            P = Math.Sqrt(X * X + Y * Y);
            RR = Math.Sqrt(X * X + Y * Y + Z * Z);

            /*      special cases for latitude and longitude */
            if (P / this._a < genau)
            {

                /*  special case, if P=0. (X=0., Y=0.) */
                At_Pole = true;
                longitude = 0.0;

                /*  if (X,Y,Z)=(0.,0.,0.) then Height becomes semi-minor axis
                 *  of ellipsoid (=center of mass), Latitude becomes PI/2 */
                if (RR / this._a < genau)
                {
                    latitude = ProjectionMath.PiHalf;
                    height = -this._b;
                    return;
                }
            }
            else
            {
                /*  ellipsoidal (geodetic) longitude
                 *  interval: -PI < Longitude <= +PI */
                longitude = Math.Atan2(Y, X);
            }

            /* --------------------------------------------------------------
             * Following iterative algorithm was developped by
             * "Institut für Erdmessung", University of Hannover, July 1988.
             * Internet: www.ife.uni-hannover.de
             * Iterative computation of CPHI,SPHI and Height.
             * Iteration of CPHI and SPHI to 10**-12 radian resp.
             * 2*10**-7 arcsec.
             * --------------------------------------------------------------
             */
            CT = Z / RR;
            ST = P / RR;
            RX = 1.0 / Math.Sqrt(1.0 - this._e2 * (2.0 - this._e2) * ST * ST);
            CPHI0 = ST * (1.0 - this._e2) * RX;
            SPHI0 = CT * RX;
            iter = 0;

            /* loop to find sin(Latitude) resp. Latitude
             * until |sin(Latitude(iter)-Latitude(iter-1))| < genau */
            do
            {
                iter++;
                RN = this._a / Math.Sqrt(1.0 - this._e2 * SPHI0 * SPHI0);

                /*  ellipsoidal (geodetic) height */
                height = P * CPHI0 + Z * SPHI0 - RN * (1.0 - this._e2 * SPHI0 * SPHI0);

                RK = this._e2 * RN / (RN + height);
                RX = 1.0 / Math.Sqrt(1.0 - RK * (2.0 - RK) * ST * ST);
                CPHI = ST * (1.0 - RK) * RX;
                SPHI = CT * RX;
                SDPHI = SPHI * CPHI0 - CPHI * SPHI0;
                CPHI0 = CPHI;
                SPHI0 = SPHI;
            }
            while (SDPHI * SDPHI > genau2 && iter < maxiter);

            /*      ellipsoidal (geodetic) latitude */
            latitude = Math.Atan(SPHI / Math.Abs(CPHI));

            p.X = longitude;
            p.Y = latitude;
            p.Z = height;
        }

        //TODO: port non-iterative algorithm????
    }
}
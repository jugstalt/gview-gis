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

namespace Proj4Net.Datum
{
    /// <summary>
    /// A class representing a geographic reference ellipsoid 
    /// (or more correctly an oblate spheroid), 
    /// used to model the shape of the surface of the earth.
    /// <para/>
    /// An oblate spheroid is a geometric surface formed
    /// by the rotation of an ellipse about its minor axis.
    /// In geodesy this is used as a convenient approximation to the
    /// geoid, the true shape of the earth's surface.
    /// <para/>
    /// An ellipsoid is defined by the following parameters:
    /// <list type="Bullet">
    /// <item><i>a</i>, the equatorial radius or semi-major axis (<see cref="A"/>)</item></list>
    /// and one of:
    /// <list type="Bullet">
    /// <item><i>b</i>, the polar radius or semi-minor axis (<see cref="B"/></item>
    /// <item><i>f</i>, the reciprocal flattening
    /// (<i>f = (a - b) / a</i>)</item>
    /// </list>
    /// In order to be used as a model of the geoid,
    /// the exact positioning of an ellipsoid
    /// relative to the geoid surface needs to be specified.
    /// This is provided by means of a geodetic <see cref="Datum"/>.
    /// <para/>
    /// Notable ellipsoids in common use include 
    /// <see cref="CLARKE_1866"/>, <see cref="GRS80"/>, and <see cref="WGS84"/>.
    /// </summary>
    /// <seealso cref="Datum"/>
    public class Ellipsoid : 
#if !SILVERLIGHT
        ICloneable, 
#endif
        IEquatable<Ellipsoid>
    {

        private String _name;

        private String _shortName;

        private double _equatorRadius = 1.0;

        private double _poleRadius = 1.0;

        private double _eccentricity = 1.0;

        private double _eccentricity2 = 1.0;

        // From: USGS PROJ package.
        // ReSharper disable InconsistentNaming
        public static readonly Ellipsoid INTERNATIONAL = new Ellipsoid("intl",
                                                                       6378388.0, 0.0, 297.0,
                                                                       "International 1909 (Hayford)");

        public static readonly Ellipsoid BESSEL = new Ellipsoid("bessel",
                                                                6377397.155, 0.0, 299.1528128, "Bessel 1841");

        public static readonly Ellipsoid CLARKE_1866 = new Ellipsoid("clrk66",
                                                                     6378206.4, 6356583.8, 0.0, "Clarke 1866");

        public static readonly Ellipsoid CLARKE_1880 = new Ellipsoid("clrk80",
                                                                     6378249.145, 0.0, 293.4663, "Clarke 1880 mod.");

        public static readonly Ellipsoid AIRY = new Ellipsoid("airy", 6377563.396,
                                                              6356256.910, 0.0, "Airy 1830");

        public static readonly Ellipsoid WGS60 = new Ellipsoid("WGS60", 6378165.0, 0.0,
                                                               298.3, "WGS 60");

        public static readonly Ellipsoid WGS66 = new Ellipsoid("WGS66", 6378145.0, 0.0,
                                                               298.25, "WGS 66");

        public static readonly Ellipsoid WGS72 = new Ellipsoid("WGS72", 6378135.0, 0.0,
                                                               298.26, "WGS 72");

        public static readonly Ellipsoid WGS84 = new Ellipsoid("WGS84", 6378137.0, 0.0,
                                                               298.257223563, "WGS 84");

        public static readonly Ellipsoid KRASSOVSKY = new Ellipsoid("krass", 6378245.0,
                                                                    0.0, 298.3, "Krassovsky, 1942");

        public static readonly Ellipsoid EVEREST = new Ellipsoid("evrst30", 6377276.345,
                                                                 0.0, 300.8017, "Everest 1830");

        public static readonly Ellipsoid INTERNATIONAL_1967 = new Ellipsoid("new_intl",
                                                                            6378157.5, 6356772.2, 0.0,
                                                                            "New International 1967");

        public static readonly Ellipsoid GRS80 = new Ellipsoid("GRS80", 6378137.0, 0.0,
                                                               298.257222101, "GRS 1980 (IUGG, 1980)");

        public static readonly Ellipsoid AUSTRALIAN = new Ellipsoid("australian",
                                                                    6378160.0, 6356774.7, 298.25, "Australian");

        public static readonly Ellipsoid MERIT = new Ellipsoid("MERIT", 6378137.0, 0.0,
                                                               298.257, "MERIT 1983");

        public static readonly Ellipsoid SGS85 = new Ellipsoid("SGS85", 6378136.0, 0.0,
                                                               298.257, "Soviet Geodetic System 85");

        public static readonly Ellipsoid IAU76 = new Ellipsoid("IAU76", 6378140.0, 0.0,
                                                               298.257, "IAU 1976");

        public static readonly Ellipsoid APL4_9 = new Ellipsoid("APL4.9", 6378137.0,
                                                                0.0, 298.25, "Appl. Physics. 1965");

        public static readonly Ellipsoid NWL9D = new Ellipsoid("NWL9D", 6378145.0, 0.0,
                                                               298.25, "Naval Weapons Lab., 1965");

        public static readonly Ellipsoid MOD_AIRY = new Ellipsoid("mod_airy",
                                                                  6377340.189, 6356034.446, 0.0, "Modified Airy");

        public static readonly Ellipsoid ANDRAE = new Ellipsoid("andrae", 6377104.43,
                                                                0.0, 300.0, "Andrae 1876 (Den., Iclnd.)");

        public static readonly Ellipsoid AUST_SA = new Ellipsoid("aust_SA", 6378160.0,
                                                                 0.0, 298.25, "Australian Natl & S. Amer. 1969");

        public static readonly Ellipsoid GRS67 = new Ellipsoid("GRS67", 6378160.0, 0.0,
                                                               298.2471674270, "GRS 67 (IUGG 1967)");

        public static readonly Ellipsoid BESS_NAM = new Ellipsoid("bess_nam",
                                                                  6377483.865, 0.0, 299.1528128, "Bessel 1841 (Namibia)");

        public static readonly Ellipsoid CPM = new Ellipsoid("CPM", 6375738.7, 0.0,
                                                             334.29, "Comm. des Poids et Mesures 1799");

        public static readonly Ellipsoid DELMBR = new Ellipsoid("delmbr", 6376428.0,
                                                                0.0, 311.5, "Delambre 1810 (Belgium)");

        public static readonly Ellipsoid ENGELIS = new Ellipsoid("engelis", 6378136.05,
                                                                 0.0, 298.2566, "Engelis 1985");

        public static readonly Ellipsoid EVRST48 = new Ellipsoid("evrst48", 6377304.063,
                                                                 0.0, 300.8017, "Everest 1948");

        public static readonly Ellipsoid EVRST56 = new Ellipsoid("evrst56", 6377301.243,
                                                                 0.0, 300.8017, "Everest 1956");

        public static readonly Ellipsoid EVRTS69 = new Ellipsoid("evrst69", 6377295.664,
                                                                 0.0, 300.8017, "Everest 1969");

        public static readonly Ellipsoid EVRTSTSS = new Ellipsoid("evrstSS",
                                                                  6377298.556, 0.0, 300.8017,
                                                                  "Everest (Sabah & Sarawak)");

        public static readonly Ellipsoid FRSCH60 = new Ellipsoid("fschr60", 6378166.0,
                                                                 0.0, 298.3, "Fischer (Mercury Datum) 1960");

        public static readonly Ellipsoid FSRCH60M = new Ellipsoid("fschr60m", 6378155.0,
                                                                  0.0, 298.3, "Modified Fischer 1960");

        public static readonly Ellipsoid FSCHR68 = new Ellipsoid("fschr68", 6378150.0,
                                                                 0.0, 298.3, "Fischer 1968");

        public static readonly Ellipsoid HELMERT = new Ellipsoid("helmert", 6378200.0,
                                                                 0.0, 298.3, "Helmert 1906");

        public static readonly Ellipsoid HOUGH = new Ellipsoid("hough", 6378270.0, 0.0,
                                                               297.0, "Hough");

        public static readonly Ellipsoid INTL = new Ellipsoid("intl", 6378388.0, 0.0,
                                                              297.0, "International 1909 (Hayford)");

        public static readonly Ellipsoid KAULA = new Ellipsoid("kaula", 6378163.0, 0.0,
                                                               298.24, "Kaula 1961");

        public static readonly Ellipsoid LERCH = new Ellipsoid("lerch", 6378139.0, 0.0,
                                                               298.257, "Lerch 1979");

        public static readonly Ellipsoid MPRTS = new Ellipsoid("mprts", 6397300.0, 0.0,
                                                               191.0, "Maupertius 1738");

        public static readonly Ellipsoid PLESSIS = new Ellipsoid("plessis", 6376523.0,
                                                                 6355863.0, 0.0, "Plessis 1817 France)");

        public static readonly Ellipsoid SEASIA = new Ellipsoid("SEasia", 6378155.0,
                                                                6356773.3205, 0.0, "Southeast Asia");

        public static readonly Ellipsoid WALBECK = new Ellipsoid("walbeck", 6376896.0,
                                                                 6355834.8467, 0.0, "Walbeck");

        public static readonly Ellipsoid NAD27 = new Ellipsoid("NAD27", 6378249.145,
                                                               0.0, 293.4663, "NAD27: Clarke 1880 mod.");

        public static readonly Ellipsoid NAD83 = new Ellipsoid("NAD83", 6378137.0, 0.0,
                                                               298.257222101, "NAD83: GRS 1980 (IUGG, 1980)");

        public static readonly Ellipsoid SPHERE = new Ellipsoid("sphere", 6371008.7714,
                                                                6371008.7714, 0.0, "Sphere");

        // ReSharper restore InconsistentNaming


        public static readonly Ellipsoid[] Ellipsoids =
            {
                BESSEL,
                CLARKE_1866, CLARKE_1880,
                AIRY,
                WGS60, WGS66, WGS72, WGS84,
                KRASSOVSKY,
                EVEREST,
                INTERNATIONAL_1967,
                GRS80,
                AUSTRALIAN,
                MERIT,
                SGS85,
                IAU76,
                APL4_9,
                NWL9D,
                MOD_AIRY,
                ANDRAE,
                AUST_SA,
                GRS67,
                BESS_NAM,
                CPM,
                DELMBR,
                ENGELIS,
                EVRST48, EVRST56, EVRTS69,
                EVRTSTSS,
                FRSCH60,
                FSRCH60M,
                FSCHR68,
                HELMERT,
                HOUGH,
                INTL,
                KAULA,
                LERCH,
                MPRTS,
                PLESSIS,
                SEASIA,
                WALBECK,
                NAD27, NAD83,
                SPHERE
            };


        /// <summary>
        /// Creates a new Ellipsoid.
        /// </summary>
        public Ellipsoid()
        {
        }

        /**
         * 
         * 
         */
        /// <summary>
        /// Creates a new Ellipsoid.
        /// <para>
        /// One of of poleRadius or reciprocalFlattening must
        /// be specified, the other must be zero</para>
        /// </summary>
        public Ellipsoid(String shortName, double equatorRadius, double poleRadius, double reciprocalFlattening,
                         String name)
        {
            _shortName = shortName;
            _name = name;
            _equatorRadius = equatorRadius;
            _poleRadius = poleRadius;

            if (poleRadius == 0.0 && reciprocalFlattening == 0.0)
                throw new ArgumentException("One of poleRadius or reciprocalFlattening must be specified");
            // don't check for only one of poleRadius or reciprocalFlattening to be specified,
            // since some defs actually supply two

            // reciprocalFlattening takes precedence over poleRadius
            if (reciprocalFlattening != 0)
            {
                double flattening = 1.0/reciprocalFlattening;
                double f = flattening;
                _eccentricity2 = 2*f - f*f;
                _poleRadius = equatorRadius*Math.Sqrt(1.0 - _eccentricity2);
            }
            else
            {
                _eccentricity2 = 1.0 - (poleRadius*poleRadius)/(equatorRadius*equatorRadius);
            }
            _eccentricity = Math.Sqrt(_eccentricity2);
        }

        public Ellipsoid(String shortName, double equatorRadius, double eccentricity2, String name)
        {
            _shortName = shortName;
            _name = name;
            _equatorRadius = equatorRadius;
            EccentricitySquared = eccentricity2;
        }

        public Object Clone()
        {
            throw new NotSupportedException();
            //try {
            //    Ellipsoid e = (Ellipsoid)super.clone();
            //    return e;
            //}
            //catch (CloneNotSupportedException e) {
            //    throw new InternalError();
            //}
        }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public String ShortName
        {
            get { return _shortName; }
            set { _shortName = value; }
        }

        public Double EquatorRadius
        {
            get { return _equatorRadius; }
            set { _equatorRadius = value; }
        }

        public Double A
        {
            get { return _equatorRadius; }
        }

        public Double B
        {
            get { return _poleRadius; }
        }

        public Double Eccentricity
        {
            get { return _eccentricity; }
            //set
            //{
            //    _eccentricity2 = value;
            //    _poleRadius = _equatorRadius * Math.Sqrt(1.0 - value);
            //    _eccentricity = Math.Sqrt(value);
            //}
        }

        public Double EccentricitySquared
        {
            get { return _eccentricity2; }
            set
            {
                _eccentricity2 = value;
                _poleRadius = _equatorRadius*Math.Sqrt(1.0 - value);
                _eccentricity = Math.Sqrt(value);
            }
        }

        public bool Equals(Ellipsoid other)
        {
            return _equatorRadius == other._equatorRadius
                   && _eccentricity2 == other._eccentricity2;
        }

        public bool Equals(Ellipsoid e, double e2Tolerance)
        {
            if (_equatorRadius != e._equatorRadius) 
                return false;
            
            if (Math.Abs(_eccentricity2
                - e._eccentricity2) > e2Tolerance) return false;
            
            return true;
        }



        public override String ToString()
        {
            return _name;
        }

    }
}
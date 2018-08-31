using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    internal class AbstractInformation
    {
        private string _remarks;
        private string _authorityCode;
        private string _authority;
        private string _alias;
        private string _abbreviation;

        protected string _name;

        internal AbstractInformation(string remarks, string authority, string authorityCode,
                                        string name, string alias, string abbreviation)
        {
            _remarks = remarks;
            _authorityCode = authorityCode;
            _authority = authority;
            _name = name;
            _alias = alias;
            _abbreviation = abbreviation;
        }

        public string Remarks
        {
            get
            {
                return _remarks;
            }
        }

        public string AuthorityCode
        {
            get
            {
                return _authorityCode;
            }
        }

        //public string WKT
        //{
        //    get
        //    {
        //        return CoordinateSystemWktWriter.Write(this);
        //    }
        //}

        public string Authority
        {
            get
            {
                return _authority;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string Alias
        {
            get
            {
                return _alias;
            }
        }

        //public string XML
        //{
        //    get
        //    {
        //        return CoordinateSystemXmlWriter.Write(this);
        //    }
        //}

        public string Abbreviation
        {
            get
            {
                return _abbreviation;
            }
        }
    }

    internal class Unit : AbstractInformation
    {
        internal Unit(string remarks, string authority, string authorityCode,
                                        string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
        }
    }

    internal class LinearUnit : Unit
    {
        private double _metersPerUnit = 0.0;

        public static LinearUnit Meters
        {
            get
            {
                return new LinearUnit(1.0, "Also known as International metre.", "EPSG", "9001", "metre", String.Empty, String.Empty);
            }
        }

        public LinearUnit(double metersPerUnit)
            : base(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
            _metersPerUnit = metersPerUnit;
        }
        internal LinearUnit(double metersPerUnit, string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            _metersPerUnit = metersPerUnit;
        }
        public double MetersPerUnit
        {
            get
            {
                return _metersPerUnit;
            }
        }
    }

    internal class AngularUnit : Unit
    {
        double _radiansPerUnit;

        internal AngularUnit(double radiansPerUnit) : this(radiansPerUnit, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) { }

        internal AngularUnit(double radiansPerUnit, string remarks, string authority, string authorityCode, string name,
                                string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            _radiansPerUnit = radiansPerUnit;
        }

        public double RadiansPerUnit
        {
            get
            {
                return _radiansPerUnit;
            }
        }
    }

    internal class PrimeMeridian : AbstractInformation
    {

        AngularUnit _angularUnit;
        double _longitude;

        internal PrimeMeridian(string name, AngularUnit angularUnit, double longitude, string remarks, string authority, string authorityCode, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (angularUnit == null)
            {
                throw new ArgumentNullException("angularUnit");
            }
            _name = name;
            _angularUnit = angularUnit;
            _longitude = longitude;
        }

        internal PrimeMeridian(string name, AngularUnit angularUnit, double longitude)
            : this(name, angularUnit, longitude, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }

        #region Static methods
        public static PrimeMeridian Greenwich
        {
            get
            {
                PrimeMeridian greenwich = new PrimeMeridian("Greenwich(default)", new AngularUnit(1), 0.0);
                return greenwich;
            }
        }
        #endregion

        public AngularUnit AngularUnit
        {
            get
            {
                return _angularUnit;
            }
        }

        public double Longitude
        {
            get
            {
                return _longitude * _angularUnit.RadiansPerUnit;
            }
        }
    }

    internal class Ellipsoid : AbstractInformation
    {
        bool _isIvfDefinitive = false;
        double _semiMajorAxis = 0.0;
        double _semiMinorAxis = 0.0;
        double _inverseFlattening = -1;
        LinearUnit _linearUnit;

        #region Constructors

        internal Ellipsoid(double semiMajorAxis, double semiMinorAxis, double inverseFlattening, bool isIvfDefinitive, LinearUnit linearUnit)
            :
            this(semiMajorAxis, semiMinorAxis, inverseFlattening, isIvfDefinitive, linearUnit,
                String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) { }

        internal Ellipsoid(double semiMajorAxis, double semiMinorAxis, double inverseFlattening, bool isIvfDefinitive, LinearUnit linearUnit,
            string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            :
                base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (linearUnit == null)
                throw new ArgumentNullException("linearUnit");

            _name = name;
            _linearUnit = linearUnit;
            _semiMajorAxis = semiMajorAxis;
            _isIvfDefinitive = isIvfDefinitive;

            if (_isIvfDefinitive)
            {
                _inverseFlattening = inverseFlattening;
                if (_inverseFlattening == 0.0)
                    _semiMinorAxis = semiMajorAxis;
                else
                    _semiMinorAxis = (1.0 - (1.0 / _inverseFlattening)) * semiMajorAxis;
            }
            else
            {
                _semiMinorAxis = semiMinorAxis;
                if (_semiMajorAxis - _semiMinorAxis != 0.0)
                {
                    _inverseFlattening = _semiMajorAxis / (_semiMajorAxis - _semiMinorAxis);
                }
                else
                {
                    _inverseFlattening = 0.0;
                }
            }
        }

        #endregion

        #region Static

        public static Ellipsoid WGS84Test
        {
            get
            {
                LinearUnit meters = new LinearUnit(1.0);
                Ellipsoid ellipsoid = new Ellipsoid(6378137.0, -1.0, 298.257223563, true, meters,
                    String.Empty, String.Empty, String.Empty, "WGS84(default)", String.Empty, String.Empty);
                return ellipsoid;
            }
        }

        #endregion

        #region Implementation of IEllipsoid

        public bool IsIvfDefinitive()
        {
            return _isIvfDefinitive;
        }

        public double SemiMajorAxis
        {
            get
            {
                return _semiMajorAxis * _linearUnit.MetersPerUnit;
            }
        }

        public double InverseFlattening
        {
            get
            {
                return _inverseFlattening;
            }
        }

        public LinearUnit AxisUnit
        {
            get
            {
                return _linearUnit;
            }
        }

        public double SemiMinorAxis
        {
            get
            {
                return _semiMinorAxis * _linearUnit.MetersPerUnit;
            }
        }

        #endregion

    }

    internal struct WGS84ConversionInfo
    {
        /// <summary>
        /// Use this struct in Proj4 Parameter String
        /// </summary>
        public bool IsInUse;
        /// <summary>
        /// Human readable text describing intended region of transformation.
        /// </summary>
        public string AreaOfUse;
        /// <summary>
        /// Bursa Wolf shift in meters.
        /// </summary>
        public double Dx;
        /// <summary>
        /// Bursa Wolf shift in meters.
        /// </summary>
        public double Dy;
        /// <summary>
        /// Bursa Wolf shift in meters.
        /// </summary>
        public double Dz;
        /// <summary>
        /// Bursa Wolf rotation in arc seconds.
        /// </summary>
        public double Ex;
        /// <summary>
        /// Bursa Wolf rotation in arc seconds.
        /// </summary>
        public double Ey;
        /// <summary>
        /// Bursa Wolf rotation in arc seconds.
        /// </summary>
        public double Ez;
        /// <summary>
        /// Bursa Wolf scaling in parts per million.
        /// </summary>
        public double Ppm;
    }

    internal enum DatumType
    {
        /// <summary>
        ///  These datums, such as ED50, NAD27 and NAD83, have been designed to
        ///  support horizontal positions on the ellipsoid as opposed to positions
        ///  in 3-D space.  These datums were designed mainly to support a
        ///  horizontal component of a position in a domain of limited extent, such
        ///  as a country, a region or a continent.
        /// </summary>
        IHD_Classic = 1001,

        /// <summary>
        /// A geocentric datum is a "satellite age" modern geodetic datum mainly of
        /// global extent, such as WGS84 (used in GPS), PZ90 (used in GLONASS) and
        /// ITRF.  These datums were designed to support both a horizontal
        /// component of position and a vertical component of position (through
        /// ellipsoidal heights).  The regional realizations of ITRF, such as
        /// ETRF, are also included in this category.
        /// </summary>
        IHD_Geocentric = 1002,

        /// <summary>
        /// Highest possible value for horizontal datum types.
        /// </summary>
        IHD_Max = 1999,

        /// <summary>
        /// Lowest possible value for horizontal datum types.
        /// </summary>
        IHD_Min = 1000,

        /// <summary>
        /// Unspecified horizontal datum type.
        /// Horizontal datums with this type should never supply
        /// a conversion to WGS84 using Bursa Wolf parameters.
        /// </summary>
        IHD_Other = 1000,

        /// <summary>
        /// Highest possible value for local datum types.
        /// </summary>
        ILD_Max = 32767,

        /// <summary>
        /// Lowest possible value for local datum types.
        /// </summary>
        ILD_Min = 10000,

        /// <summary>
        /// The vertical datum of altitudes or heights in the atmosphere.  These
        /// are approximations of orthometric heights obtained with the help of
        /// a barometer or a barometric altimeter.  These values are usually
        /// expressed in one of the following units: meters, feet, millibars
        /// (used to measure pressure levels),  or theta value (units used to
        /// measure geopotential height).
        /// </summary>
        IVD_AltitudeBarometric = 2003,

        /// <summary>
        ///  This attribute is used to support the set of datums generated
        ///  for hydrographic engineering projects where depth measurements below
        ///  sea level are needed.  It is often called a hydrographic or a marine
        ///  datum.  Depths are measured in the direction perpendicular
        ///  (approximately) to the actual equipotential surfaces of the earth's
        ///  gravity field, using such procedures as echo-sounding.
        /// </summary>
        IVD_Depth = 2006,

        /// <summary>
        /// A vertical datum for ellipsoidal heights that are measured along the
        /// normal to the ellipsoid used in the definition of horizontal datum.
        /// </summary>
        IVD_Ellipsoidal = 2002,

        /// <summary>
        ///  A vertical datum of geoid model derived heights, also called
        ///  GPS-derived heights. These heights are approximations of
        ///  orthometric heights (H), constructed from the ellipsoidal heights
        /// (h) by the use of the given geoid undulation model (N) through the
        /// equation: H=h-N.
        /// </summary>
        IVD_GeoidModelDerived = 2005,

        /// <summary>
        /// Highest possible value for vertical datum types.
        /// </summary>
        IVD_Max = 2999,

        /// <summary>
        /// Lowest possible value for vertical datum types.
        /// </summary>
        IVD_Min = 2000,

        /// <summary>
        /// A normal height system.
        /// </summary>
        IVD_Normal = 2004,

        /// <summary>
        ///  A vertical datum for orthometric heights that are measured along the
        /// plumb line.
        /// </summary>
        IVD_Orthometric = 2001,

        /// <summary>
        /// Unspecified vertical datum type.
        /// </summary>
        IVD_Other = 2000,
    }

    internal class Datum : AbstractInformation
    {
        DatumType _datumType;

        internal Datum(string name, DatumType datumType)
            :
            this(datumType, String.Empty, String.Empty, String.Empty, name, String.Empty, String.Empty) { }

        internal Datum(DatumType datumType)
            :
            this(datumType, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) { }

        public Datum(DatumType datumType, string remarks, string authorityCode, string authority, string name, string alias, string abbreviation)
            :
                        base(remarks, authorityCode, authority, name, alias, abbreviation)
        {
            _datumType = datumType;
        }

        public DatumType DatumType
        {
            get
            {
                return _datumType;
            }
        }
    }

    internal class HorizontalDatum : Datum
    {
        Ellipsoid _ellipsoid;
        WGS84ConversionInfo _wgs84ConversionInfo;
        #region Constructors

        internal HorizontalDatum(string name, DatumType horizontalDatumType, Ellipsoid ellipsoid, WGS84ConversionInfo toWGS84)
            : this(name, horizontalDatumType, ellipsoid, toWGS84, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }

        public HorizontalDatum(string name, DatumType horizontalDatumType, Ellipsoid ellipsoid, WGS84ConversionInfo toWGS84,
            string remarks, string authority, string authorityCode, string alias, string abbreviation)
            : base(horizontalDatumType, remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (ellipsoid == null)
                throw new ArgumentNullException("ellipsoid");
            _ellipsoid = ellipsoid;
            _wgs84ConversionInfo = toWGS84;
        }
        #endregion

        #region Static methods
        public static HorizontalDatum WGS84
        {
            get
            {
                Ellipsoid ellipsoid = Ellipsoid.WGS84Test;
                WGS84ConversionInfo conversionInfo = new WGS84ConversionInfo();
                HorizontalDatum horizontaldatum = new HorizontalDatum("WGS84", DatumType.IHD_Geocentric, ellipsoid, conversionInfo);
                return horizontaldatum;
            }
        }
        #endregion
        
        public WGS84ConversionInfo WGS84Parameters
        {
            get
            {
                WGS84ConversionInfo wgs = _wgs84ConversionInfo;
                return wgs;
            }
        }

        public Ellipsoid Ellipsoid
        {
            get
            {
                return _ellipsoid;
            }
        }
    }

    internal class VerticalDatum : Datum
    {
        internal VerticalDatum(string name, DatumType verticalDatumType)
            : this(verticalDatumType, String.Empty, String.Empty, String.Empty, name, String.Empty, String.Empty)
        {
        }

        public VerticalDatum(DatumType datumType,
            string remarks,
            string authorityCode,
            string authority,
            string name,
            string alias,
            string abbreviation)
            : base(datumType, remarks, authority, authorityCode, name, alias, abbreviation)
        {
        }

        #region Static

        public static VerticalDatum Ellipsoidal
        {
            get
            {
                return new VerticalDatum("Ellipsoidal", DatumType.IVD_Ellipsoidal);
            }
        }
        #endregion
    }

    internal class LocalDatum : Datum
    {
        internal LocalDatum(string name, DatumType datumType)
            : base(name, datumType)
        {
        }
    }

    internal enum AxisOrientation
    {
        Down = 6,
        East = 3,
        North = 1,
        Other = 0,
        South = 2,
        Up = 5,
        West = 4,
    }

    internal class AxisInfo
    {
        private string _name = String.Empty;

        private AxisOrientation _orientation;

        public static AxisInfo X
        {
            get
            {
                return new AxisInfo("x", AxisOrientation.East);
            }
        }

        public static AxisInfo Y
        {
            get
            {
                return new AxisInfo("y", AxisOrientation.North);
            }
        }

        public static AxisInfo Longitude
        {
            get
            {
                return new AxisInfo("Longitude", AxisOrientation.East);
            }
        }

        public static AxisInfo Latitude
        {
            get
            {
                return new AxisInfo("Latitude", AxisOrientation.North);
            }
        }

        public static AxisInfo Altitude
        {
            get
            {
                return new AxisInfo("Altitude", AxisOrientation.Up);
            }
        }

        public AxisInfo(string name, AxisOrientation orientation)
        {
            _name = name;
            _orientation = orientation;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public AxisOrientation Orientation
        {
            get
            {
                return _orientation;
            }
        }
    }

    internal abstract class CoordinateSystem : AbstractInformation
    {
        Unit _unit = null;

        internal CoordinateSystem(string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation) { }

        public virtual AxisInfo GetAxis(int Dimension)
        {
            throw new InvalidOperationException("CoordinateSystem does not have axis information.");
        }

        public virtual Unit GetUnits(int dimension)
        {
            if (dimension >= 0 && dimension < this.Dimension)
                return _unit;
            throw new ArgumentOutOfRangeException(String.Format("Dimension must be between 0 and {0}", this.Dimension));
        }

        public virtual int Dimension
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public virtual Envelope DefaultEnvelope
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class GeographicCoordinateSystem : CoordinateSystem
    {
        PrimeMeridian _primeMeridian;
        IEnvelope _defaultEnvelope;
        HorizontalDatum _horizontalDatum;
        AngularUnit _angularUnit;
        AxisInfo[] _axisInfo;


        internal GeographicCoordinateSystem(string name,
            AngularUnit angularUnit,
            HorizontalDatum horizontalDatum,
            PrimeMeridian primeMeridian,
            AxisInfo axis0,
            AxisInfo axis1)
            :
        this(angularUnit, horizontalDatum, primeMeridian, axis0, axis1, String.Empty, String.Empty, String.Empty, name, String.Empty, String.Empty)
        {

        }

        internal GeographicCoordinateSystem(
            AngularUnit angularUnit,
            HorizontalDatum horizontalDatum,
            PrimeMeridian primeMeridian,
            AxisInfo axis0,
            AxisInfo axis1,
            string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            _angularUnit = angularUnit;
            _horizontalDatum = horizontalDatum;
            _primeMeridian = primeMeridian;
            _axisInfo = new AxisInfo[] { axis0, axis1 };

            // define the envelope.
            _defaultEnvelope = new Envelope(-180,-90,180,90);
        }

        #region Implementation of IGeographicCoordinateSystem
        /// <summary>
        /// Gets axis information for the specified dimension.
        /// </summary>
        /// <param name="Dimension">The dimension to get the axis information for.</param>
        /// <returns>IAxisInfo containing the axis information.</returns>
        public override AxisInfo GetAxis(int Dimension)
        {
            return _axisInfo[Dimension];
        }

        /// <summary>
        /// Gets the unit information for the specified dimension.
        /// </summary>
        /// <param name="dimension">The dimentsion to get the units information for.</param>
        /// <returns>IUnit containing infomation about the units.</returns>
        public override Unit GetUnits(int dimension)
        {
            if (dimension >= 0 && dimension < this.Dimension)
            {
                return _angularUnit;
            }
            throw new ArgumentOutOfRangeException(String.Format("Dimension must be between 0 and {0}", this.Dimension));
        }
        /// <summary>
        /// Gets the number of dimensions for a geographic coordinate system (2).
        /// </summary>
        public override int Dimension
        {
            get
            {
                return 2;
            }
        }


        /// <summary>
        /// Gets the WGS 84 conversion information.
        /// </summary>
        /// <param name="index">????</param>
        /// <returns>IWGS84ConversionInfo containing the WGS 84 conversion information.</returns>
        public WGS84ConversionInfo GetWGS84ConversionInfo(int index)
        {
            return new WGS84ConversionInfo();
        }

        /// <summary>
        /// Gets the angular units used in the coordinate system.
        /// </summary>
        public AngularUnit AngularUnit
        {
            get
            {
                return _angularUnit;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public int NumConversionToWGS84
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets the horizontal datum for the coordinate system.
        /// </summary>
        public HorizontalDatum HorizontalDatum
        {
            get
            {
                return _horizontalDatum;
            }
        }


        /// <summary>
        /// Gets the prime meridian for this coordinate system.
        /// </summary>
        public PrimeMeridian PrimeMeridian
        {
            get
            {
                return _primeMeridian;
            }
        }
        #endregion



    }

    internal struct ProjectionParameter
    {
        public ProjectionParameter(string name, double v)
        {
            Name = name;
            Value = v;
        }

        public string Name;

        public double Value;
    }

    internal class VerticalCoordinateSystem : CoordinateSystem
    {
        VerticalDatum _verticaldatum;
        AxisInfo[] _axisinfo;
        LinearUnit _units;

        internal VerticalCoordinateSystem(string name, VerticalDatum verticaldatum, AxisInfo axisinfo, LinearUnit units)
            : base(name, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
            if (verticaldatum == null)
            {
                throw new ArgumentNullException("verticaldatum");
            }
            if (units == null)
            {
                throw new ArgumentNullException("units");
            }

            _name = name;
            _verticaldatum = verticaldatum;
            _axisinfo = new AxisInfo[1] { axisinfo };
            _units = units;
        }
        internal VerticalCoordinateSystem(
            string name,
            VerticalDatum verticaldatum,
            string remarks, string authority, string authorityCode, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (verticaldatum == null)
            {
                throw new ArgumentNullException("verticaldatum");
            }
            _verticaldatum = verticaldatum;
            _units = LinearUnit.Meters;
            _axisinfo = new AxisInfo[1] { AxisInfo.Altitude };
        }
        public VerticalCoordinateSystem(string name,
            VerticalDatum verticaldatum,
            AxisInfo axisinfo,
            LinearUnit linearUnit,
            string remarks, string authority, string authorityCode, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (verticaldatum == null)
            {
                throw new ArgumentNullException("verticaldatum");
            }

            _verticaldatum = verticaldatum;
            _axisinfo = new AxisInfo[1] { axisinfo };
            _units = linearUnit;
        }

        #region Static
        public static VerticalCoordinateSystem Ellipsoidal
        {
            get
            {
                VerticalDatum datum = VerticalDatum.Ellipsoidal;
                return new VerticalCoordinateSystem("Ellipsoidal", datum, AxisInfo.Altitude, LinearUnit.Meters);
            }
        }

        #endregion

        #region Implementation of IVerticalCoordinateSystem

        public override int Dimension
        {
            get
            {
                return 1;
            }
        }

        public VerticalDatum VerticalDatum
        {
            get
            {
                return _verticaldatum;
            }
        }

        public LinearUnit VerticalUnit
        {
            get
            {
                return _units;
            }
        }

        public override AxisInfo GetAxis(int dimension)
        {
            return _axisinfo[dimension];
        }
        #endregion
    }

    internal class CompoundCoordinateSystem : CoordinateSystem
    {
        AxisInfo[] _axisInfo;

        CoordinateSystem _headCRS;

        CoordinateSystem _tailCRS;

        internal CompoundCoordinateSystem(CoordinateSystem headCRS, CoordinateSystem tailCRS, string remarks, string authority, string authorityCode,
                    string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (headCRS == null)
                throw new ArgumentNullException("headCRS");

            if (tailCRS == null)
                throw new ArgumentNullException("tailCRS");

            _headCRS = headCRS;
            _tailCRS = tailCRS;

            _axisInfo = new AxisInfo[this.Dimension];

            // copy axis information
            for (int i = 0; i < headCRS.Dimension; i++)
                _axisInfo[i] = _headCRS.GetAxis(i);

            int offset = headCRS.Dimension;
            for (int i = 0; i < tailCRS.Dimension; i++)
                _axisInfo[i + offset] = _tailCRS.GetAxis(i);
        }

        public override AxisInfo GetAxis(int dimension)
        {
            return _axisInfo[dimension];
        }

        public override int Dimension
        {
            get
            {
                return _headCRS.Dimension + _tailCRS.Dimension;
            }
        }

        public CoordinateSystem TailCS
        {
            get
            {
                return _tailCRS;
            }
        }

        public CoordinateSystem HeadCS
        {
            get
            {
                return _headCRS;
            }
        }
    }

    internal class HorizontalCoordinateSystem : CoordinateSystem
    {
        HorizontalDatum _horizontalDatum;
        AxisInfo[] _axisInfoArray;

        public HorizontalCoordinateSystem(HorizontalDatum horizontalDatum, AxisInfo[] axisInfoArray)
            : this(horizontalDatum, axisInfoArray, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }

        public HorizontalCoordinateSystem(HorizontalDatum horizontalDatum, AxisInfo[] axisInfoArray,
            string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {
            if (horizontalDatum == null)
            {
                throw new ArgumentNullException("horizontalDatum");
            }
            if (axisInfoArray == null)
            {
                throw new ArgumentNullException("axisInfoArray");
            }
            _horizontalDatum = horizontalDatum;
            _axisInfoArray = axisInfoArray;
        }


        public HorizontalDatum HorizontalDatum
        {
            get
            {
                return _horizontalDatum;
            }
        }
    }

    internal class LocalCoordinateSystem : CoordinateSystem
    {
        internal LocalCoordinateSystem()
            : base(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
            throw new NotImplementedException();
        }

        public LocalDatum LocalDatum
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class FittedCoordinateSystem : CoordinateSystem
    {
        internal FittedCoordinateSystem()
            : base(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
            throw new NotImplementedException();
        }

        public string GetToBase()
        {
            throw new NotImplementedException();
        }

        public string ToBase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CoordinateSystem BaseCoordinateSystem
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class GeocentricCoordinateSystem : CoordinateSystem
    {
        internal GeocentricCoordinateSystem()
            : base(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
            throw new NotImplementedException();
        }

        public LinearUnit LinearUnit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HorizontalDatum HorizontalDatum
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PrimeMeridian PrimeMeridian
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    internal class Projection : AbstractInformation
    {
        //ParameterList _parameters;
        string _classication;
        ProjectionParameter[] _projectionParameters;

        #region Constructors

        internal Projection(string name, ProjectionParameter[] projectionParameters, string classification, string remarks, string authority, string authorityCode)
            : base(remarks, authority, authorityCode, name, String.Empty, String.Empty)
        {
            _projectionParameters = projectionParameters;
            _classication = classification;
        }
        #endregion

        public ProjectionParameter GetParameter(int index)
        {
            return _projectionParameters[index];
        }

        public int NumParameters
        {
            get
            {
                return _projectionParameters.Length;
            }
        }

        public string ClassName
        {
            get
            {
                return _classication;
            }
        }
    }

    internal class ProjectedCoordinateSystem : CoordinateSystem
    {
        HorizontalDatum _horizontalDatum;
        AxisInfo[] _axisInfoArray;
        GeographicCoordinateSystem _geographicCoordSystem;
        Projection _projection;
        LinearUnit _linearUnit;

        
        internal ProjectedCoordinateSystem(
            HorizontalDatum horizontalDatum,
            AxisInfo[] axisInfoArray,
            GeographicCoordinateSystem geographicCoordSystem,
            LinearUnit linearUnit,
            Projection projection)
            : this(horizontalDatum, axisInfoArray, geographicCoordSystem, linearUnit, projection, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }
        internal ProjectedCoordinateSystem(
            HorizontalDatum horizontalDatum,
            AxisInfo[] axisInfoArray,
            GeographicCoordinateSystem geographicCoordSystem,
            LinearUnit linearUnit,
            Projection projection,
            string remarks, string authority, string authorityCode, string name, string alias, string abbreviation)
            : base(remarks, authority, authorityCode, name, alias, abbreviation)
        {

            if (axisInfoArray == null)
            {
                throw new ArgumentNullException("axisInfoArray");
            }
            if (geographicCoordSystem == null)
            {
                throw new ArgumentNullException("geographicCoordSystem");
            }
            if (projection == null)
            {
                throw new ArgumentNullException("projection");
            }
            if (linearUnit == null)
            {
                throw new ArgumentNullException("linearUnit");
            }
            _horizontalDatum = horizontalDatum;
            _axisInfoArray = axisInfoArray;
            _geographicCoordSystem = geographicCoordSystem;
            _projection = projection;
            _linearUnit = linearUnit;
        }


        public AxisInfo GetAxis(int dimension)
        {
            return _axisInfoArray[dimension];
        }

        public Unit GetUnits(int dimension)
        {
            throw new NotImplementedException();
        }

        public int Dimension
        {
            get
            {
                return _axisInfoArray.Length;
            }
        }

        public LinearUnit LinearUnit
        {
            get
            {
                return _linearUnit;
            }
        }

        public HorizontalDatum HorizontalDatum
        {
            get
            {
                return _horizontalDatum;
            }
        }

        public Envelope DefaultEnvelope
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public GeographicCoordinateSystem GeographicCoordinateSystem
        {
            get
            {
                return _geographicCoordSystem;
            }
        }

        public Projection Projection
        {
            get
            {
                return _projection;
            }
        }
    }

    internal class ParameterList : System.Collections.Specialized.ListDictionary
    {

        public double GetDouble(string key, double defaultValue)
        {
            if (this.Contains(key))
            {
                return (double)this[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public double GetDouble(string key)
        {
            if (this.Contains(key))
            {
                try
                {
                    return (double)this[key];
                }
                catch (Exception e)
                {
                    throw new ArgumentException(String.Format("key {0} has an invalid entry.", key), e);
                }
            }
            else
            {
                throw new ArgumentException(String.Format("The key with a value of '{0}' is not in the list.", key));
            }
        }

    }

    internal class Geotransformation : AbstractInformation
    {
        //private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        string _classication, _method;
        ParameterList _parameters;
        GeographicCoordinateSystem _geographicCoordinateSystem1;
        GeographicCoordinateSystem _geographicCoordinateSystem2;

        #region Constructors

        internal Geotransformation(string name, string method, GeographicCoordinateSystem geoCoordSys1,GeographicCoordinateSystem geoCoordSys2, ParameterList Parameters, string classification, string remarks, string authority, string authorityCode)
            : base(remarks, authority, authorityCode, name, String.Empty, String.Empty)
        {
            _parameters = Parameters;
            _method = method;
            _classication = classification;
            _geographicCoordinateSystem1 = geoCoordSys1;
            _geographicCoordinateSystem2 = geoCoordSys2;
        }
        #endregion

        public string ClassName
        {
            get
            {
                return _classication;
            }
        }

        public GeodeticDatum CreateGeodeticDatum()
        {
            GeodeticDatum datum = new GeodeticDatum(this.Name);

            switch(_method.ToLower()) 
            {
                case "geocentric_translation":
                    if (_parameters.Count != 3)
                        throw new Exception("Wrong number of parameters");
                    datum.X_Axis = _parameters.GetDouble("x_axis_translation");
                    datum.Y_Axis = _parameters.GetDouble("y_axis_translation");
                    datum.Z_Axis = _parameters.GetDouble("z_axis_translation");
                    break;
                case "coordinate_frame":
                case "position_vector":
                    if (_parameters.Count != 7)
                        throw new Exception("Wrong number of parameters");
                    datum.X_Axis = _parameters.GetDouble("x_axis_translation");
                    datum.Y_Axis = _parameters.GetDouble("y_axis_translation");
                    datum.Z_Axis = _parameters.GetDouble("z_axis_translation");
                    datum.X_Rotation = _parameters.GetDouble("x_axis_rotation");
                    datum.Y_Rotation = _parameters.GetDouble("y_axis_rotation");
                    datum.Z_Rotation = _parameters.GetDouble("z_axis_rotation");
                    datum.Scale_Diff = _parameters.GetDouble("scale_difference");
                    break;
                default:
                    throw new Exception("Unknown Method: " + _method);
            }

            return datum;
        }
    }
}

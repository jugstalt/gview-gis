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
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using Proj4Net.Datum;
using Proj4Net.Units;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    ///<summary>
    /// A map projection is a mathematical algorithm
    /// for representing a spheroidal surface 
    /// on a plane.
    /// <para/>
    /// A single projection
    /// defines a (usually infinite) family of
    /// <see cref="CoordinateReferenceSystem"/>s,
    /// distinguished by different values for the
    /// projection parameters.
    /// </summary>
    public class Projection 
#if !SILVERLIGHT
        : ICloneable
#endif
    {

        public Projection()
            :this(Datum.Ellipsoid.SPHERE, null)
        {
        }
        public Projection(Ellipsoid ellipsoid, IDictionary<String,String> parameterMap)
        {
            _ellipsoid = ellipsoid;
        }

        /// <summary>
        /// The minimum latitude of the bounds of this projection
        /// </summary>
        protected double _minLatitude = -Math.PI / 2;

        ///<summary>
        ///The minimum longitude of the bounds of this projection. This is relative to the projection centre.
        ///</summary>
        protected double _minLongitude = -Math.PI;

        ///<summary>
        ///The maximum latitude of the bounds of this projection
        ///</summary>
        protected double _maxLatitude = Math.PI / 2;

        ///<summary>
        /// The maximum longitude of the bounds of this projection. This is relative to the projection centre.
        ///</summary>
        protected double _maxLongitude = Math.PI;

        ///<summary>
        /// The latitude of the centre of projection
        ///</summary>
        protected double _projectionLatitude;

        ///<summary>The longitude of the centre of projection, in radians</summary>
        protected double _projectionLongitude;

        ///<summary>
        /// Standard parallel 1 (for projections which use it)
        ///</summary>
        protected double _projectionLatitude1;

        ///<summary>
        /// Standard parallel 2 (for projections which use it)
        ///</summary>
        protected double _projectionLatitude2;

        ///<summary>
        ///The projection alpha value
        ///</summary>
        protected double _alpha = Double.NaN;

        ///<summary>
        /// The projection lonc value
        /// </summary>
        protected double _lonc = Double.NaN;

        ///<summary>
        /// The projection scale factor
        ///</summary>
        protected double _scaleFactor = 1.0;

        ///<summary>The false Easting of this projection</summary>
        protected double _falseEasting;

        ///<summary>The false Northing of this projection</summary>
        protected double _falseNorthing;

        ///<summary>Indicates whether a Southern Hemisphere UTM zone</summary>
        protected Boolean _isSouth;

        ///<summary>The latitude of true scale. Only used by specific projections.</summary>
        protected double _trueScaleLatitude;

        ///<summary>The equator radius</summary>
        protected double _a;

        ///<summary>The eccentricity</summary>
        protected double _e;

        ///<summary>The eccentricity squared</summary>
        protected double _es;

        ///<summary>1-(eccentricity squared)</summary>
        protected double _oneEs;

        ///<summary>1/(1-(eccentricity squared))</summary>
        protected double _roneEs;

        ///<summary>The ellipsoid used by this projection</summary>
        protected Ellipsoid _ellipsoid;

        ///<summary>True if this projection is using a sphere (_es == 0)</summary>
        protected Boolean _spherical;

        ///<summary>True if this projection is geocentric</summary>
        protected Boolean _geocentric;

        ///<summary>The name of this projection</summary>
        private String _name;

        ///<summary>Conversion factor from metres to whatever units the projection uses.</summary>
        protected double _fromMetres = 1;

        ///<summary>The total scale factor = Earth radius * units</summary>
        protected double _totalScale;

        ///<summary>falseEasting, adjusted to the appropriate units using fromMetres</summary>
        private double _totalFalseEasting;

        ///<summary>falseNorthing, adjusted to the appropriate units using fromMetres</summary>
        private double _totalFalseNorthing;

        /// <summary>
        /// Units of this projection. Default is metres, but may even be degrees
        /// </summary>
        private Unit _unit;

        private Meridian _primeMeridian;

        // Some useful constants
// ReSharper disable InconsistentNaming
        protected const double EPS10 = 1e-10;
        protected const double RTD = ProjectionMath.RadiansToDegrees;
        protected const double DTR = ProjectionMath.DegreesToRadians;
// ReSharper restore InconsistentNaming

        protected void CopyParams(Projection to)
        {
            if (to == null)
                throw new ArgumentNullException("to");
            to._a = _a;
            to._alpha = _alpha;
            to._e = _e;
            to._ellipsoid = (Ellipsoid)_ellipsoid.Clone();
            to._es = _es;
            to._falseEasting = _falseEasting;
            to._falseNorthing = _falseNorthing;
            to._fromMetres = _fromMetres;
            to._isSouth = _isSouth;
            to._lonc = _lonc;
            to._maxLatitude = _maxLatitude;
            to._maxLongitude = _maxLongitude;
            to._minLatitude = _minLatitude;
            to._minLongitude = _minLongitude;
            to._name = _name;
            to._oneEs = _oneEs;
            to._projectionLatitude = _projectionLatitude;
            to._projectionLatitude1 = _projectionLatitude1;
            to._projectionLatitude2 = _projectionLatitude2;
            to._projectionLongitude = _projectionLongitude;
            to._roneEs = _roneEs;
            to._scaleFactor = _scaleFactor;
            to._spherical = _spherical;
            to._totalScale = _totalScale;
            to._trueScaleLatitude = _trueScaleLatitude;
            to._unit = _unit;
            to._primeMeridian = _primeMeridian;
        }

        public virtual Object Clone()
        {
            var projection = new Projection((Ellipsoid)Ellipsoid.Clone(), null);
            CopyParams(projection);
            return projection;
        }

        ///<summary>
        /// Projects a geographic point (in degrees), producing a projected result 
        /// (in the units of the target coordinate system).
        ///</summary>
        /// <param name="src">the input geographic coordinate (in degrees)</param>
        /// <param name="dst">the projected coordinate (in coordinate system units)</param>
        /// <returns>the target coordinate</returns>
        public virtual Coordinate Project(Coordinate src, Coordinate dst)
        {
            double x = src.X * DTR;
            if (_projectionLongitude != 0)
                x = ProjectionMath.NormalizeLongitude(x - _projectionLongitude);
            return ProjectRadians(x, src.Y * DTR, dst);
        }

        ///<summary>
        /// Projects a geographic point (in radians), producing a projected result
        /// (in the units of the target coordinate system).
        ///</summary>
        /// <param name="src">the input geographic coordinate (in radians)</param>
        /// <param name="dst">the projected coordinate (in coordinate system units)</param>
        /// <returns>the target coordinate</returns>
        public Coordinate ProjectRadians(Coordinate src, Coordinate dst)
        {
            double x = src.X;
            if (_projectionLongitude != 0)
                x = ProjectionMath.NormalizeLongitude(x - _projectionLongitude);
            return ProjectRadians(x, src.Y, dst);
        }

        ///<summary>
        /// Projects a geographic point (in radians), producing a projected result
        /// (in the units of the target coordinate system).
        ///</summary>
        private Coordinate ProjectRadians(double x, double y, Coordinate dst)
        {
            Project(x, y, dst);
            if (_unit == Units.Units.Degrees)
            {
                // convert radians to DD
                dst.X *= RTD;
                dst.Y *= RTD;
            }
            else
            {
                // assume result is in metres
                dst.X = _totalScale * dst.X + _totalFalseEasting;
                dst.Y = _totalScale * dst.Y + _totalFalseNorthing;
            }
            return dst;
        }

        ///<summary>
        /// Computes the projection of a given point (i.e. from geographics to projection space). 
        /// This should be overridden for all projections.
        ///</summary>
        /// <param name="x">the geographic x ordinate (in radians)</param>
        /// <param name="y">the geographic y ordinate (in radians)</param>
        /// <param name="dst">the projected coordinate (in coordinate system units)</param>
        /// <returns>the target coordinate</returns>
        public virtual Coordinate Project(double x, double y, Coordinate dst)
        {
            dst.X = x;
            dst.Y = y;
            return dst;
        }

        ///<summary>
        /// Inverse-projects a point (in the units defined by the coordinate system), 
        /// producing a geographic result (in degrees)
        ///</summary>
        /// <param name="src">the input projected coordinate (in coordinate system units)</param>
        /// <param name="dst">the inverse-projected geographic coordinate (in degrees)</param>
        /// <returns>the target coordinate</returns>
        public virtual Coordinate InverseProject(Coordinate src, Coordinate dst)
        {
            InverseProjectRadians(src, dst);
            dst.X *= RTD;
            dst.Y *= RTD;
            return dst;
        }

        ///<summary>
        /// Inverse-projects a point (in the units defined by the coordinate system), 
        /// producing a geographic result (in radians)
        ///</summary>
        /// <param name="src">the input projected coordinate (in coordinate system units)</param>
        /// <param name="dst">the inverse-projected geographic coordinate (in radians)</param>
        /// <returns>the target coordinate</returns>
        public Coordinate InverseProjectRadians(Coordinate src, Coordinate dst)
        {
            double x;
            double y;
            if (_unit == Units.Units.Degrees)
            {
                // convert DD to radians
                x = src.X * DTR;
                y = src.Y * DTR;
            }
            else
            {
                x = (src.X - _totalFalseEasting) / _totalScale;
                y = (src.Y - _totalFalseNorthing) / _totalScale;
            }
            ProjectInverse(x, y, dst);
            if (dst.X < -Math.PI)
                dst.X = -Math.PI;
            else if (dst.X > Math.PI)
                dst.X = Math.PI;
            if (_projectionLongitude != 0)
                dst.X = ProjectionMath.NormalizeLongitude(dst.X + _projectionLongitude);
            return dst;
        }

        ///<summary>
        /// Computes the inverse projection of a given point (i.e. from projection space to geographics). 
        /// This should be overridden for all projections.
        ///</summary>
        /// <param name="x">the projected x ordinate (in coordinate system units)</param>
        /// <param name="y">the projected y ordinate (in coordinate system units)</param>
        /// <param name="dst">the inverse-projected geographic coordinate</param>
        /// <returns>the target coordinate
        /// </returns>
        public virtual Coordinate ProjectInverse(double x, double y, Coordinate dst)
        {
            dst.X = x;
            dst.Y = y;
            return dst;
        }

        ///<summary>
        /// Computes the inverse projection of a given point (i.e. from projection space to geographics). 
        /// This should be overridden for all projections.
        ///</summary>
        /// <param name="src">the projected coordinate (in coordinate system units)</param>
        /// <param name="dst">the inverse-projected geographic coordinate</param>
        /// <returns>the target coordinate
        /// </returns>
        public virtual Coordinate ProjectInverse(Coordinate src, Coordinate dst)
        {
            dst.X = src.X;
            dst.Y = src.Y;
            return dst;
        }


        /// <summary>
        /// Tests whether this projection is conformal.<para/>
        /// A conformal projection preserves local angles.
        /// </summary>
        /// <returns><c>true</c> if this projection is conformal</returns>
        public virtual Boolean IsConformal
        {
            get { return false; }
        }

        /// <summary>
        /// Tests whether this projection is equal-area.<para/>
        /// An equal-area projection preserves relative sizes
        /// of projected areas.
        /// </summary>
        /// <returns><c>true</c> if this projection is equal area</returns>
        public virtual Boolean IsEqualArea
        {
            get { return false; }
        }

        /// <summary>
        /// Tests whether this projection has an inverse.
        /// <para/>
        /// If this method returns <tt>true</tt>
        /// then the <see cref="InverseProject"/> and
        /// <see cref="InverseProjectRadians"/>
        /// methods will return meaningful results.
        /// </summary>
        /// <returns><c>true</c> if this projection has an inverse</returns>
        public virtual Boolean HasInverse
        {
            get { return false; }
        }

        /// <summary>
        /// Tests whether under this projection lines of 
        /// latitude and longitude form a rectangular grid
        /// </summary>
        /// <returns><c>true</c> if lat/long lines form a rectangular grid for this projection</returns>
        public virtual Boolean IsRectilinear
        {
            get { return false; }
        }

        /// <summary>
        /// Returns <c>true</c> if latitude lines are parallel for this projection
        /// <para>Defaults to <see cref="IsRectilinear"/></para>
        /// </summary>
        public virtual Boolean ParallelsAreParallel
        {
            get { return IsRectilinear; }
        }

        ///<returns><c>true</c> if the given lat/long point is visible in this projection</returns>
        public virtual Boolean Inside(double x, double y)
        {
            x = NormalizeLongitude((float)(x * DTR - _projectionLongitude));
            return _minLongitude <= x && x <= _maxLongitude && _minLatitude <= y && y <= _maxLatitude;
        }

        ///<summary>Get/Set the name of this Projection</summary>
        public String Name
        {
            get {
                if (String.IsNullOrEmpty(_name))
                    return ToString();
                return _name;
                }
            set
            {
                _name = value;
            }
        }

        public Meridian PrimeMeridian { get { return _primeMeridian; } set { _primeMeridian = value; } }


        ///<summary>
        /// Get a string which describes this projection in PROJ.4 format.
        ///</summary>
        public String Proj4Description
        {
            get
            {
                var format = new AngleFormat(AngleFormat.PatternDDMMSS1, false);
                var sb = new StringBuilder();
                sb.Append(
                    "+proj=" + Name +
                    " +a=" + _a
                    );
                if (_es != 0)
                    sb.Append(" +es=" + _es);
                sb.Append(" +lon_0=");
                sb.Append(_projectionLongitude.ToString(format));
                sb.Append(" +lat_0=");
                sb.Append(_projectionLatitude.ToString(format));
                if (_falseEasting != 1)
                    sb.Append(" +x_0=" + _falseEasting);
                if (_falseNorthing != 1)
                    sb.Append(" +y_0=" + _falseNorthing);
                if (_scaleFactor != 1)
                    sb.Append(" +k=" + _scaleFactor);
                if (_fromMetres != 1)
                    sb.Append(" +fr_meters=" + _fromMetres);
                if (_primeMeridian.Name != NamedMeridian.Greenwich)
                    sb.Append(_primeMeridian.Proj4Description);
                return sb.ToString();
            }
        }

        public override String ToString()
        {
            return "None";
        }

        ///<summary>
        /// Get/Set the minimum latitude. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MinLatitude
        {
            get { return _minLatitude; }
            set { _minLatitude = value; }
        }

        ///<summary>
        /// Get/Set the maximum latitude. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MaxLatitude
        {
            get { return _maxLatitude; }
            set { _maxLatitude = value; }
        }

        ///<summary>
        /// Get/Set the minimum latitude in degrees. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MinLatitudeDegrees
        {
            get { return _minLatitude * RTD; }
            set { _minLatitude = DTR * value; }
        }

        ///<summary>
        /// Get/Set the maximum latitude in degrees. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MaxLatitudeDegrees
        {
            get { return _maxLatitude * RTD; }
            set { _maxLatitude = DTR * value; }
        }

        ///<summary>
        /// Get/Set the minimum latitude. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MinLongitude
        {
            get { return _minLongitude; }
            set { _minLongitude = value; }
        }

        ///<summary>
        /// Get/Set the maximum longitude. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MaxLongitude
        {
            get { return _maxLongitude; }
            set { _maxLongitude = value; }
        }

        ///<summary>
        /// Get/Set the minimum longitude in degrees. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MinLongitudeDegrees
        {
            get { return _minLongitude * RTD; }
            set { _minLongitude = DTR * value; }
        }

        ///<summary>
        /// Get/Set the maximum longitude in degrees. This is only used for Shape clipping and doesn't affect projection.
        ///</summary>
        public Double MaxLongitudeDegrees
        {
            get { return _maxLongitude * RTD; }
            set { _maxLongitude = DTR * value; }
        }

        ///<summary>
        /// Set the projection atitude in radians.
        ///</summary>
        public Double ProjectionLatitude
        {
            get { return _projectionLatitude; }
            set { _projectionLatitude = value; }
        }

        ///<summary>
        /// Set the projection latitude in degrees.
        ///</summary>
        public Double ProjectionLatitudeDegrees
        {
            get { return _projectionLatitude * RTD; }
            set { _projectionLatitude = DTR * value; }
        }


        ///<summary>
        /// Set the projection longitude in radians.
        ///</summary>
        public Double ProjectionLongitude
        {
            get { return _projectionLongitude; }
            set { _projectionLongitude = value; }
        }

        ///<summary>
        /// Set the projection longitude in degrees.
        ///</summary>
        public Double ProjectionLongitudeDegrees
        {
            get { return _projectionLongitude*RTD;}
            set { _projectionLongitude = DTR*value; }
        }

        ///<summary>
        /// Get/Set the latitude of true scale in radians. This is only used by certain projections.
        ///</summary>
        public double TrueScaleLatitude
        {
            get { return _trueScaleLatitude; }
            set { _trueScaleLatitude = value; }
        }

        ///<summary>
        /// Set the latitude of true scale in degrees. This is only used by certain projections.
        ///</summary>
        public Double TrueScaleLatitudeDegrees
        {
            get { return _trueScaleLatitude*RTD; }
            set { _trueScaleLatitude = DTR*value; }
        }

        ///<summary>
        /// Get/Set the projection latitude2 (in radians).
        ///</summary>
        public double ProjectionLatitude1
        {
            get { return _projectionLatitude1; }
            set { _projectionLatitude1 = value; }
        }

        ///<summary>
        /// Get/Set the projection latitude2 in degrees.
        ///</summary>
        public double ProjectionLatitude1Degrees
        {
            get { return _projectionLatitude1 * RTD; }
            set { _projectionLatitude1 = DTR * value; }
        }


        ///<summary>
        /// Get/Set the projection latitude2 (in radians).
        ///</summary>
        public double ProjectionLatitude2
        {
            get { return _projectionLatitude2;}
            set {_projectionLatitude2 = value;}
        }

        ///<summary>
        /// Get/Set the projection latitude2 in degrees.
        ///</summary>
        public double ProjectionLatitude2Degrees
        {
            get { return _projectionLatitude2 * RTD; }
            set { _projectionLatitude2 = DTR * value; }
        }

        ///<summary>
        /// Get/Set the Alpha value (in Radinas)
        ///</summary>
        public double Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        ///<summary>
        /// Get/Set the Alpha value in Degrees
        ///</summary>
        public double AlphaDegrees
        {
            get { return _alpha*RTD; }
            set { _alpha = DTR*value; }
        }

        /**
         * Sets the lonc value.
         */
        public double LonCDegrees
        {
            get { return _lonc; }
            set { _lonc = DTR * value;}
        }

        /**
         * Set the false Northing in projected units.
         */
        public double FalseNorthing
        {
            get { return _falseNorthing; }
            set { _falseNorthing = value; }
        }

        /**
         * Set the false Easting in projected units.
         */
        public double FalseEasting
        {
            get { return _falseEasting; }
            set { _falseEasting = value; }
        }

        public Boolean SouthernHemisphere
        {
            get { return _isSouth; }
            set { _isSouth = value; }
        }

        ///<summary>
        /// Get/Set the projection scale factor. 
        /// This value is called "k0" in PROJ.4.
        /// This is set to 1 by default.
        ///</summary>
        public Double ScaleFactor
        {
            get { return _scaleFactor; }
            set { _scaleFactor = value; }
        }

        /// <summary>
        /// Gets whether projection is sperical or not.
        /// </summary>
        public bool Spherical
        {
            get { return _spherical; }
        }

        /// <summary>
        /// Gets whether projection is geocentric or not.
        /// </summary>
        public bool Geocentric
        {
            get { return _geocentric; }
        }

        /// <summary>
        /// Gets the Equator radius
        /// </summary>
        public double EquatorRadius
        {
            get { return _a; }
        }

        /// <summary>
        /// Gets/Sets Eccentricity. Dependant values (<see cref="EccentricitySquared"/>, _oneEs, _roneEs are set accordingly
        /// </summary>
        public double Eccentricity
        {
            get { return _e; }
            protected set
            {
                _e = value;
                _es = Math.Pow(value, 2d);
                _oneEs = 1d - _es;
                _roneEs = 1d/_oneEs;
            }
        }

        /// <summary>
        /// Gets/Sets EccentricitySquared. Dependant values (<see cref="Eccentricity"/>, _oneEs, _roneEs are set accordingly
        /// </summary>
        public double EccentricitySquared
        {
            get { return _es; }
            protected set
            {
                _es = value;
                _oneEs = 1d - _oneEs;
                _roneEs = 1d/_roneEs;
                _e = Math.Sqrt(_es);
            }
        }

        ///<summary>
        /// Set the conversion factor from metres to projected units. This is set to 1 by default.
        ///</summary>
        public Double FromMetres
        {
            get { return _fromMetres; }
            set { _fromMetres = value; }
        }

        public Ellipsoid Ellipsoid
        {
            get
            {
                return _ellipsoid;
            }
            set
            {
                _ellipsoid = value;
                _a = value.EquatorRadius;
                _e = value.Eccentricity;
                _es = value.EccentricitySquared;
            }
        }

        ///<summary>
        /// Returns the ESPG code for this projection, or 0 if unknown.
        ///</summary>
        public Unit Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        ///<summary>
        /// Initialize the projection. This should be called after setting parameters and before using the projection.
        /// This is for performance reasons as initialization may be expensive.
        ///</summary>
        public virtual void Initialize()
        {
            _spherical = (EquatorRadius == 0.0);
            _oneEs = 1 - _es;
            _roneEs = 1.0 / _oneEs;
            _totalScale = EquatorRadius * FromMetres;
            _totalFalseEasting = FalseEasting * FromMetres;
            _totalFalseNorthing = FalseNorthing * FromMetres;
        }

        public virtual void Initialize(IDictionary<String, String> parameters)
        {}

        public static float NormalizeLongitude(float angle)
        {
            if (Double.IsInfinity(angle) || Double.IsNaN(angle))
                throw new ArgumentException("Infinite or NaN longitude", "angle");
            while (angle > 180)
                angle -= 360;
            while (angle < -180)
                angle += 360;
            return angle;
        }

        public static double NormalizeLongitudeRadians(double angle)
        {
            if (Double.IsInfinity(angle) || Double.IsNaN(angle))
                throw new ArgumentException("Infinite or NaN longitude");
            while (angle > Math.PI)
                angle -= ProjectionMath.TwoPI;
            while (angle < -Math.PI)
                angle += ProjectionMath.TwoPI;
            return angle;
        }

    }

}
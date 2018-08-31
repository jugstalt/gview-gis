using System;
using System.Globalization;
using System.Text;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net
{
    /// <summary>
    /// Stores a the coordinates for a position  
    /// defined relative to some <see cref="CoordinateReferenceSystem"/>.
    /// The coordinate is defined via X, Y, and optional Z ordinates. 
    /// Provides utility methods for comparing the ordinates of two positions and
    /// for creating positions from Strings/storing positions as strings.
    /// <para/>
    /// The primary use of this class is to represent coordinate
    /// values which are to be transformed
    /// by a <see cref="ICoordinateTransform"/>.
    /// </summary>
    public class ProjCoordinate : Coordinate
    {
        public static String DECIMAL_FORMAT_PATTERN = "{0:N}";
        public static NumberFormatInfo DECIMAL_FORMAT;
        static ProjCoordinate()
        {
            DECIMAL_FORMAT = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
            DECIMAL_FORMAT.NumberGroupSeparator = string.Empty;
        }

        ///**
        // * The X ordinate for this point. 
        // * <p>
        // * Note: This member variable
        // * can be accessed directly. In the future this direct access should
        // * be replaced with getter and setter methods. This will require 
        // * refactoring of the Proj4Net code base.
        // */
        //private double _x;

        ///**
        // * The Y ordinate for this point. 
        // * <p>
        // * Note: This member variable
        // * can be accessed directly. In the future this direct access should
        // * be replaced with getter and setter methods. This will require 
        // * refactoring of the Proj4Net code base.
        // */
        //private double _y;

        ///**
        // * The Z ordinate for this point. 
        // * If this variable has the value <tt>Double.NaN</tt>
        // * then this coordinate does not have a Z value.
        // * <p>
        // * Note: This member variable
        // * can be accessed directly. In the future this direct access should
        // * be replaced with getter and setter methods. This will require 
        // * refactoring of the Proj4Net code base.
        // */
        //private double _z;

        /// <summary>
        /// Creates an instance of this class with default ordinate values.
        /// </summary>
        public ProjCoordinate()
            : this(0.0, 0.0)
        {
        }

        /// <summary>
        /// Creates a ProjCoordinate using the provided double parameters.
        /// The first double parameter is the <see cref="X"/> ordinate (or easting), 
        /// the second double parameter is the <see cref="Y"/> ordinate (or northing), 
        /// and the third double parameter is the <see cref="Z"/> ordinate (elevation or height).
        /// <para/>
        /// Valid values should be passed for all three (3) double parameters. If
        /// you want to create a horizontal-only point without a valid Z value, use
        /// the constructor defined in this class that only accepts two (2) double
        /// parameters.
        /// </summary>
        /// <seealso cref="ProjCoordinate(double, double)"/>
        public ProjCoordinate(double argX, double argY, double argZ)
            : base(argX, argY, argZ)
        {
        }

        /// <summary>
        /// Creates a ProjCoordinate using the provided double parameters.
        /// The first double parameter is the <see cref="X"/> ordinate (or easting), 
        /// the second double parameter is the <see cref="Y"/> ordinate (or northing). 
        /// This constructor is used to create a "2D" point, so the Z ordinate
        /// is automatically set to Double.NaN. 
        /// </summary>
        public ProjCoordinate(double argX, double argY)
            :base(argX, argY)
        {
        }

        /// <summary>
        /// Create a ProjCoordinate by parsing a String in the same format as returned
        /// by the <see cref="ToString"/> method defined by this class.
        /// </summary>
        /// <param name="argToParse">The string to parse</param>
        public ProjCoordinate(String argToParse)
        {
            // Make sure the String starts with "ProjCoordinate: ".
            var startsWith = argToParse.StartsWith("ProjCoordinate: ");

            if (!startsWith)
            {
                var toThrow = new ArgumentException
                    ("The input string was not in the proper format.", "argToParse");

                throw toThrow;
            }

            // 15 characters should cut out "ProjCoordinate: ".
            var chomped = argToParse.Substring(16);

            // Get rid of the starting and ending square brackets.

            var withoutFrontBracket = chomped.Substring(1);

            // Calc the position of the last bracket.
            int length = withoutFrontBracket.Length;
            int positionOfCharBeforeLast = length - 2;
            String withoutBackBracket = withoutFrontBracket.Substring(0,
                                                                      positionOfCharBeforeLast);

            // We should be left with just the ordinate values as strings, 
            // separated by spaces. Split them into an array of Strings.
            String[] parts = withoutBackBracket.Split(' ');

            // Get number of elements in Array. There should be two (2) elements
            // or three (3) elements.
            // If we don't have an array with two (2) or three (3) elements,
            // then we need to throw an exception.
            if (parts.Length != 2)
            {
                if (parts.Length != 3)
                {
                    var toThrow = new ArgumentException
                        ("The input string was not in the proper format.", "argToParse");

                    throw toThrow;
                }
            }

            // Convert strings to doubles.
            X = Double.Parse(parts[0]);
            Y = Double.Parse(parts[1]);

            // You might not always have a Z ordinate. If you do, set it.
            if (parts.Length == 3)
            {
                Z = Double.Parse(parts[2]);
            }
        }

        public Boolean HasValidZOrdinate
        {
            get { return CoordinateChecker.HasValidZOrdinate(this); }
        }

        /// <summary>
        /// Indicates if this ProjCoordinate has valid X ordinate and Y ordinate
        /// values. Values are considered invalid if they are Double.NaN or 
        /// positive/negative infinity.
        /// </summary>
        public Boolean HasValidXandYOrdinates
        {
            get { return CoordinateChecker.HasValidXandYOrdinate(this); }
        }

        #region Coordinate Members

        public double Distance(ProjCoordinate p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            var tmpP = (p is ProjCoordinate)
                            ? p as ProjCoordinate
                            : new ProjCoordinate(p.X, p.Y, p.Z);

            double dx, dy, dz = 0;
            if (HasValidXandYOrdinates && tmpP.HasValidXandYOrdinates)
            {
                dx = X - p.X;
                dy = Y - p.Y;
            }
            else
                throw new Proj4NetException();

            if (HasValidZOrdinate && tmpP.HasValidZOrdinate)
                dz = Z - tmpP.Z;

            if (HasValidZOrdinate != tmpP.HasValidZOrdinate)
                throw new Proj4NetException();

            return Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }

        public bool Equals2D(ProjCoordinate other)
        {
            return CoordinateChecker.AreXOrdinatesEqual(this, other, 0) &&
                   CoordinateChecker.AreYOrdinatesEqual(this, other, 0);
        }

        public bool Equals3D(ProjCoordinate other)
        {
            if (!Equals2D(other))
                return false;
            return CoordinateChecker.AreZOrdinatesEqual(this, other, 0);
        }

        public int CompareTo(ProjCoordinate other)
        {
            if (Equals(other))
                return 0;

            if (!CoordinateChecker.AreXOrdinatesEqual(this, other, 0))
            {
                return X < other.X ? -1 : 1;
            }
            if (!CoordinateChecker.AreYOrdinatesEqual(this, other, 0))
                return Y < other.Y ? -1 : 1;

            if (CoordinateChecker.HasValidZOrdinate(this))
            {
                if (!CoordinateChecker.HasValidZOrdinate(other))
                    return -1;
                return Z < other.Z ? -1 : 1;
            }

            if (CoordinateChecker.HasValidZOrdinate(other))
                return Z < other.Z ? -1 : 1;

            return 0;
        }

        public bool Equals(ProjCoordinate other)
        {
            return CoordinateChecker.AreXOrdinatesEqual(this, other, 0) &&
                   CoordinateChecker.AreYOrdinatesEqual(this, other, 0) &&
                   CoordinateChecker.AreZOrdinatesEqual(this, other, 0);
        }

        public new object Clone()
        {
            return new ProjCoordinate(X, Y, Z);
        }

        #endregion

        /// <summary>
        /// Returns a string representing the ProjPoint in the format:
        /// <tt>ProjCoordinate[X Y Z]</tt>.
        /// <para/>
        /// Example: 
        /// <pre>ProjCoordinate[6241.11 5218.25 12.3]</pre>
        /// </summary>
        public override String ToString()
        {
            var builder = new StringBuilder();
            builder.Append("ProjCoordinate[");
            builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, X);
            builder.Append(" ");
            builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, Y);
            builder.Append(" ");
            builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, Z);
            builder.Append("]");

            return builder.ToString();
            
            //return "ProjCoordinate" + ToShortString();
        }

        /// <summary>
        /// Returns a string representing the ProjPoint in the format:
        /// <tt>[X Y]</tt> or
        /// <tt>[X, Y, Z]</tt>.
        /// Z is not displayed if it is <see cref="Double.NaN"/>.
        /// <para/>
        /// Example: 
        /// <pre>[6241.11, 5218.25, 12.3]</pre>
        /// </summary>
        public String ToShortString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, X);
            builder.Append(", ");
            builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, Y);
            if (!CoordinateChecker.HasValidZOrdinate(this))
            {
                builder.Append(", ");
                builder.AppendFormat(DECIMAL_FORMAT, DECIMAL_FORMAT_PATTERN, Z);
            }
            builder.Append("]");

            return builder.ToString();
        }
    }
}

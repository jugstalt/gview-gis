using System;
using System.Globalization;
using System.Text;
using Proj4Net.Utility;

namespace Proj4Net.Units
{
    /// <summary>
    /// Exception thrown if angle format string is not valid.
    /// </summary>
    public class AngleFormatException : FormatException
    {
        public AngleFormatException(String qualifier)
            :base(String.Format("'{0}' is present more than once in format string", qualifier))
        {
        }
    }
    
    /// <summary>
    /// Class for formatting and parsing angles in D/M/S notation
    /// </summary>
    /// <example>
    /// //Create angle format instance
    /// AngleFormat af = new AngleFormat();
    /// //
    /// string sDMS = String.Format(af, "{0:DdMmSsN}", Math.PI);
    /// //Use angle format to parse values
    /// Double val = af.Parse(sDMS);
    /// </example>
    public class AngleFormat : IFormatProvider, ICustomFormatter
    {

        /// <summary>
        /// Pattern to print e.g. PI as 180d00
        /// </summary>
        public const String PatternDDMM = "DdM";
        /// <summary>
        /// Pattern to print e.g. PI as 180d00'00"
        /// </summary>
        public const String PatternDDMMSS1 = "DdM'S\"";
        /// <summary>
        /// Pattern to print e.g. PI as 180°00'00"
        /// </summary>
        public const String PatternDDMMSS2 = "D\xb0M'S\"";
        /// <summary>
        /// Pattern to print e.g. PI/2 as 90d00m00s
        /// </summary>
        public const String PatternDDMMSS3 = "DdMmSs";
        /// <summary>
        /// Pattern to print e.g. PI as 180d00'00"W or -PI 180d00'00"E
        /// </summary>
        public const String PatternLongitudeDDMMSS = "DdM'S\"W";
        /// <summary>
        /// Pattern to print e.g. PI/2 as 90°00'00"N or -PI/2 180°00'00"S
        /// </summary>
        public const String PatternLatitudeDDMMSS = "DdM'S\"N";
        /// <summary>
        /// Pattern to print e.g. PI/2 as 90.0
        /// </summary>
        public const String PatternDecimal = "D.F";

        #region Fields

        private static readonly NumberFormatInfo AngleNumberFormatInfo = 
            (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        private readonly String _pattern;
        private readonly Boolean _isDegrees;
        
        #endregion

        #region Object construction and disposal

        /// <summary>
        /// Creates an Instance of this class using <see cref="PatternDDMMSS1"/> and assuming that parsed values are to be returned in radians.
        /// </summary>
        public AngleFormat()
            : this(PatternDDMMSS1)
        {
        }
        /// <summary>
        /// Creates an instance of this class using the given pattern and assuming that parsed values are to be returned in radians.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use for formatting
        /// <para>The following characters represent degree values:
        /// <list type="Table">
        /// <listheader><term>Literal</term><description>Value to output</description></listheader>
        /// <item><term>D</term><description>Degree)</description></item>
        /// <item><term>M</term><description>Minute</description></item>
        /// <item><term>S</term><description>Second</description></item>
        /// <item><term>F</term><description>Fraction</description></item>
        /// <item><term>N, S*)</term><description>Indicates that value</description></item>
        /// <item><term>W*)</term><description></description></item>
        /// <item><term>all other</term><description>passed 1:1</description></item>
        /// </list>
        /// </para>
        /// </param>
        /// <exception cref="AngleFormatException">Thrown if degree component (degree, minute, second, fraction) is presen more than once</exception>
        public AngleFormat(String pattern)
            : this(pattern, false)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the given pattern and assuming that parsed values are to be returned in radians.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="isDegrees"></param>
        /// <exception cref="AngleFormatException">Thrown if degree component (degree, minute, second, fraction) is presen more than once</exception>
        public AngleFormat(String pattern, Boolean isDegrees)
        {
            _pattern = pattern;
            _isDegrees = isDegrees;
            AngleNumberFormatInfo.NumberDecimalDigits = 0;
            AngleNumberFormatInfo.NumberGroupSeparator = "";
            CheckFormatString(_pattern);
        }
        #endregion

        #region Implementation of IFormatProvider

        public object GetFormat(Type formatType)
        {
            // Determine whether custom formatting object is requested.
            return (formatType == typeof(ICustomFormatter))
                ? this
                : null;
        }

        #endregion

        #region Implementation of ICustomFormatter

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is float || arg is double))
            {
                if (arg is IFormattable)
                    return ((IFormattable)arg).ToString(format, formatProvider);

                return arg != null ? arg.ToString() : String.Empty;
            }

            if (String.IsNullOrEmpty(format))
                format = _pattern;
            else
            {
                CheckFormatString(format);
            }

            int length = format.Length;
            Boolean negative = false;

            Double number = Convert.ToDouble(arg);
            if (number < 0)
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    char c = _pattern[i];
                    if (c == 'W' || c == 'N')
                    {
                        number = -number;
                        negative = true;
                        break;
                    }
                }
            }

            double ddmmss = _isDegrees 
                ? number 
                : ProjectionMath.ToDegrees(number);
            int iddmmss = (int)Math.Round(ddmmss * 3600);
            if (iddmmss < 0)
                iddmmss = -iddmmss;
            int fraction = iddmmss % 3600;

            StringBuilder result = new StringBuilder();
            foreach(char c in format)
            {
                int f;
                switch (c)
                {
                    case 'R':
                        result.Append(number);
                        break;
                    case 'D':
                        result.Append((int)ddmmss);
                        break;
                    case 'M':
                        f = fraction / 60;
                        if (f < 10)
                            result.Append('0');
                        result.Append(f);
                        break;
                    case 'S':
                        f = fraction % 60;
                        if (f < 10)
                            result.Append('0');
                        result.Append(f);
                        break;
                    case 'F':
                        result.Append(fraction);
                        break;
                    case 'W':
                        if (negative)
                            result.Append('W');
                        else
                            result.Append('E');
                        break;
                    case 'N':
                        if (negative)
                            result.Append('S');
                        else
                            result.Append('N');
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }

        #endregion

        private static void CheckFormatString(String format)
        {
            char[] singles = new char[] {'D','M','S','W', 'N'};
            foreach (var c in singles)
            {
                int firstOccurence = format.IndexOf(c);
                if (firstOccurence > -1 &&
                    format.IndexOf(c, firstOccurence + 1) > -1)
                    throw new AngleFormatException(c.ToString());
            }

        }

        public Double Parse(String text)
        {
            double d = 0, m = 0, s = 0;
            double result;
            Boolean negate = false;
            int length = text.Length;
            if ( length > 0)
            {
                char c = text[length-1];
                switch (c)
                {
                    case 'W':
                    case 'S':
                        negate = true;
                        text = text.Substring(0, text.Length - 1);
                        break;
                    case 'E':
                    case 'N':
                        text = text.Substring(0, text.Length - 1);
                        break;
                }
            }

            int i = text.IndexOf('d');
            if (i == -1)
                i = text.IndexOf('\u00b0');
            if (i != -1)
            {
                String dd = text.Substring(0, i);
                String mmss = text.Substring(i + 1);
                d = Convert.ToDouble(dd, CultureInfo.InvariantCulture);
                i = mmss.IndexOf('m');
                if (i == -1)
                    i = mmss.IndexOf('\'');
                if (i != -1)
                {
                    if (i != 0)
                    {
                        String mm = mmss.Substring(0, i);
                        m = Convert.ToDouble(mm, CultureInfo.InvariantCulture);
                    }
                    if (mmss.EndsWith("s") || mmss.EndsWith("\""))
                        mmss = mmss.Substring(0, mmss.Length - 1);
                    if (i != mmss.Length - 1)
                    {
                        String ss = mmss.Substring(i + 1);
                        s = Convert.ToDouble(ss, CultureInfo.InvariantCulture);
                    }
#if SILVERLIGHT
                    if (m < 0 || m > 59)
                        throw new ArgumentOutOfRangeException("m", "Minutes must be between 0 and 59");
                    if (s < 0 || s >= 60)
                        throw new ArgumentOutOfRangeException("s", "Seconds must be between 0 and 59");
#else
                    if (m < 0 || m > 59)
                        throw new ArgumentOutOfRangeException("m", m, "Minutes must be between 0 and 59");
                    if (s < 0 || s >= 60)
                        throw new ArgumentOutOfRangeException("s", s, "Seconds must be between 0 and 59");
#endif
                }
                else if (i != 0)
                    m = Convert.ToDouble(mmss, CultureInfo.InvariantCulture);
                if (_isDegrees)
                    result = ProjectionMath.DegreesMinutesSecondsToDegrees(d, m, s);
                else
                    result = ProjectionMath.DegreesMinutesSecondsToRadians(d, m, s);
            }
            else
            {
                result = Convert.ToDouble(text, CultureInfo.InvariantCulture);
                if (!_isDegrees)
                    result = ProjectionMath.ToRadians(result);
            }

            if (negate)
                result = -result;

            return result;
        }

    }
}
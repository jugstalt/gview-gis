using System;
using System.Globalization;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Projection
{
    /// <summary>
    /// List of named meridians
    /// </summary>
    public enum NamedMeridian
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Greenwich, England
        /// </summary>
        Greenwich = 8901,
        /// <summary>
        /// Lisbon, Portugal
        /// </summary>
        Lisbon = 8902,
        /// <summary>
        /// Paris, France
        /// </summary>
        Paris = 8903,
        /// <summary>
        /// Bogota, Colombia
        /// </summary>
        Bogota = 8904,
        /// <summary>
        /// Madrid, Spain
        /// </summary>
        Madrid = 8905,
        /// <summary>
        /// Rome, Italy
        /// </summary>
        Rome = 8906,
        /// <summary>
        /// Berne, Switzerland
        /// </summary>
        Bern = 8907,
        /// <summary>
        /// Jakarta, Indonesia
        /// </summary>
        Jakarta = 8908,
        /// <summary>
        ///  Brasil
        /// </summary>
        Ferro = 8909,
        /// <summary>
        /// Brussels, Belgiuum
        /// </summary>
        Brussels = 8910,
        /// <summary>
        /// Stockholm, Sweden
        /// </summary>
        Stockholm = 8911,
        /// <summary>
        /// Athens, Greece
        /// </summary>
        Athens = 8912,
        /// <summary>
        /// Oslo, Norway
        /// </summary>
        Oslo = 8913, 

        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = int.MaxValue,
    }
    
    /// <summary>
    /// A meridian structure
    /// </summary>
    public struct Meridian
    {
        private const double Epsilon = 1e-7;

        private readonly double _longitude;
        private NamedMeridian _name;

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
		public double Longitude
        {
            get { return _longitude; }
        }
		
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        public NamedMeridian Name
        {
            get
            {
                TestName();
                return _name;
            }
        }
		
		/// <summary>
		/// Gets the code.
		/// </summary>
		/// <value>
		/// The code.
		/// </value>
        public int Code
        {
            get 
            { 
                TestName();
                return (int)_name;
            }
        }
		
		/// <summary>
		/// Gets the proj4 description.
		/// </summary>
		/// <value>
		/// The proj4 description.
		/// </value>
        public string Proj4Description
        {
            get
            {
                TestName();
				string rhs = _name != NamedMeridian.Unknown
                           				? _name.ToString().ToLower()
                           				: ProjectionMath.ToDegrees(_longitude).ToString(NumberFormatInfo.InvariantInfo);
                return  " +pm=" + rhs;
            }
        }

        private void TestName()
        {
            if (_name == NamedMeridian.Undefined)
            {
                QueryNameAndCode(_longitude, out _name);
            }
        }

        private static void QueryNameAndCode(double longitude, out NamedMeridian name)
        {
            //in degrees
            var val = longitude/ProjectionMath.DegreesToRadians;
            if (Math.Abs(val) < Epsilon)
            {
                name = NamedMeridian.Greenwich;
                return;
            }
            if (Math.Abs(val - -9.131906111) < Epsilon)
            {
                name = NamedMeridian.Lisbon;
                return;
            }
            if (Math.Abs(val - 2.337229167) < Epsilon)
            {
                name = NamedMeridian.Paris;
                return;
            }
            if (Math.Abs(val - -74.08091667) < Epsilon)
            {
                name = NamedMeridian.Bogota;
                return;
            }
            if (Math.Abs(val - -3.687938889) < Epsilon)
            {
                name = NamedMeridian.Madrid;
                return;
            }
            if (Math.Abs(val - 12.45233333) < Epsilon)
            {
                name = NamedMeridian.Rome;
                return;
            }
            if (Math.Abs(val - 7.439583333) < Epsilon)
            {
                name = NamedMeridian.Bern;
                return;
            }
            if (Math.Abs(val - 106.8077194) < Epsilon)
            {
                name = NamedMeridian.Jakarta;
                return;
            }
            if (Math.Abs(val - -17.66666667) < Epsilon)
            {
                name = NamedMeridian.Ferro;
                return;
            }
            if (Math.Abs(val - 4.367975) < Epsilon)
            {
                name = NamedMeridian.Brussels;
                return;
            }
            if (Math.Abs(val - 18.05827778) < Epsilon)
            {
                name = NamedMeridian.Stockholm;
                return;
            }
            if (Math.Abs(val - 23.7163375) < Epsilon)
            {
                name = NamedMeridian.Athens;
                return;
            }
            if (Math.Abs(val - 10.72291667) < Epsilon)
            {
                name = NamedMeridian.Oslo;
                return;
            }

            name = NamedMeridian.Unknown;
        }

        /// <summary>
        /// Creates a new Meridian
        /// </summary>
        /// <param name="name">Name of meridian</param>
        /// <param name="longitude">Longitude of meridian in radians</param>
        private Meridian(NamedMeridian name, double longitude)
        {
            _name = name;
            _longitude = longitude;
        }

        /// <summary>
        /// Factory method to create meridians by their name>
        /// </summary>
        /// <param name="name">The name of the meridian</param>
        /// <returns>The meridian</returns>
        public static Meridian CreateByName(string name)
        {
            var namedMeridian = (NamedMeridian) Enum.Parse(typeof (NamedMeridian), name, true);
            return CreateByNamedMeridian(namedMeridian);
        }

        /// <summary>
        /// Factory method to create meridians by their <see cref="NamedMeridian"/>
        /// </summary>
        /// <param name="meridan">The named meridian</param>
        /// <returns>The meridian</returns>
        public static Meridian CreateByNamedMeridian(NamedMeridian meridan)
        {
            switch (meridan)
            {
                case NamedMeridian.Greenwich:
                    return new Meridian(meridan, 0);
                case NamedMeridian.Lisbon:
                    return new Meridian(meridan, ProjectionMath.ToRadians(-9.131906111));
                case NamedMeridian.Paris:
                    return new Meridian(meridan, ProjectionMath.ToRadians(2.337229167));
                case NamedMeridian.Bogota:
                    return new Meridian(meridan, ProjectionMath.ToRadians(-74.08091667));
                case NamedMeridian.Madrid:
                    return new Meridian(meridan, ProjectionMath.ToRadians(-3.687938889));
                case NamedMeridian.Rome:
                    return new Meridian(meridan, ProjectionMath.ToRadians(12.45233333));
                case NamedMeridian.Bern:
                    return new Meridian(meridan, ProjectionMath.ToRadians(7.439583333));
                case NamedMeridian.Jakarta:
                    return new Meridian(meridan, ProjectionMath.ToRadians(106.8077194));
                case NamedMeridian.Ferro:
                    return new Meridian(meridan, ProjectionMath.ToRadians(-17.66666667));
                case NamedMeridian.Brussels:
                    return new Meridian(meridan, ProjectionMath.ToRadians(4.367975));
                case NamedMeridian.Stockholm:
                    return new Meridian(meridan, ProjectionMath.ToRadians(18.05827778));
                case NamedMeridian.Athens:
                    return new Meridian(meridan, ProjectionMath.ToRadians(23.7163375));
                case NamedMeridian.Oslo:
                    return new Meridian(meridan, ProjectionMath.ToRadians(10.72291667));
            }
            throw new ArgumentOutOfRangeException("meridan");
        }

        /// <summary>
        /// Factory method to create meridians by their degree value
        /// </summary>
        /// <param name="degree">The longitude of the meridian in degrees</param>
        /// <returns>The meridian</returns>
        public static Meridian CreateByDegree(double degree)
        {
            return new Meridian(NamedMeridian.Undefined, ProjectionMath.ToRadians(degree));
        }

        public override string ToString()
        {
            return Proj4Description.Trim();
        }
		
		public override int GetHashCode ()
		{
			return 6524 ^ _longitude.GetHashCode ();
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is Meridian))
				return false;
			
			var other = (Meridian)obj;
			return this == other;
		}
		
        public static bool operator==(Meridian lhs, Meridian rhs)
        {
            lhs.TestName();
            rhs.TestName();
            if (lhs._name == rhs._name && lhs._name != NamedMeridian.Unknown)
                return true;
            return Math.Abs(lhs._longitude - rhs._longitude) < Epsilon;
        }

        public static bool operator !=(Meridian lhs, Meridian rhs)
        {
            return !(lhs == rhs);
        }

        public Coordinate InverseAdjust(Coordinate geoCoord)
        {
            if (!double.IsPositiveInfinity(geoCoord.X))
                geoCoord.X += _longitude;
            return geoCoord;
        }

        public Coordinate Adjust(Coordinate geoCoord)
        {
            if (!double.IsPositiveInfinity(geoCoord.X))
                geoCoord.X -= _longitude;
            return geoCoord;
        }
    }
}

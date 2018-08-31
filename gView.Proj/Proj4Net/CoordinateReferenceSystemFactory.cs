using System;
using System.Text.RegularExpressions;
using GeoAPI.Geometries;
using Proj4Net.IO;
using Proj4Net.Parser;

namespace Proj4Net
{
    /// <summary>
    /// A factory which can create <see cref="CoordinateReferenceSystem"/>s
    /// from a variety of ways of specifying them.
    /// <para/>
    /// This is the primary way of creating coordinate systems 
    /// for carrying out projections transformations.
    /// <para/>
    /// <tt>CoordinateReferenceSystem</tt>s can be used to
    /// define <see cref="ICoordinateTransform"/>s to perform transformations
    /// on <see cref="Coordinate"/>s. 
    ///  </summary>
    /// <author>Martin Davis</author>
    public class CoordinateReferenceSystemFactory
    {
        private static readonly Proj4FileReader CRSReader = new Proj4FileReader();

        private static readonly Registry _registry = new Registry();
        // TODO: add method to allow reading from arbitrary PROJ4 CS file

        ///<summary>
        /// Gets the <see cref="Proj4Net.Registry"/> used by this factory.
        ///</summary>
        public Registry Registry
        {
            get { return _registry; }
        }

        ///<summary>
        /// Creates a <see cref="CoordinateReferenceSystem"/> (CRS) from a well-known name.
        /// CRS names are of the form: "<tt>authority:code</tt>", 
        /// with the components being: 
        /// <list type="Bullet">
        /// <item>The <b><tt>authority</tt></b> is a code for a namespace supported by PROJ.4. 
        /// Currently supported values are 
        /// <list type="Bullet">
        /// <item>EPSG</item>
        /// <item>ESRI</item>
        /// <item>WORLD</item>
        /// <item>NAD83</item>
        /// <item>NAD27</item></list>
        /// If no authority is provided, <c>EPSG</c> namespace be assumed.</item>
        /// <item>
        /// The ><b><tt>code</tt></b> is the id of a coordinate system in the authority namespace. 
        /// For example, in the <c>EPSG</c> namespace a code is an integer value 
        /// which identifies a CRS definition in the EPSG database..
        /// (Codes are read and handled as strings).
        /// </item>
        /// </list>
        /// An example of a valid CRS name is <tt>EPSG:3005</tt>.
        ///</summary>
        /// <param name="name">the name of a coordinate system, with optional authority prefix</param>
        /// <returns>The <see cref="CoordinateReferenceSystem"/> corresponding to the given name</returns>
        /// <exception cref="UnsupportedParameterException">if a PROJ.4 parameter is not supported</exception>
        /// <exception cref="InvalidValueException">if a parameter value is invalid</exception>
        /// <exception cref="UnknownAuthorityCodeException">if the authority code cannot be found</exception>
        public CoordinateReferenceSystem CreateFromName(String name)
        //throws UnsupportedParameterException, InvalidValueException, UnknownAuthorityCodeException
        {
            String[] parameters = CRSReader.GetParameters(name);
            if (parameters == null)
                throw new UnknownAuthorityCodeException(name);
            return CreateFromParameters(name, parameters);
        }

        ///<summary>
        /// Creates a <see cref="CoordinateReferenceSystem"/>    
        /// from a PROJ.4 projection parameter string.
        /// <para/>
        /// An example of a valid PROJ.4 parameter string is:
        /// <pre>
        /// +proj=aea +lat_1=50 +lat_2=58.5 +lat_0=45 +lon_0=-126 +x_0=1000000 +y_0=0 +ellps=GRS80 +units=m
        /// </pre>
        ///</summary>
        /// <param name="name">a name for this coordinate system (may be <c>null</c>) for an anonymous coordinate system)</param>
        /// <param name="paramStr">a PROJ.4 projection parameter string</param>
        /// <returns>The specified <see cref="CoordinateReferenceSystem"/></returns>
        /// <exception cref="UnsupportedParameterException">if a given PROJ.4 parameter is not supported</exception>
        /// <exception cref="InvalidValueException">if a supplied parameter value is invalid</exception>
        public CoordinateReferenceSystem CreateFromParameters(String name, String paramStr)
        {
            return CreateFromParameters(name, SplitParameters(paramStr));
        }

        ///<summary>
        /// Creates a <see cref="CoordinateReferenceSystem"/>
        /// defined by an array of PROJ.4 projection parameters.
        /// PROJ.4 parameters are generally of the form
        /// "<tt>+name=value</tt>".
        /// </summary>
        /// <param name="name">a name for this coordinate system (may be null)</param>
        /// <param name="parameters">an array of PROJ.4 parameters</param>
        /// <returns>a <see cref="CoordinateReferenceSystem"/></returns>
        /// <exception cref="UnsupportedParameterException">if a PROJ.4 parameter is not supported</exception>
        /// <exception cref="InvalidValueException">if a parameter value is invalid</exception>
        public CoordinateReferenceSystem CreateFromParameters(String name, String[] parameters)
        {
            if (parameters == null)
                return null;

            var parser = new Proj4Parser(Registry);
            return parser.Parse(name, parameters);
        }

        private static String[] SplitParameters(String paramStr)
        {
            var regex = new Regex("\\s+" 
#if !SILVERLIGHT
            , RegexOptions.Compiled);
#else
                );
#endif
            String[] parameters = regex.Split(paramStr);
            return parameters;
        }
    }
}
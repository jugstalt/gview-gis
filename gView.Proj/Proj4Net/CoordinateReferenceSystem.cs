using System;
using System.Text;
using Proj4Net.Datum;
using Proj4Net.Projection;
using Proj4Net.Units;

namespace Proj4Net
{
    /// <summary>
    /// Represents a projected or geodetic geospatial coordinate system,
    /// to which coordinates may be referenced.
    /// A coordinate system is defined by the following things:
    /// <list type="Bullet">
    /// <item>an <see cref="Ellipsoid"/> specifies how the shape of the Earth is approximated</item>
    /// <item>a <see cref="Datum"/> provides the mapping from the ellipsoid to 
    /// actual locations on the earth</item>
    /// <item>a <see cref="Projection"/> method maps the ellpsoidal surface to a planar space.
    /// (The projection method may be null in the case of geodetic coordinate systems).</item>
    /// <item>a <see cref="Unit"/> indicates how the ordinate values 
    /// of coordinates are interpreted</item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="CoordinateReferenceSystemFactory"/>
    public class CoordinateReferenceSystem
    {
        // allows specifying transformations which convert to/from Geographic coordinates on the same datum
        public static readonly CoordinateReferenceSystem CS_GEO = new CoordinateReferenceSystem("CS_GEO", null, null, null);

        //TODO: add metadata like authority, id, _name, parameter string, datum, ellipsoid, datum shift parameters

        private readonly String _name;
        private readonly String[] _parameters;
        private readonly Datum.Datum _datum;
        private readonly Projection.Projection _proj;

        public CoordinateReferenceSystem(String name, String[] parameters, Datum.Datum datum, Projection.Projection proj)
        {
            _name = name;
            _parameters = parameters;
            _datum = datum;
            _proj = proj;

            if (name == null)
            {
                String projName = "null-proj";
                if (proj != null)
                    projName = proj.Name;
                _name = projName + "-CS";
            }
        }

        public String Name
        {
            get {return _name;}
        }

        public String[] Parameters
        {
            get { return _parameters; }
        }

        public Datum.Datum Datum
        {
            get { return _datum; }
        }

        public Projection.Projection Projection
        {
            get { return _proj; }
        }

        public String GetParameterString()
        {
            if (_parameters == null) return "";

            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < _parameters.Length; i++)
            {
                buf.Append(_parameters[i]);
                buf.Append(" ");
            }
            return buf.ToString();
        }

        /// <summary>
        /// Creates a geographic (unprojected) <see cref="CoordinateReferenceSystem"/> 
        /// based on the <see cref="Datum"/> of this CRS.
        /// This is useful for defining <see cref="ICoordinateTransform"/>s
        /// to and from geographic coordinate systems,
        /// where no datum transformation is required.
        /// The <see cref="Units"/> of the geographic CRS are set to <see cref="Units.Degrees"/>
        /// </summary>
        /// <returns>A geographic <see cref="CoordinateReferenceSystem"/> based on the datum of this CRS</returns>
        public CoordinateReferenceSystem CreateGeographic()
        {
            var datum = Datum;
            var geoProj = new LongLatProjection
                              {
                                    Ellipsoid = Projection.Ellipsoid, 
                                    Unit = Units.Units.Degrees
                              };
            geoProj.Initialize();
            return new CoordinateReferenceSystem("GEO-" + datum.Code, null, datum, geoProj);
        }

        public override String ToString() { return _name; }
    }
}

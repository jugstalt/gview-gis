using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    internal class IndentedTokenWriter : IndentedTextWriter
    {
        private bool _start = true, _nl = false;
        private char _last = ',', _first = '\0';

        public IndentedTokenWriter(TextWriter writer)
            : base(writer)
        {
        }
        public IndentedTokenWriter(TextWriter writer, string tabString)
            : base(writer, tabString)
        {
        }

        public override void Write(string s)
        {
            if (_nl) base.WriteLine();
            _nl = false;
            base.Write(s);
        }
        public override void WriteLine(string s)
        {
            if (s != String.Empty)
                _first = s[0];

            if (!_start)
            {
                if (_last != ',' && _last != '[' &&
                    _first != ',' && _first != ']')
                {
                    base.WriteLine(",");
                    _last = ',';
                }
                else
                {
                    base.WriteLine();
                }
            }
            else
            {
                _start = false;
            }

            if (s != String.Empty)
                _last = s[s.Length - 1];

            base.Write(s);
            _nl = true;
        }
    }

    internal class WktCoordinateSystemWriter
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        
        public static string Write(object obj)
        {
            return Write(obj, false);   
        }
        internal static string Write(object obj, bool esri)
        {
            if (obj == null) return "";

            TextWriter textwriter = new StringWriter();
            IndentedTextWriter indentedWriter = new IndentedTokenWriter(textwriter);
            Write(obj, esri, indentedWriter);
            return textwriter.ToString();
        }
        private static void Write(object obj, bool esri, IndentedTextWriter writer)
        {
            if (obj is CoordinateSystem)
                WriteCoordinateSystem(obj as CoordinateSystem, esri, writer);
            else if (obj is Datum)
                WriteDatum(obj as Datum, esri, writer);
            else if (obj is Ellipsoid)
                WriteEllipsoid(obj as Ellipsoid, esri, writer);
            else if (obj is AxisInfo)
            {
                AxisInfo info = (AxisInfo)obj;
                WriteAxis(info, esri, writer);
            }
            else if (obj is WGS84ConversionInfo)
            {
                WGS84ConversionInfo info = (WGS84ConversionInfo)obj;
                WriteWGS84ConversionInfo(info, esri, writer);
            }
            else if (obj is Unit)
                WriteUnit(obj as Unit, esri, writer);
            else if (obj is PrimeMeridian)
                WritePrimeMeridian(obj as PrimeMeridian, esri, writer);
            else throw new NotImplementedException(String.Format("Cannot convert {0} to WKT.", obj.GetType().FullName));
        }

        private static void WriteCoordinateSystem(CoordinateSystem coordinateSystem, bool esri, IndentedTextWriter writer)
        {
            if (coordinateSystem is CompoundCoordinateSystem)
                WriteCompoundCoordinateSystem(coordinateSystem as CompoundCoordinateSystem, esri, writer);
            else if (coordinateSystem is GeographicCoordinateSystem)
                WriteGeographicCoordinateSystem(coordinateSystem as GeographicCoordinateSystem, esri, writer);
            else if (coordinateSystem is ProjectedCoordinateSystem)
                WriteProjectedCoordinateSystem(coordinateSystem as ProjectedCoordinateSystem, esri, writer);
            else if (coordinateSystem is LocalCoordinateSystem)
                WriteLocalCoordinateSystem(coordinateSystem as LocalCoordinateSystem, esri, writer);
            else if (coordinateSystem is FittedCoordinateSystem)
                WriteFittedCoordinateSystem(coordinateSystem as FittedCoordinateSystem, esri, writer);
            else if (coordinateSystem is GeocentricCoordinateSystem)
                WriteGeocentricCoordinateSystem(coordinateSystem as GeocentricCoordinateSystem, esri, writer);
            else if (coordinateSystem is VerticalCoordinateSystem)
                WriteVerticalCoordinateSystem(coordinateSystem as VerticalCoordinateSystem, esri, writer);
            else if (coordinateSystem is HorizontalCoordinateSystem)
                WriteHorizontalCoordinateSystem(coordinateSystem as HorizontalCoordinateSystem, esri, writer);
            else throw new InvalidOperationException("this coordinate system is recongized");
        }

        private static void WriteUnit(Unit unit, bool esri, IndentedTextWriter writer)
        {
            if (unit is AngularUnit)
                WriteAngularUnit(unit as AngularUnit,esri, writer);
            else if (unit is LinearUnit)
                WriteLinearUnit(unit as LinearUnit,esri, writer);
            else throw new InvalidOperationException("this unit is not recognized");
        }

        private static void WriteAuthority(AbstractInformation authority, bool esri, IndentedTextWriter writer)
        {
            if (esri) return;

            if (authority == null ||
                (authority.AuthorityCode == String.Empty && authority.Authority == String.Empty)) return;
            writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", authority.Authority, authority.AuthorityCode));
            
        }

        private static void WriteAngularUnit(AngularUnit angularUnit, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("UNIT[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",{1}", angularUnit.Name, angularUnit.RadiansPerUnit.ToString(_nhi)));
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", angularUnit.Authority, angularUnit.AuthorityCode));
            WriteAuthority(angularUnit,esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteCompoundCoordinateSystem(CompoundCoordinateSystem compoundCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("COMPD_CS[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",", compoundCoordinateSystem.Name));
            WriteCoordinateSystem(compoundCoordinateSystem.HeadCS,esri, writer);
            writer.WriteLine(",");
            WriteCoordinateSystem(compoundCoordinateSystem.TailCS,esri, writer);
            writer.WriteLine(",");
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", compoundCoordinateSystem.Authority, compoundCoordinateSystem.AuthorityCode));
            WriteAuthority(compoundCoordinateSystem,esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteGeographicCoordinateSystem(GeographicCoordinateSystem geographicCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("GEOGCS[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",", geographicCoordinateSystem.Name));
            WriteHorizontalDatum(geographicCoordinateSystem.HorizontalDatum,esri, writer);
            WritePrimeMeridian(geographicCoordinateSystem.PrimeMeridian,esri, writer);
            WriteAngularUnit(geographicCoordinateSystem.AngularUnit,esri, writer);
            
            for (int dimension = 0; dimension < geographicCoordinateSystem.Dimension; dimension++)
                WriteAxis(geographicCoordinateSystem.GetAxis(dimension), esri, writer);
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", geographicCoordinateSystem.Authority, geographicCoordinateSystem.AuthorityCode));
            WriteAuthority(geographicCoordinateSystem,esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.Write("]");
        }

        private static void WriteProjectedCoordinateSystem(ProjectedCoordinateSystem projectedCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("PROJCS[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",", projectedCoordinateSystem.Name));
            WriteGeographicCoordinateSystem(projectedCoordinateSystem.GeographicCoordinateSystem, esri,writer);
            //writer.WriteLine(",");
            WriteProjection(projectedCoordinateSystem.Projection, writer);
            WriteLinearUnit(projectedCoordinateSystem.LinearUnit,esri, writer);
            for (int dimension = 0; dimension < projectedCoordinateSystem.Dimension; dimension++)
                WriteAxis(projectedCoordinateSystem.GetAxis(dimension), esri, writer);
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", projectedCoordinateSystem.Authority, projectedCoordinateSystem.AuthorityCode));
            WriteAuthority(projectedCoordinateSystem,esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteDatum(Datum datum, bool esri, IndentedTextWriter writer)
        {
            if (datum is VerticalDatum)
                WriteVerticalDatum(datum as VerticalDatum, esri,writer);
            else if (datum is HorizontalDatum)
                WriteHorizontalDatum(datum as HorizontalDatum, esri,writer);
            else throw new NotImplementedException("This datum is not supported.");
        }

        private static void WriteHorizontalDatum(HorizontalDatum horizontalDatum, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("DATUM[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",", horizontalDatum.Name));
            WriteEllipsoid(horizontalDatum.Ellipsoid, esri, writer);
            WriteWGS84ConversionInfo(horizontalDatum.WGS84Parameters,esri, writer);
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", horizontalDatum.Authority, horizontalDatum.AuthorityCode));
            WriteAuthority(horizontalDatum, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteEllipsoid(Ellipsoid ellipsoid, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("SPHEROID[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",{1},{2}", ellipsoid.Name, ellipsoid.SemiMajorAxis.ToString(_nhi), ellipsoid.InverseFlattening.ToString(_nhi)));
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", ellipsoid.Authority, ellipsoid.AuthorityCode));
            WriteAuthority(ellipsoid, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteAxis(AxisInfo axis, bool esri, IndentedTextWriter writer)
        {
            if (esri) return;

            if (axis == null) return;

            string axisOrientation = String.Empty;
            switch (axis.Orientation)
            {
                case AxisOrientation.Down:
                    axisOrientation = "DOWN";
                    break;
                case AxisOrientation.East:
                    axisOrientation = "EAST";
                    break;
                case AxisOrientation.North:
                    axisOrientation = "NORTH";
                    break;
                case AxisOrientation.Other:
                    axisOrientation = "OTHER";
                    break;
                case AxisOrientation.South:
                    axisOrientation = "SOUTH";
                    break;
                case AxisOrientation.Up:
                    axisOrientation = "UP";
                    break;
                case AxisOrientation.West:
                    axisOrientation = "WEST";
                    break;
                default:
                    throw new InvalidOperationException("This should not exist");
            }
            writer.WriteLine(String.Format("AXIS[\"{0}\",\"{1}\"]", axis.Name, axisOrientation));
        }

        private static void WriteWGS84ConversionInfo(WGS84ConversionInfo conversionInfo, bool esri, IndentedTextWriter writer)
        {
            if (esri) return;

            writer.WriteLine(String.Format("TOWGS84[{0},{1},{2},{3},{4},{5},{6}]",
                    conversionInfo.Dx.ToString(_nhi), conversionInfo.Dy.ToString(_nhi), conversionInfo.Dz.ToString(_nhi),
                    conversionInfo.Ex.ToString(_nhi), conversionInfo.Ey.ToString(_nhi), conversionInfo.Ez.ToString(_nhi),
                    conversionInfo.Ppm.ToString(_nhi)));
        }

        private static void WriteLinearUnit(LinearUnit linearUnit, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("UNIT[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",{1}", linearUnit.Name, linearUnit.MetersPerUnit.ToString(_nhi)));
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", linearUnit.Authority, linearUnit.AuthorityCode));
            WriteAuthority(linearUnit, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WritePrimeMeridian(PrimeMeridian primeMeridian, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("PRIMEM[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",{1}", primeMeridian.Name, primeMeridian.Longitude.ToString(_nhi)));
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", primeMeridian.Authority, primeMeridian.AuthorityCode));
            WriteAuthority(primeMeridian, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteProjection(Projection projection, IndentedTextWriter writer)
        {
            writer.WriteLine(String.Format("PROJECTION[\"{0}\"]", projection.Name));
            for (int i = 0; i < projection.NumParameters; i++)
            {
                string paramName = projection.GetParameter(i).Name;
                double paramValue = projection.GetParameter(i).Value;
                writer.WriteLine(String.Format("PARAMETER[\"{0}\",{1}]", paramName, paramValue.ToString(_nhi)));
            }
        }

        private static void WriteVerticalCoordinateSystem(VerticalCoordinateSystem verticalCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("VERT_CS[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",", verticalCoordinateSystem.Name));
            WriteDatum(verticalCoordinateSystem.VerticalDatum, esri, writer);
            WriteUnit(verticalCoordinateSystem.VerticalUnit, esri, writer);
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", verticalCoordinateSystem.Authority, verticalCoordinateSystem.AuthorityCode));
            WriteAuthority(verticalCoordinateSystem, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        private static void WriteVerticalDatum(VerticalDatum verticalDatum, bool esri, IndentedTextWriter writer)
        {
            writer.WriteLine("VERT_DATUM[");
            writer.Indent = writer.Indent + 1;
            writer.WriteLine(String.Format("\"{0}\",{1},", verticalDatum.Name, DatumTypeAsCode(verticalDatum.DatumType)));
            //writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", verticalDatum.Authority, verticalDatum.AuthorityCode));
            WriteAuthority(verticalDatum, esri, writer);
            writer.Indent = writer.Indent - 1;
            writer.WriteLine("]");
        }

        public static string DatumTypeAsCode(DatumType datumtype)
        {
            string datumCode = Enum.Format(typeof(DatumType), datumtype, "d");
            return datumCode;
        }

        public static void WriteFittedCoordinateSystem(FittedCoordinateSystem fiitedCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }

        public static void WriteGeocentricCoordinateSystem(GeocentricCoordinateSystem geocentricCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }

        public static void WriteHorizontalCoordinateSystem(HorizontalCoordinateSystem horizontalCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }

        private static void WriteLocalCoordinateSystem(LocalCoordinateSystem localCoordinateSystem, bool esri, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }

        private static void WriteLocalDatum(LocalDatum localDatum, bool esri, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    internal class ESRIWktCoordinateSystemWriter
    {
        public static string Write(object obj)
        {
            string ret = WktCoordinateSystemWriter.Write(obj, true);
            if (ret == String.Empty) return String.Empty;

            StringBuilder sb = new StringBuilder();
            StringReader reader = new StringReader(ret);
            string l;
            while ((l = reader.ReadLine()) != null)
            {
                sb.Append(l.Trim());
            }

            return sb.ToString();
        }
    }

    internal class ESRIGeotransWktCoordinateWriter
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public static string Write(object obj)
        {
            GeographicCoordinateSystem geocoordsys = null;
            WGS84ConversionInfo wgsinfo;
            if (obj is ProjectedCoordinateSystem)
            {
                geocoordsys = ((ProjectedCoordinateSystem)obj).GeographicCoordinateSystem;

            }
            else if (obj is GeographicCoordinateSystem)
            {
                geocoordsys = obj as GeographicCoordinateSystem;
            }
            if (geocoordsys == null) return String.Empty;

            wgsinfo = geocoordsys.HorizontalDatum.WGS84Parameters;

            string GEOGCS = ESRIWktCoordinateSystemWriter.Write(geocoordsys);

            StringBuilder sb = new StringBuilder(@"GEOGTRAN[""");
            sb.Append(geocoordsys.Name + "_");
            sb.Append(@"To_WGS_1984"",");
            sb.Append(GEOGCS + ",");

            sb.Append(@"GEOGCS[""GCS_WGS_1984"",");
            sb.Append(@"DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137.0,298.257223563]],");
            sb.Append(@"PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]],");

            sb.Append(@"METHOD[""Position_Vector""],");
            sb.Append(@"PARAMETER[""X_Axis_Translation""," + wgsinfo.Dx.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""Y_Axis_Translation""," + wgsinfo.Dy.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""Z_Axis_Translation""," + wgsinfo.Dz.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""X_Axis_Rotation""," + wgsinfo.Ex.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""Y_Axis_Rotation""," + wgsinfo.Ey.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""Z_Axis_Rotation""," + wgsinfo.Ez.ToString(_nhi) + "],");
            sb.Append(@"PARAMETER[""Scale_Difference""," + wgsinfo.Ppm.ToString(_nhi) + "]]");

            return sb.ToString();
        }
    }
}

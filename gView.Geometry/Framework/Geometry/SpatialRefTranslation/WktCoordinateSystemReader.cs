using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using System.IO;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    public class WktCoordinateSystemReader
    {
        public static object Create(string wkt)
        {
            object returnObject = null;
            bool includeAuthority = (wkt.ToLower().IndexOf("authority") != -1);
            StringReader reader = new StringReader(wkt);
            WktStreamTokenizer tokenizer = new WktStreamTokenizer(reader);
            tokenizer.NextToken();
            string objectName = tokenizer.GetStringValue();
            switch (objectName)
            {
                case "UNIT":
                    Unit unit = ReadUnit(tokenizer, includeAuthority);
                    returnObject = unit;
                    break;
                case "VERT_DATUM":
                    VerticalDatum verticalDatum = ReadVerticalDatum(tokenizer, includeAuthority);
                    returnObject = verticalDatum;
                    break;
                case "SPHEROID":
                    Ellipsoid ellipsoid = ReadEllipsoid(tokenizer, includeAuthority);
                    returnObject = ellipsoid;
                    break;
                case "TOWGS84":
                    WGS84ConversionInfo wgsInfo = ReadWGS84ConversionInfo(tokenizer, includeAuthority);
                    returnObject = wgsInfo;
                    break;
                case "DATUM":
                    HorizontalDatum horizontalDatum = ReadHorizontalDatum(tokenizer, includeAuthority);
                    returnObject = horizontalDatum;
                    break;
                case "PRIMEM":
                    PrimeMeridian primeMeridian = ReadPrimeMeridian(tokenizer, includeAuthority);
                    returnObject = primeMeridian;
                    break;
                case "VERT_CS":
                    VerticalCoordinateSystem verticalCS = ReadVerticalCoordinateSystem(tokenizer, includeAuthority);
                    returnObject = verticalCS;
                    break;
                case "GEOGCS":
                    GeographicCoordinateSystem geographicCS = ReadGeographicCoordinateSystem(tokenizer, includeAuthority);
                    returnObject = geographicCS;
                    break;
                case "PROJCS":
                    ProjectedCoordinateSystem projectedCS = ReadProjectedCoordinateSystem(tokenizer, includeAuthority);
                    returnObject = projectedCS;
                    break;
                case "COMPD_CS":
                    CompoundCoordinateSystem compoundCS = ReadCompoundCoordinateSystem(tokenizer, includeAuthority);
                    returnObject = compoundCS;
                    break;
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    throw new NotSupportedException(String.Format("{0} is not implemented.", objectName));
                default:
                    throw new ParseException(String.Format("'{0'} is not recongnized.", objectName));

            }
            reader.Close();
            return returnObject;
        }

        private static Unit ReadUnit(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //UNIT["degree",0.01745329251994433,AUTHORITY["EPSG","9102"]]
            Unit unit = null;
            tokenizer.ReadToken("[");
            string unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double unitsPerUnit = tokenizer.GetNumericValue();
            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            tokenizer.ReadToken("]");
            switch (unitName.ToUpper())
            {
                // take into account the different spellings of the word meter/metre.
                case "METER":
                case "METRE":
                case "KILOMETRE":
                    case "KILOMETER":
                    unit = new LinearUnit(unitsPerUnit, String.Empty, authority, authorityCode, unitName, String.Empty, String.Empty);
                    break;
                case "DEGREE":
                case "RADIAN":
                case "GRAD":
                    unit = new AngularUnit(unitsPerUnit, String.Empty, authority, authorityCode, unitName, String.Empty, String.Empty);
                    break;
                //case "CLARKE'S LINK":
                //case "GOLD COAST FOOT":
                //case "US SURVEY FOOT":
                //case "CLARKE'S FOOT":
                //case "FOOT":
                //case "LINK":
                //case "INDIAN YARD":
                //case "GERMAN LEGAL METRE":
                //case "BRITISH CHAIN (SEARS 1922)":
                //case "BRITISH FOOT (SEARS 1922)":
                //case "BRITISH CHAIN (SEARS 1922 TRUNCATED)":
                //case "BRITISH CHAIN (BENOIT 1922 TRUNCATED)":
                //    unit = new LinearUnit(unitsPerUnit, String.Empty, authority, authorityCode, unitName, String.Empty, String.Empty);
                //    break;
                default:
                    string u = unitName.ToUpper();
                    if (u.Contains("YARD") || u.Contains("CHAIN") ||
                        u.Contains("FOOT") || u.Contains("LINK") ||
                        u.Contains("METRE") || u.Contains("METER") ||
                        u.Contains("KILOMETER") || u.Contains("KILOMETRE"))
                    {
                        unit = new LinearUnit(unitsPerUnit, String.Empty, authority, authorityCode, unitName, String.Empty, String.Empty);
                    }
                    else
                    {
                        throw new NotImplementedException(String.Format("{0} is not recognized a unit of measure.", unitName));
                    }
                    break;
            }
            return unit;
        }

        private static CoordinateSystem ReadCoordinateSystem(string coordinateSystem, WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            CoordinateSystem returnCS = null;
            switch (coordinateSystem)
            {
                case "VERT_CS":
                    VerticalCoordinateSystem verticalCS = ReadVerticalCoordinateSystem(tokenizer, includeAuthority);
                    returnCS = verticalCS;
                    break;
                case "GEOGCS":
                    GeographicCoordinateSystem geographicCS = ReadGeographicCoordinateSystem(tokenizer, includeAuthority);
                    returnCS = geographicCS;
                    break;
                case "PROJCS":
                    ProjectedCoordinateSystem projectedCS = ReadProjectedCoordinateSystem(tokenizer, includeAuthority);
                    returnCS = projectedCS;
                    break;
                case "COMPD_CS":
                    CompoundCoordinateSystem compoundCS = ReadCompoundCoordinateSystem(tokenizer, includeAuthority);
                    returnCS = compoundCS;
                    break;
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    throw new InvalidOperationException(String.Format("{0} coordinate system is not recongized.", coordinateSystem));
            }
            return returnCS;
        }

        private static WGS84ConversionInfo ReadWGS84ConversionInfo(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //TOWGS84[0,0,0,0,0,0,0]
            tokenizer.ReadToken("[");
            WGS84ConversionInfo info = new WGS84ConversionInfo();
            tokenizer.NextToken();
            info.Dx = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Dy = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Dz = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Ex = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Ey = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Ez = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");

            tokenizer.NextToken();
            info.Ppm = tokenizer.GetNumericValue();

            tokenizer.ReadToken("]");

            info.IsInUse = true;
            return info;
        }

        private static CompoundCoordinateSystem ReadCompoundCoordinateSystem(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            
            //COMPD_CS[
            //"OSGB36 / British National Grid + ODN",
            //PROJCS[]
            //VERT_CS[]
            //AUTHORITY["EPSG","7405"]
            //]

            //TODO add a ReadCoordinateSystem - that determines the correct coordinate system to 
            //read. Right now this hard coded for a projected and a vertical coord sys - so the UK
            //national grid works.
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string headCSCode = tokenizer.GetStringValue();
            CoordinateSystem headCS = ReadCoordinateSystem(headCSCode, tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string tailCSCode = tokenizer.GetStringValue();
            CoordinateSystem tailCS = ReadCoordinateSystem(tailCSCode, tokenizer, includeAuthority);

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            tokenizer.ReadToken("]");
            CompoundCoordinateSystem compoundCS = new CompoundCoordinateSystem(headCS, tailCS, String.Empty, authority, authorityCode, name, String.Empty, String.Empty);
            return compoundCS;

        }

        private static Ellipsoid ReadEllipsoid(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double majorAxis = tokenizer.GetNumericValue();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double e = tokenizer.GetNumericValue();

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            tokenizer.ReadToken("]");
            Ellipsoid ellipsoid = new Ellipsoid(majorAxis, 0.0, e, true, LinearUnit.Meters, String.Empty, authority, authorityCode, name, String.Empty, String.Empty);
            return ellipsoid;
        }

        private static Projection ReadProjection(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //tokenizer.NextToken();// PROJECTION
            tokenizer.ReadToken("PROJECTION");
            tokenizer.ReadToken("[");//[
            string projectionName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]");//]
            tokenizer.ReadToken(",");//,
            tokenizer.ReadToken("PARAMETER");
            ParameterList paramList = new ParameterList();
            while (tokenizer.GetStringValue() == "PARAMETER")
            {
                tokenizer.ReadToken("[");
                string paramName = tokenizer.ReadDoubleQuotedWord();
                //added by monoGIS team: parameters names may be capitalized, but the code works only with lower-case!
                paramName = paramName.ToLower();
                tokenizer.ReadToken(",");
                tokenizer.NextToken();
                double paramValue = tokenizer.GetNumericValue();
                tokenizer.ReadToken("]");
                paramList.Add(paramName, paramValue);

                if (!tokenizer.TryReadToken(",")) break;
                if (!tokenizer.TryReadToken("PARAMETER")) break;
            }

            ProjectionParameter[] paramArray = new ProjectionParameter[paramList.Count];
            int i = 0;
            foreach (string key in paramList.Keys)
            {
                ProjectionParameter param = new ProjectionParameter();
                param.Name = key;
                param.Value = (double)paramList[key];
                paramArray[i] = param;
                i++;
            }
            string authority = String.Empty;
            string authorityCode = String.Empty;
            Projection projection = new Projection(projectionName, paramArray, String.Empty, String.Empty, authority, authorityCode);
            return projection;
        }

        private static ProjectedCoordinateSystem ReadProjectedCoordinateSystem(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //PROJCS[
            //    "OSGB 1936 / British National Grid",
            //    GEOGCS[
            //        "OSGB 1936",
            //        DATUM[...]
            //        PRIMEM[...]
            //        AXIS["Geodetic latitude",NORTH]
            //        AXIS["Geodetic longitude",EAST]
            //        AUTHORITY["EPSG","4277"]
            //    ],
            //    PROJECTION["Transverse Mercator"],
            //    PARAMETER["latitude_of_natural_origin",49],
            //    PARAMETER["longitude_of_natural_origin",-2],
            //    PARAMETER["scale_factor_at_natural_origin",0.999601272],
            //    PARAMETER["false_easting",400000],
            //    PARAMETER["false_northing",-100000],
            //    AXIS["Easting",EAST],
            //    AXIS["Northing",NORTH],
            //    AUTHORITY["EPSG","27700"]
            //]

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("GEOGCS");
            GeographicCoordinateSystem geographicCS = ReadGeographicCoordinateSystem(tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            Projection projection = ReadProjection(tokenizer, includeAuthority);
            tokenizer.ReadToken("UNIT");
            Unit unit = ReadUnit(tokenizer, includeAuthority);

            AxisInfo axisInfo1 = null, axisInfo2 = null;
            if (tokenizer.TryReadToken(","))
            {
                if (tokenizer.TryReadToken("AXIS"))
                    axisInfo1 = ReadAxisInfo(tokenizer);
                if (tokenizer.TryReadToken(","))
                    if (tokenizer.TryReadToken("AXIS"))
                        axisInfo2 = ReadAxisInfo(tokenizer);
            }

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            tokenizer.ReadToken("]");

            int axisInfoDim = 0;
            if (axisInfo1 != null) axisInfoDim = 1;
            if (axisInfo2 != null) axisInfoDim = 2;
            AxisInfo[] axisArray = new AxisInfo[axisInfoDim];
            if (axisInfo1 != null) axisArray[0] = axisInfo1;
            if (axisInfo2 != null) axisArray[1] = axisInfo2;

            ProjectedCoordinateSystem projectedCS = new ProjectedCoordinateSystem(geographicCS.HorizontalDatum, axisArray, geographicCS, unit as LinearUnit, projection, String.Empty, authority, authorityCode, name, String.Empty, String.Empty);

            return projectedCS;
        }

        internal static GeographicCoordinateSystem ReadGeographicCoordinateSystem(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //GEOGCS["OSGB 1936",
            //DATUM["OSGB 1936",SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]
            //PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
            //UNIT["degree",0.01745329251994328,AUTHORITY["EPSG","9122"]]
            //AXIS["Geodetic latitude","NORTH"]
            //AXIS["Geodetic longitude","EAST"]
            //AUTHORITY["EPSG","4277"]
            //]

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("DATUM");
            HorizontalDatum horizontalDatum = ReadHorizontalDatum(tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("PRIMEM");
            PrimeMeridian primeMeridian = ReadPrimeMeridian(tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("UNIT");
            Unit angularUnit = ReadUnit(tokenizer, includeAuthority);

            AxisInfo axisInfo1 = null, axisInfo2 = null;
            if (tokenizer.TryReadToken(","))
            {
                if (tokenizer.TryReadToken("AXIS"))
                    axisInfo1 = ReadAxisInfo(tokenizer);
                if(tokenizer.TryReadToken(","))
                    if (tokenizer.TryReadToken("AXIS"))
                        axisInfo2 = ReadAxisInfo(tokenizer);
            }

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            tokenizer.ReadToken("]");

            GeographicCoordinateSystem geographicCS = new GeographicCoordinateSystem(angularUnit as AngularUnit, horizontalDatum,
                    primeMeridian, axisInfo1, axisInfo2, String.Empty, authority, authorityCode, name, String.Empty, String.Empty);
            return geographicCS;
        }

        private static AxisInfo ReadAxisInfo(WktStreamTokenizer tokenizer)
        {
            //AXIS["Geodetic longitude","EAST"]
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            string orientationString = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]");
            AxisOrientation orientation = (AxisOrientation)Enum.Parse(typeof(AxisOrientation), orientationString, true);
            AxisInfo axis = new AxisInfo(name, orientation);
            return axis;
        }

        private static HorizontalDatum ReadHorizontalDatum(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //DATUM["OSGB 1936",
            //       SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]],
            //       TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("SPHEROID");
            Ellipsoid ellipsoid = ReadEllipsoid(tokenizer, includeAuthority);
            string authority = String.Empty;
            string authorityCode = String.Empty;
            WGS84ConversionInfo wgsInfo = new WGS84ConversionInfo();
            wgsInfo.IsInUse = false;

            if (tokenizer.TryReadToken(","))
            {
                if (tokenizer.TryReadToken("TOWGS84"))
                {
                    wgsInfo = ReadWGS84ConversionInfo(tokenizer, includeAuthority);
                }
                if (includeAuthority)
                {
                    tokenizer.ReadToken(",");
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                }
            }
            tokenizer.ReadToken("]");
            // make an assumption about the datum type.
            DatumType datumType = DatumType.IHD_Geocentric;
            HorizontalDatum horizontalDatum = new HorizontalDatum(name, datumType, ellipsoid, wgsInfo, String.Empty, authority, authorityCode, String.Empty, String.Empty);

            return horizontalDatum;
        }

        private static PrimeMeridian ReadPrimeMeridian(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            double longitude = tokenizer.GetNumericValue();

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            // make an assumption about the Angular units - degrees.
            PrimeMeridian primeMeridian = new PrimeMeridian(name, new AngularUnit(180 / Math.PI), longitude, String.Empty, authority, authorityCode, String.Empty, String.Empty);
            tokenizer.ReadToken("]");
            return primeMeridian;
        }

        private static VerticalCoordinateSystem ReadVerticalCoordinateSystem(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //VERT_CS["Newlyn",
            //VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["EPSG","5101"]]
            //UNIT["metre",1,AUTHORITY["EPSG","9001"]]
            //AUTHORITY["EPSG","5701"]
            
            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("VERT_DATUM");
            VerticalDatum verticalDatum = ReadVerticalDatum(tokenizer, includeAuthority);
            tokenizer.ReadToken("UNIT");
            Unit unit = ReadUnit(tokenizer, includeAuthority);
            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            tokenizer.ReadToken("]");

            VerticalCoordinateSystem verticalCS = new VerticalCoordinateSystem(name, verticalDatum, String.Empty, authority, authorityCode, String.Empty, String.Empty);
            return verticalCS;
        }

        private static VerticalDatum ReadVerticalDatum(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            //VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["5101","EPSG"]]
            tokenizer.ReadToken("[");
            string datumName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.NextToken();
            string datumTypeNumber = tokenizer.GetStringValue();

            string authority = String.Empty;
            string authorityCode = String.Empty;
            if (includeAuthority)
            {
                tokenizer.ReadToken(",");
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
            }
            DatumType datumType = (DatumType)Enum.Parse(typeof(DatumType), datumTypeNumber);
            VerticalDatum verticalDatum = new VerticalDatum(datumType, String.Empty, authorityCode, authority, datumName, String.Empty, String.Empty);
            tokenizer.ReadToken("]");
            return verticalDatum;
        }
    }

    public class ESRIGeotransWktCoordinateReader
    {
        public static object Create(string wkt)
        {
            object returnObject = null;
            bool includeAuthority = (wkt.ToLower().IndexOf("authority") != -1);
            StringReader reader = new StringReader(wkt);
            WktStreamTokenizer tokenizer = new WktStreamTokenizer(reader);
            tokenizer.NextToken();
            string objectName = tokenizer.GetStringValue();
            switch (objectName)
            {
                case "GEOGTRAN":
                    Geotransformation geotrans = ReadGeotransformation(tokenizer, includeAuthority);
                    returnObject = geotrans; 
                    break;
                default:
                    throw new ParseException(String.Format("'{0'} is not recongnized.", objectName));
            }
            reader.Close();
            return returnObject;
        }

        private static Geotransformation ReadGeotransformation(WktStreamTokenizer tokenizer, bool includeAuthority)
        {
            // GEOGTRAN[
            //       "MGI_To_WGS_1984_2",
            //        GEOGCS[
            //            "GCS_MGI",
            //            DATUM["D_MGI",SPHEROID["Bessel_1841",6377397.155,299.1528128]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],GEOGCS["GCS_WGS_1984",DATUM["D_WGS_1984",SPHEROID["WGS_1984",6378137.0,298.257223563]],PRIMEM["Greenwich",0.0],UNIT["Degree",0.0174532925199433]],
            //            METHOD["Position_Vector"],
            //            PARAMETER["X_Axis_Translation",577.326],
            //            PARAMETER["Y_Axis_Translation",90.129],
            //            PARAMETER["Z_Axis_Translation",463.919],
            //            PARAMETER["X_Axis_Rotation",5.1365988],
            //            PARAMETER["Y_Axis_Rotation",1.4742],
            //            PARAMETER["Z_Axis_Rotation",5.2970436],
            //            PARAMETER["Scale_Difference",2.4232]
            //]

            tokenizer.ReadToken("[");
            string name = tokenizer.ReadDoubleQuotedWord();
            
            tokenizer.ReadToken(",");   
            tokenizer.ReadToken("GEOGCS");

            GeographicCoordinateSystem geographicCS1 = WktCoordinateSystemReader.ReadGeographicCoordinateSystem(tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("GEOGCS");

            GeographicCoordinateSystem geographicCS2 = WktCoordinateSystemReader.ReadGeographicCoordinateSystem(tokenizer, includeAuthority);
            tokenizer.ReadToken(",");
            string method = String.Empty;
            ParameterList paramList = new ParameterList();
            if (tokenizer.TryReadToken("METHOD"))
            {
                tokenizer.ReadToken("[");
                method = tokenizer.ReadDoubleQuotedWord();
                tokenizer.ReadToken("]");
            }
            if (tokenizer.TryReadToken(","))
            {
                tokenizer.ReadToken("PARAMETER");
                while (tokenizer.GetStringValue() == "PARAMETER")
                {
                    tokenizer.ReadToken("[");
                    string paramName = tokenizer.ReadDoubleQuotedWord();
                    paramName = paramName.ToLower();
                    tokenizer.ReadToken(",");
                    tokenizer.NextToken();
                    double paramValue = tokenizer.GetNumericValue();
                    tokenizer.ReadToken("]");
                    paramList.Add(paramName, paramValue);

                    if (!tokenizer.TryReadToken(",")) break;
                    if (!tokenizer.TryReadToken("PARAMETER")) break;
                }
            }
            return new Geotransformation(name, method, geographicCS1, geographicCS2, paramList, "", "", "", "");
        }
    }
}

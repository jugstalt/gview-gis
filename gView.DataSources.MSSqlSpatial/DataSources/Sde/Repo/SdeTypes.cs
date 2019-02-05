using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    class SdeTypes
    {
        public const System.Int32 SE_SMALLINT_TYPE = 1;   /* 2-byte Integer */
        public const System.Int32 SE_INTEGER_TYPE = 2;   /* 4-byte Integer */
        public const System.Int32 SE_FLOAT_TYPE = 3;   /* 4-byte Float */
        public const System.Int32 SE_DOUBLE_TYPE = 4;   /* 8-byte Float */
        public const System.Int32 SE_STRING_TYPE = 5;   /* Null Term. Character Array */
        public const System.Int32 SE_BLOB_TYPE = 6;   /* Variable Length Data */
        public const System.Int32 SE_DATE_TYPE = 7;   /* Struct tm Date */
        public const System.Int32 SE_SHAPE_TYPE = 8;   /* Shape geometry (SE_SHAPE) */
        public const System.Int32 SE_RASTER_TYPE = 9;   /* Raster */
        public const System.Int32 SE_XML_TYPE = 10;  /* XML Document */
        public const System.Int32 SE_INT64_TYPE = 11;  /* 8-byte Integer */
        public const System.Int32 SE_UUID_TYPE = 12;  /* A Universal Unique ID */
        public const System.Int32 SE_CLOB_TYPE = 13;  /* Character variable length data */
        public const System.Int32 SE_NSTRING_TYPE = 14;  /* UNICODE Null Term. Character Array */
        public const System.Int32 SE_NCLOB_TYPE = 15;  /* UNICODE Character Large Object */

        //http://webhelp.esri.com/arcgisdesktop/9.2/index.cfm?TopicName=System_tables_of_a_geodatabase_in_SQL_Server
        [Flags]
        public enum SdeFieldFlags
        {
            IsId = 1
        }

        public enum SdeGeometryTppe
        {
            unknown=0,
            point = 1,
            multipoint = -1,  // ??
            linestring = -2,  // ??
            multilinestring = 9,  
            polygon = -3,     // ??
            multipolygon = 11
        }

        static public FieldType FieldType(SdeColumn sdeColumn)
        {

            if (((SdeFieldFlags)sdeColumn.Flags).HasFlag(SdeFieldFlags.IsId))
                return Framework.Data.FieldType.ID;

            switch(sdeColumn.SdeType)
            {
                case SE_SMALLINT_TYPE:
                    return Framework.Data.FieldType.smallinteger;
                case SE_FLOAT_TYPE:
                    return Framework.Data.FieldType.Float;
                case SE_DOUBLE_TYPE:
                    return Framework.Data.FieldType.Double;
                case SE_STRING_TYPE:
                    return Framework.Data.FieldType.String;
                case SE_BLOB_TYPE:
                    return Framework.Data.FieldType.binary;
                case SE_DATE_TYPE:
                    return Framework.Data.FieldType.Date;
                case SE_SHAPE_TYPE:
                    return Framework.Data.FieldType.Shape;
                case SE_RASTER_TYPE:
                    return Framework.Data.FieldType.binary;
                case SE_XML_TYPE:
                    return Framework.Data.FieldType.String;
                case SE_INT64_TYPE:
                    return Framework.Data.FieldType.biginteger;
                case SE_UUID_TYPE:
                    return Framework.Data.FieldType.guid;
                case SE_CLOB_TYPE:
                    return Framework.Data.FieldType.binary;
                case SE_NSTRING_TYPE:
                    return Framework.Data.FieldType.NString;
                case SE_NCLOB_TYPE:
                    return Framework.Data.FieldType.binary;
                case SE_INTEGER_TYPE:
                    return Framework.Data.FieldType.integer;
            }

            return Framework.Data.FieldType.unknown;
        }
    }
}

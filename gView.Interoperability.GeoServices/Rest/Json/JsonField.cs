using gView.Framework.Core.Data;
using Newtonsoft.Json;
using System;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("domain")]
        public JsonDomain Domain { get; set; }

        public FieldType GetFieldType()
        {
            switch (Type)
            {
                case "esriFieldTypeOID":
                    return FieldType.ID;
                case "esriFieldTypeGeometry":
                    return FieldType.Shape;
                case "esriFieldTypeSmallInteger":
                    return FieldType.smallinteger;
                case "esriFieldTypeInteger":
                    return FieldType.integer;
                case "esriFieldTypeString":
                    return FieldType.String;
                case "esriFieldTypeSingle":
                    return FieldType.Float;
                case "esriFieldTypeDouble":
                    return FieldType.Double;
                case "esriFieldTypeDate":
                    return FieldType.Date;
                case "esriFieldTypeBlob":
                    return FieldType.binary;
                case "esriFieldTypeGUID":
                    return FieldType.guid;
            }

            return FieldType.String;
        }

        #region Static Members

        static public EsriFieldType ToType(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.ID:
                    return EsriFieldType.esriFieldTypeOID;
                case FieldType.Shape:
                    return EsriFieldType.esriFieldTypeGeometry;
                case FieldType.boolean:
                    return EsriFieldType.esriFieldTypeSmallInteger;
                case FieldType.biginteger:
                    return EsriFieldType.esriFieldTypeInteger;
                case FieldType.character:
                    return EsriFieldType.esriFieldTypeString;
                case FieldType.integer:
                    return EsriFieldType.esriFieldTypeInteger;
                case FieldType.smallinteger:
                    return EsriFieldType.esriFieldTypeSmallInteger;
                case FieldType.Float:
                    return EsriFieldType.esriFieldTypeSingle;
                case FieldType.Double:
                    return EsriFieldType.esriFieldTypeDouble;
                case FieldType.String:
                    return EsriFieldType.esriFieldTypeString;
                case FieldType.Date:
                    return EsriFieldType.esriFieldTypeDate;
                case FieldType.binary:
                    return EsriFieldType.esriFieldTypeBlob;
                case FieldType.guid:
                    return EsriFieldType.esriFieldTypeGUID;
                case FieldType.unknown:
                default:
                    return EsriFieldType.esriFieldTypeString;
            }
        }

        static public EsriFieldType ToType(Type type)
        {
            if (type != null)
            {
                if (type == typeof(short))
                {
                    return EsriFieldType.esriFieldTypeSmallInteger;
                }
                if (type == typeof(int) || type == typeof(long))
                {
                    return EsriFieldType.esriFieldTypeInteger;
                }
                if (type == typeof(float))
                {
                    return EsriFieldType.esriFieldTypeSingle;
                }
                if (type == typeof(double))
                {
                    return EsriFieldType.esriFieldTypeDouble;
                }
                if (type == typeof(DateTime))
                {
                    return EsriFieldType.esriFieldTypeDate;
                }
                if (type == typeof(byte[]))
                {
                    return EsriFieldType.esriFieldTypeBlob;
                }
                if (type == typeof(Guid))
                {
                    return EsriFieldType.esriFieldTypeGUID;
                }
            }

            return EsriFieldType.esriFieldTypeString;
        }

        #endregion
    }
}

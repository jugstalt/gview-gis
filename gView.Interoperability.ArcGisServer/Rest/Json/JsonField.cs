using gView.Framework.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
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

        #region Static Members

        static public EsriFieldType ToType(FieldType fieldType)
        {
            switch(fieldType)
            {
                case FieldType.ID:
                    return EsriFieldType.esriFieldTypeOID;
                case FieldType.Shape:
                    return EsriFieldType.esriFieldTypeGeometry;
                case FieldType.boolean:
                    return EsriFieldType.esriFieldTypeSmallInteger;
                case FieldType.biginteger:
                    return EsriFieldType.esriFieldTypeSmallInteger;
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

        #endregion
    }
}

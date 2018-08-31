using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.Dataset.GML
{
    internal class XmlSchemaReader
    {
        private XmlDocument _schema = null;
        private XmlNamespaceManager _ns = null;

        public XmlSchemaReader(XmlDocument schema)
        {
            if (schema == null) return;
            _schema = schema;
            _ns = new XmlNamespaceManager(_schema.NameTable);
            _ns.AddNamespace("W3", "http://www.w3.org/2001/XMLSchema");
        }

        public string[] ElementNames
        {
            get
            {
                if (_schema == null) return null;

                List<string> names = new List<string>();
                foreach (XmlNode element in _schema.SelectNodes("W3:schema/W3:element[@name]", _ns))
                {
                    names.Add(element.Attributes["name"].Value);
                }
                return names.ToArray();
            }
        }

        public Fields ElementFields(string elementName, out string shapeFieldname, out geometryType geomType)
        {
            shapeFieldname = "";
            geomType = geometryType.Unknown;

            if (_schema == null) return null;

            XmlNode elementNode = _schema.SelectSingleNode("W3:schema/W3:element[@name='" + elementName + "']",_ns);
            if (elementNode == null || elementNode.Attributes["type"] == null)
            {
                elementNode = _schema.SelectSingleNode("W3:schema/W3:element[@name='" + TypeWithoutPrefix(elementName) + "']", _ns);
                if (elementNode == null || elementNode.Attributes["type"] == null)
                    return null;
            }

            string type = TypeWithoutPrefix(elementNode.Attributes["type"].Value);
            XmlNode complexTypeNode = _schema.SelectSingleNode("W3:schema/W3:complexType[@name='" + type + "']", _ns);
            if (complexTypeNode == null) return null;

            Fields fields=new Fields();
            foreach (XmlNode eNode in complexTypeNode.SelectNodes("W3:complexContent/W3:extension/W3:sequence/W3:element", _ns))
            {
                string name = String.Empty;
                if (eNode.Attributes["name"] != null)
                {
                    name = eNode.Attributes["name"].Value;
                }
                
                FieldType fType = FieldType.String;
                int size = 8;
                XmlNode restrictionNode = eNode.SelectSingleNode("W3:simpleType/W3:restriction[@base]", _ns);

                if (restrictionNode != null)
                {
                    switch (restrictionNode.Attributes["base"].Value.ToLower())
                    {
                        case "string":
                            fType = FieldType.String;
                            XmlNode maxLengthNode = restrictionNode.SelectSingleNode("W3:maxLength[@value]", _ns);
                            if (maxLengthNode != null)
                                size = int.Parse(maxLengthNode.Attributes["value"].Value);
                            break;
                        case "decimal":
                            fType = FieldType.Double;
                            break;
                        case "integer":
                            fType = FieldType.integer;
                            break;
                    }
                }
                else if (eNode.Attributes["type"] != null)
                {
                    switch (TypeWithoutPrefix(eNode.Attributes["type"].Value.ToLower()))
                    {
                        case "string":
                            fType = FieldType.String;
                            break;
                        case "int":
                        case "integer":
                            fType = FieldType.integer;
                            break;
                        case "decimal":
                        case "double":
                            fType = FieldType.Double;
                            break;
                        case "geometrypropertytype":
                            shapeFieldname = name;
                            continue;
                        case "polygonproperty":
                        case "polygonpropertytype":
                        case "multipolygonproperty":
                        case "multipolygonpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Polygon;
                            continue;
                        case "linestringproperty":
                        case "linestringpropertytype":
                        case "multilinestringproperty":
                        case "multilinestringpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Polyline;
                            continue;
                        case "pointproperty":
                        case "pointpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Point;
                            continue;
                        case "multipointproperty":
                        case "multipointpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Multipoint;
                            continue;
                        case "featureidtype":
                        case "gmlobjectidtype":
                            fType=FieldType.ID;
                            break;
                        case "datetime":
                            fType = FieldType.Date;
                            break;
                        case "long":
                            fType = FieldType.biginteger;
                            break;
                        case "short":
                            fType = FieldType.smallinteger;
                            break;
                        default:
                            break;
                    }
                }
                else if (eNode.Attributes["ref"] != null)
                {
                    switch (TypeWithoutPrefix(eNode.Attributes["ref"].Value.ToLower()))
                    {
                        case "polygonproperty":
                        case "polygonpropertytype":
                        case "multipolygonproperty":
                        case "multipolygonpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Polygon;
                            continue;
                        case "linestringproperty":
                        case "linestringpropertytype":
                        case "multilinestringproperty":
                        case "multilinestringpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Polyline;
                            continue;
                        case "pointproperty":
                        case "pointpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Point;
                            continue;
                        case "multipointproperty":
                        case "multipointpropertytype":
                            shapeFieldname = name;
                            geomType = geometryType.Multipoint;
                            continue;
                    }
                }
                if(name!=String.Empty)
                    fields.Add(new Field(name, fType, size));
            }
            return fields;
        }

        public string TargetNamespaceURI
        {
            get
            {
                if (_schema == null) return String.Empty;

                XmlNode schemaNode = _schema.SelectSingleNode("W3:schema[@targetNamespace]", _ns);
                if (schemaNode == null) return String.Empty;

                return schemaNode.Attributes["targetNamespace"].Value;
            }
        }

        public string MyNamespaceName(string elementName)
        {
            if (_schema == null) return null;

            XmlNode elementNode = _schema.SelectSingleNode("W3:schema/W3:element[@name='" + elementName + "']", _ns);
            if (elementNode == null || elementNode.Attributes["type"] == null) return null;

            string myns = elementNode.Attributes["type"].Value.Split(':')[0];

            return myns;
        }

        private string TypeWithoutPrefix(string type)
        {
            return gView.Framework.OGC.XML.Globals.TypeWithoutPrefix(type);
        }
    }

    internal class XmlSchemaWriter
    {
        private List<FeatureClassSchema> _fcschemas = null;

        public XmlSchemaWriter(IFeatureClass fc)
        {
            if (fc == null) return;

            _fcschemas = new List<FeatureClassSchema>();
            _fcschemas.Add(new FeatureClassSchema(fc.Name, fc));
        }
        public XmlSchemaWriter(List<IFeatureClass> fcs)
        {
            if (fcs == null) return;

            _fcschemas = new List<FeatureClassSchema>();
            foreach (IFeatureClass fc in fcs)
            {
                if (fc == null) continue;
                _fcschemas.Add(new FeatureClassSchema(fc.Name, fc));
            }
        }
        public XmlSchemaWriter(FeatureClassSchema fcschema)
        {
            if (fcschema == null) return;

            _fcschemas = new List<FeatureClassSchema>();
            _fcschemas.Add(fcschema);
        }
        public XmlSchemaWriter(List<FeatureClassSchema> fcschemas)
        {
            _fcschemas = fcschemas;
        }

        public string Write()
        {
            if (_fcschemas == null) return "";

            StringBuilder sb = new StringBuilder();

            sb.Append(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
            sb.Append(@"<schema
   targetNamespace=""http://www.gViewGIS.com/server""
   xmlns:gv=""http://www.gViewGIS.com/server"" 
   xmlns:ogc=""http://www.opengis.net/ogc""
   xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
   xmlns=""http://www.w3.org/2001/XMLSchema""
   xmlns:gml=""http://www.opengis.net/gml""
   elementFormDefault=""qualified"" version=""0.1"" >

  <import namespace=""http://www.opengis.net/gml""
          schemaLocation=""http://schemas.opengeospatial.net/gml/2.1.2/feature.xsd"" />

   ");

            foreach (FeatureClassSchema fcschema in _fcschemas)
            {
                if (fcschema==null || fcschema.FeatureClass == null) continue;
                IFeatureClass fc = fcschema.FeatureClass;

                sb.Append(@"<element name=""" + fcschema.FeatureClassID + @""" 
           type=""gv:" + fcschema.FeatureClassID + @"Type"" 
           substitutionGroup=""gml:_Feature"" />
   ");

                sb.Append(@"<complexType name=""" + fcschema.FeatureClassID + @"Type"">
    <complexContent>
      <extension base=""gml:AbstractFeatureType"">
        <sequence>");

                // Fields
                string geomTypeName = "gml:GeometryPropertyType";
                switch (fc.GeometryType)
                {
                    case geometryType.Point:
                        geomTypeName = "gml:PointPropertyType";
                        break;
                    case geometryType.Multipoint:
                        geomTypeName = "gml:MultiPointPropertyType";
                        break;
                    case geometryType.Polyline:
                        geomTypeName = "gml:MultiLineStringPropertyType";
                        break;
                    case geometryType.Polygon:
                        geomTypeName = "gml:MultiPolygonPropertyType";
                        break;
                }
                sb.Append(@"<element name=""" + fc.ShapeFieldName.Replace("#", "") + @""" type=""" + geomTypeName + @""" minOccurs=""0"" maxOccurs=""1""/>");

                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    if (field.name == fc.ShapeFieldName) continue;

                    sb.Append(@"<element name=""" + field.name + @""" ");
                    switch (field.type)
                    {
                        case FieldType.String:
                            sb.Append(@">
            <simpleType>
            <restriction base=""string"">
            <maxLength value=""" + field.size + @"""/>
            </restriction>
            </simpleType></element>");
                            break;
                        case FieldType.Float:
                        case FieldType.Double:
                            sb.Append(@">
            <simpleType>
            <restriction base=""decimal"">
            <totalDigits value=""11""/>
            <fractionDigits value=""0""/>
            </restriction>
            </simpleType></element>");
                            break;
                        case FieldType.smallinteger:
                            sb.Append(@"type=""short""/>");
                            break;
                        case FieldType.biginteger:
                            sb.Append(@"type=""long""/>");
                            break;
                        case FieldType.integer:
                            sb.Append(@"type=""integer""/>");
                            break;
                            break;
                        case FieldType.Date:
                            sb.Append(@"type=""datetime""/>");
                            break;
                        case FieldType.ID:
                            sb.Append(@"type=""ogc:FeatureIdType""/>");
                            break;
                        default:
                            sb.Append(@"type=""string""/>");
                            break;
                    }
                }

                sb.Append(@"
        </sequence>
      </extension>
    </complexContent>
  </complexType>");
            }

            sb.Append(@"
</schema>");

            return sb.ToString();
        }

        #region HelperClasses
        public class FeatureClassSchema
        {
            private string _fcID;
            private IFeatureClass _fc;
            
            public FeatureClassSchema(string fcID, IFeatureClass fc)
            {
                _fcID = fcID;
                _fc = fc;
            }

            public string FeatureClassID
            {
                get { return _fcID; }
            }
            public IFeatureClass FeatureClass
            {
                get { return _fc; }
            }
        }
        #endregion
    }
}

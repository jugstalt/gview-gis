using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace gView.Framework.OGC.WFS
{
    public class DescribeFeatureHelper
    {
        XmlDocument _doc = null;
        XmlNamespaceManager _ns = null;

        public DescribeFeatureHelper(string xml)
        {
            try
            {
                _doc = new XmlDocument();
                _doc.LoadXml(xml);

                _ns = new XmlNamespaceManager(_doc.NameTable);
                _ns.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");

            }
            catch
            {
                _doc = null;
            }
        }

        public string TargetNamespace
        {
            get
            {
                if (_doc == null) return String.Empty;

                XmlNode schemaNode = _doc.SelectSingleNode("xsd:schema[@targetNamespace]", _ns);
                if (schemaNode == null) return String.Empty;

                return schemaNode.Attributes["targetNamespace"].Value;
            }
        }

        public Field[] TypeFields(string typeName)
        {
            List<Field> fields = new List<Field>();
            if (_doc != null)
            {
                XmlNode complexType = _doc.SelectSingleNode("xsd:schema/xsd:complexType[@name='" + typeName + "']", _ns);
                if (complexType == null) complexType = _doc.SelectSingleNode("xsd:schema/xsd:complexType[@name='" + typeName + "_Type']", _ns);
                if (complexType == null) complexType = _doc.SelectSingleNode("xsd:schema/xsd:complexType[@name='" + typeName + "Type']", _ns);

                if (complexType != null)
                {
                    foreach (XmlNode elementNode in complexType.SelectNodes("xsd:complexContent/xsd:extension/xsd:sequence/xsd:element", _ns))
                    {
                        if (elementNode.Attributes["name"] != null && elementNode.Attributes["type"] != null)
                            fields.Add(new Field(elementNode.Attributes["name"].Value, elementNode.Attributes["type"].Value.ToLower().StartsWith("gml:") ? "shape" : elementNode.Attributes["type"].Value.ToLower()));
                        else if (elementNode.Attributes["name"] != null)
                            fields.Add(new Field(elementNode.Attributes["name"].Value, "unknown"));
                        else if (elementNode.Attributes["ref"] != null)
                        {
                            if (elementNode.Attributes["ref"].Value.ToLower().StartsWith("gml"))
                                fields.Add(new Field(elementNode.Attributes["ref"].Value, "shape"));
                            else
                                fields.Add(new Field(elementNode.Attributes["ref"].Value, "unknown"));
                        }

                    }
                }
            }

            return fields.ToArray();
        }

        #region Classes
        public class Field
        {
            public string Name, Type;

            public Field(string name, string type)
            {
                Name = name;
                Type = type;
            }
        }
        #endregion
    }
}

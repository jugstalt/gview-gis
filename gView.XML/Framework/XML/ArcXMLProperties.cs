using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using System.Xml;

namespace gView.Framework.XML
{
    public class ArcXMLProperties : IPersistable
    {
        private XmlDocument _doc = new XmlDocument();
        public ArcXMLProperties()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<PROPERTIES>");
            
            sb.Append("<FEATURECOORDSYS");
            sb.Append(" id=\"\"");
            sb.Append(" string=\"\"");
            sb.Append(" datumtransformid=\"\"");
            sb.Append(" datumtransformstring=\"\"");
            sb.Append(" />");

            sb.Append("<FILTERCOORDSYS");
            sb.Append(" id=\"\"");
            sb.Append(" string=\"\"");
            sb.Append(" datumtransformid=\"\"");
            sb.Append(" datumtransformstring=\"\"");
            sb.Append(" />");

            sb.Append("</PROPERTIES>");
            _doc.LoadXml(sb.ToString());
        }

        public XmlDocument Properties
        {
            get { return _doc; }
        }

        public string PropertyString
        {
            get
            {
                if (_doc == null) return "";
                XmlNode propNode = _doc.SelectSingleNode("PROPERTIES");
                if (propNode == null) return "";

                StringBuilder sb = new StringBuilder();
                foreach (XmlNode node in propNode.ChildNodes)
                {
                    bool hasValues = false;
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Value.Trim() != String.Empty)
                        {
                            hasValues = true;
                            break;
                        }
                    }
                    if (hasValues)
                    {
                        sb.Append("<" + node.Name);
                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            if (attribute.Value.Trim() != String.Empty)
                            {
                                sb.Append(" " + attribute.Name + "=\"" + CheckEsc(attribute.Value) + "\"");
                            }
                        }
                        sb.Append(" />\n");
                    }
                }
                return sb.ToString();
            }
        }

        public void Write(XmlWriter xWriter,string tag)
        {
            XmlNodeList nodes = _doc.SelectNodes("PROPERTIES/" + tag);
            if (nodes.Count == 0) return;

            foreach (XmlNode node in nodes)
            {
                bool hasValues = false;
                foreach (XmlAttribute a in node.Attributes)
                {
                    if (a.Value.Trim() != String.Empty)
                    {
                        hasValues = true;
                        break;
                    }
                }
                if (hasValues)
                {
                    xWriter.WriteStartElement(node.Name);

                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Value.Trim() != String.Empty)
                        {
                            xWriter.WriteAttributeString(attribute.Name, attribute.Value);
                        }
                    }
                    xWriter.WriteEndElement();
                }
            }
        }

        private static string CheckEsc(string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            string outerXml = (string)stream.Load("xml", "");
            if (outerXml != "")
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(outerXml);

                    _doc = doc;
                }
                catch
                {
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("xml", _doc.OuterXml);
        }

        #endregion
    }

    //internal class PersistableXmlNode : IPersistable
    //{
    //    private XmlNode _node;
    //    public PersistableXmlNode(XmlNode node)
    //    {
    //        _node = node;
    //    }

    //    #region IPersistable Member

    //    public void Load(IPersistStream stream)
    //    {
            
    //    }

    //    public void Save(IPersistStream stream)
    //    {
    //        if (_node == null) return;


    //    }

    //    #endregion
    //}
}

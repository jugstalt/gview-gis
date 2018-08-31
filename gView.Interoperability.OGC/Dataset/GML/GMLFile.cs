using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.Geometry;
using gView.Framework.Data;
using System.Xml;
using gView.Framework.system;
using gView.Framework.OGC.GML;

namespace gView.Interoperability.OGC.Dataset.GML
{
    internal class GMLFile
    {
        private Dataset _gmlDataset = null;
        private string _filename = String.Empty, _errMsg = String.Empty;
        private XmlDocument _doc = null;
        private XmlNamespaceManager _ns = null;
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private GmlVersion _gmlVersion = GmlVersion.v1;

        public GMLFile(string filename)
        {
            try
            {
                _gmlDataset = new Dataset();
                _gmlDataset.ConnectionString = filename;
                if (!_gmlDataset.Open())
                    _gmlDataset = null;

                _filename = filename;

                _doc = new XmlDocument();
                _doc.Load(_filename);

                _ns = new XmlNamespaceManager(_doc.NameTable);
                _ns.AddNamespace("GML", "http://www.opengis.net/gml");
                _ns.AddNamespace("WFS", "http://www.opengis.net/wfs");
                _ns.AddNamespace("OGC", "http://www.opengis.net/ogc");
                _ns.AddNamespace("myns", _gmlDataset.targetNamespace);


            }
            catch
            {
                _doc = null;
            }
        }

        public bool Delete()
        {
            if (_gmlDataset != null)
                return _gmlDataset.Delete();

            return false;
        }

        public static bool Create(string filename, IGeometryDef geomDef, Fields fields, GmlVersion gmlVersion)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                string name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                string gml_filename = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".gml";
                string xsd_filename = fi.FullName.Substring(0, fi.FullName.Length - fi.Extension.Length) + ".xsd";

                FeatureClass featureClass = new FeatureClass(null, name, fields);
                XmlSchemaWriter schemaWriter = new XmlSchemaWriter(featureClass);
                string schema = schemaWriter.Write();

                StreamWriter sw = new StreamWriter(xsd_filename, false, Encoding.UTF8);
                sw.WriteLine(schema.Trim());
                sw.Flush();
                sw.Close();

                sw = new StreamWriter(gml_filename, false, Encoding.UTF8);
                sw.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<gml:FeatureCollection xmlns:gml=""http://www.opengis.net/gml"" 
                       xmlns:xlink=""http://www.w3.org/1999/xlink"" 
                       xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                       xmlns:gv=""http://www.gViewGIS.com/server"" 
                       xsi:schemaLocation=""http://www.gview.com/gml " + name + @".xsd"">".Trim());

                string boundingBox = GeometryTranslator.Geometry2GML(new Envelope(), String.Empty, gmlVersion);
                sw.WriteLine(@"
   <gml:boundedBy>");
                sw.Write(boundingBox);
                sw.Write(@"
   </gml:boundedBy>");

                sw.Write(@"
</gml:FeatureCollection>");

                sw.Flush();
                sw.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private IEnvelope Envelope
        {
            get
            {
                if (_doc == null) return null;
                try
                {
                    XmlNode envNode = _doc.SelectSingleNode("GML:FeatureCollection/GML:boundedBy", _ns);
                    if (envNode == null) return null;
                    return GeometryTranslator.GML2Geometry(envNode.InnerXml, _gmlVersion) as IEnvelope;
                }
                catch(Exception ex)
                {
                    _errMsg = ex.Message;
                    return null;
                }
            }
            set
            {
                if (_doc == null || value==null) return;

                try
                {
                    XmlNode coords = _doc.SelectSingleNode("GML:FeatureCollection/GML:boundedBy/GML:Box/GML:coordinates", _ns);
                    if (coords == null) return;

                    coords.InnerText =
                        value.minx.ToString(_nhi) + "," + value.miny.ToString(_nhi) + " " +
                        value.maxx.ToString(_nhi) + "," + value.maxy.ToString(_nhi);

                }
                catch
                {
                }
            }
        }

        private bool UpdateEnvelope(IEnvelope append)
        {
            if (append == null) return true;

            IEnvelope envelope = this.Envelope;
            if (envelope == null) return false;

            if (envelope.minx == 0.0 && envelope.maxx == 0.0 &&
                envelope.miny == 0.0 && envelope.maxy == 0.0)
            {
                envelope = append;
            }
            else
            {
                envelope.Union(append);
            }

            this.Envelope = envelope;
            return true;
        }
        public bool AppendFeature(IFeatureClass fc, IFeature feature)
        {
            if (_doc == null || fc == null || feature == null) return false;

            XmlNode featureCollection = _doc.SelectSingleNode("GML:FeatureCollection", _ns);
            if (featureCollection == null) return false;

            XmlNode featureMember = _doc.CreateElement("gml","featureMember",_ns.LookupNamespace("GML"));
            XmlNode featureclass = _doc.CreateElement("gv", fc.Name, _ns.LookupNamespace("myns"));

            featureMember.AppendChild(featureclass);

            if (feature.Shape != null)
            {
                try
                {
                    string geom = GeometryTranslator.Geometry2GML(feature.Shape, String.Empty, _gmlVersion);
                    geom = @"<gml:theGeometry xmlns:gml=""http://www.opengis.net/gml"">" + geom + "</gml:theGeometry>";
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(geom);

                    XmlNode geomNode = _doc.CreateElement("gv", fc.ShapeFieldName.Replace("#", ""), _ns.LookupNamespace("myns"));
                    
                    foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
                    {
                        geomNode.AppendChild(_doc.ImportNode(node, true));
                    }
                    if (!UpdateEnvelope(feature.Shape.Envelope))
                    {
                        _errMsg = "Can't update envelope...";
                        return false;
                    }
                    featureclass.AppendChild(geomNode);
                }
                catch(Exception ex)
                {
                    _errMsg = ex.Message;
                    return false;
                }
            }
            foreach (FieldValue fv in feature.Fields)
            {
                XmlNode attrNode = _doc.CreateElement("gv", fv.Name.Replace("#", ""), _ns.LookupNamespace("myns"));
                if (fv.Value != null)
                    attrNode.InnerText = fv.Value.ToString();
                featureclass.AppendChild(attrNode);
            }
            featureCollection.AppendChild(featureMember);

            return true;
        }

        public bool Flush()
        {
            if (_doc == null) return false;
            try
            {
                _doc.Save(_filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

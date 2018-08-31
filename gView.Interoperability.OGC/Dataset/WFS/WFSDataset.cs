using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Interoperability.OGC.Dataset.WMS;
using gView.Framework.Web;
using System.Xml;
using gView.Framework.OGC.WFS;
using gView.Framework.Geometry;

namespace gView.Interoperability.OGC.Dataset.WFS
{
    class WFSDataset
    {
        private string _url;
        internal GetCapabilities _getCapabilities;
        internal DescribeFeatureType _decribeFeatureType;
        internal GetFeature _getFeature;
        internal Operations _operations;
        internal Filter_Capabilities _filter_capabilites;
        internal WMSDataset _dataset;
        internal GmlVersion _gmlVersion = GmlVersion.v1;

        private List<IDatasetElement> _elements = new List<IDatasetElement>();

        public WFSDataset(WMSDataset dataset, string url)
        {
            _dataset = dataset;
            _url = url;
        }

        public bool Open()
        {
            try
            {
                _elements.Clear();
                string param = "REQUEST=GetCapabilities&VERSION=1.0.0&SERVICE=WFS";

                string url = WMSDataset.Append2Url(_url, param);
                string response = WebFunctions.HttpSendRequest(url, "GET", null);
                response = WMSDataset.RemoveDOCTYPE(response);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("WFS", "http://www.opengis.net/wfs");
                ns.AddNamespace("OGC", "http://www.opengis.net/ogc");
                ns.AddNamespace("GML", "http://www.opengis.net/gml");

                XmlNode CapabilitiesNode = doc.SelectSingleNode("WFS:WFS_Capabilities/WFS:Capability", ns);
                _getCapabilities = new GetCapabilities(CapabilitiesNode.SelectSingleNode("WFS:Request/WFS:GetCapabilities", ns), ns);
                _decribeFeatureType = new DescribeFeatureType(CapabilitiesNode.SelectSingleNode("WFS:Request/WFS:DescribeFeatureType", ns), ns);
                _getFeature = new GetFeature(CapabilitiesNode.SelectSingleNode("WFS:Request/WFS:GetFeature", ns), ns);

                XmlNode FeatureTypeListNode = doc.SelectSingleNode("WFS:WFS_Capabilities/WFS:FeatureTypeList", ns);
                _operations = new Operations(FeatureTypeListNode.SelectSingleNode("WFS:Operations", ns));

                foreach (XmlNode featureTypeNode in FeatureTypeListNode.SelectNodes("WFS:FeatureType", ns))
                {
                    string name = "";
                    string title = "";

                    XmlNode nameNode = featureTypeNode.SelectSingleNode("WFS:Name", ns);
                    XmlNode titleNode = featureTypeNode.SelectSingleNode("WFS:Title", ns);

                    WMSClass.SRS srs = new WMSClass.SRS(featureTypeNode, ns, "WFS");

                    name = title=nameNode.InnerText;
                    if (titleNode != null) title = titleNode.InnerText;

                    WFSFeatureClass featureClass = new WFSFeatureClass(this, name, srs);
                    //DatasetElement dselement = new DatasetElement(featureClass);
                    ILayer dselement = LayerFactory.Create(featureClass);
                    if (dselement == null) continue;
                    dselement.Title = name;

                    _elements.Add(dselement);
                }

                _filter_capabilites = new Filter_Capabilities(doc.SelectSingleNode("WFS:WFS_Capabilities/OGC:Filter_Capabilities", ns), ns);
                return true;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return false;
            }
        }

        public List<IDatasetElement> Elements
        {
            get { return _elements; }
        }

        public IDatasetElement this[string name]
        {
            get
            {
                if (_elements == null) return null;

                foreach (IDatasetElement element in _elements)
                {
                    if (element == null || element.Class == null) continue;
                    if (element.Class.Name == name) return element;
                }
                return null;
            }
        }

        #region HeplperClasses
        internal class GetRequest
        {
            public string Get_OnlineResource = "";
            public string Post_OnlineResource = "";
            public List<string> Formats = new List<string>();

            public GetRequest(XmlNode node, XmlNamespaceManager ns)
            {
                if (node == null) return;

                XmlNode onlineResource = node.SelectSingleNode("WFS:DCPType/WFS:HTTP/WFS:Get", ns);
                if (onlineResource != null && onlineResource.Attributes["onlineResource"] != null)
                    Get_OnlineResource = onlineResource.Attributes["onlineResource"].Value;

                onlineResource = node.SelectSingleNode("WFS:DCPType/WFS:HTTP/WFS:Post", ns);
                if (onlineResource != null && onlineResource.Attributes["onlineResource"] != null)
                    Post_OnlineResource = onlineResource.Attributes["onlineResource"].Value;


                // TODO:
                // Nur zu testzwecken!!!
                //Get_OnlineResource = Get_OnlineResource.Replace("mapserv?", "mapserv425?");
                //Post_OnlineResource = Post_OnlineResource.Replace("mapserv?", "mapserv425?");
            }
        }
        internal class GetCapabilities : GetRequest
        {
            public GetCapabilities(XmlNode node, XmlNamespaceManager ns)
                : base(node, ns)
            {
            }
        }
        internal class DescribeFeatureType : GetRequest
        {
            public DescribeFeatureType(XmlNode node, XmlNamespaceManager ns)
                : base(node, ns)
            {
            }
        }
        internal class GetFeature : GetRequest
        {
            public GetFeature(XmlNode node, XmlNamespaceManager ns)
                : base(node, ns)
            {
            }
        }
        internal class Operations
        {
            List<string> SupportedOperations = new List<string>();

            public Operations(XmlNode node)
            {
                if (node == null) return;

                foreach (XmlNode c in node.ChildNodes)
                {
                    SupportedOperations.Add(c.Name);
                }
            }

            public bool Supports(string op)
            {
                foreach (string operation in SupportedOperations)
                {
                    if (operation.ToLower() == op.ToLower()) return true;
                }
                return false;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Xml;

namespace gView.Framework.OGC.WFS
{
    public enum WFSRequestType
    {
        GetCapabilities,
        DescribeFeatureType,
        GetFeature
    }

    public class WFSParameterDescriptor
    {
        #region Declarations
        private WFSRequestType _requestType = WFSRequestType.GetCapabilities;
        private string _version = "1.0.0", _typeName = "", _srsName = String.Empty;
        private int _maxFeatures = -1;
        private XmlNode _filter = null;
        private XmlNode _sortBy = null;
        private string _subfields = "*";

        #endregion

        public WFSParameterDescriptor()
        {
        }
        #region Parse
        public bool ParseParameters(string[] parameters)
        {
            ParseParameters(new Parameters(parameters));
            return true;
        }
        private void ParseParameters(Parameters Request)
        {
            if (Request["REQUEST"] == null)
            {
                WriteError("mandatory REQUEST parameter is missing");
            }
            switch (Request["REQUEST"].ToUpper())
            {
                case "GETCAPABILITIES":
                    _requestType = WFSRequestType.GetCapabilities;
                    break;
                case "DESCRIBEFEATURETYPE":
                    _requestType = WFSRequestType.DescribeFeatureType;
                    break;
                case "GETFEATURE":
                    _requestType = WFSRequestType.GetFeature;
                    break;
            }
            if (Request["VERSION"] == null)
            {
                WriteError("mandatory VERSION parameter is missing");
            }
            _version = Request["VERSION"];

            if (Request["TYPENAME"] == null && _requestType != WFSRequestType.GetCapabilities)
            {
                WriteError("mandatory VERSION parameter is missing");
            }
            _typeName = Request["TYPENAME"];

            if (Request["MAXFEATURES"] != null)
            {
                _maxFeatures = int.Parse(Request["MAXFEATURES"]);
            }

            if (Request["FILTER"] != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Request["FILTER"]);
                _filter = doc.SelectSingleNode("Filter");
            }
        }

        public bool ParseParameters(XmlDocument doc)
        {
            if (doc == null) return false;
            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("WFS", "http://www.opengis.net/wfs");
            ns.AddNamespace("OGC", "http://www.opengis.net/ogc");

            XmlNode request = doc.SelectSingleNode("WFS:GetCapabilities", ns);
            if (request != null)
            {
                _requestType = WFSRequestType.GetCapabilities;
            }

            if (request == null)
            {
                request = doc.SelectSingleNode("WFS:DescribeFeatureType", ns);
                if (request != null)
                {
                    _requestType = WFSRequestType.DescribeFeatureType;
                }
            }

            if (request == null)
            {
                request = doc.SelectSingleNode("WFS:GetFeature", ns);

                if (request != null)
                {
                    _requestType = WFSRequestType.GetFeature;
                }
            }

            if (request == null)
            {
                return false;
            }

            if (request.Attributes["version"] == null)
            {
                WriteError("mandatory VERSION parameter is missing");
            }
            _version = request.Attributes["version"].Value;
            if (request.Attributes["maxfeatures"] != null)
            {
                _maxFeatures = int.Parse(request.Attributes["maxfeatures"].Value);
            }
            else if (request.Attributes["maxFeatures"] != null)
            {
                _maxFeatures = int.Parse(request.Attributes["maxFeatures"].Value);
            }

            XmlNode query = request.SelectSingleNode("WFS:Query[@typeName]", ns);
            if (query != null)
            {
                _typeName = query.Attributes["typeName"].Value;
                
                _filter = query.SelectSingleNode("Filter", ns);
                if (_filter == null)
                    _filter = query.SelectSingleNode("WFS:Filter", ns);
                if (_filter == null)
                    _filter = query.SelectSingleNode("OGC:Filter", ns);

                _sortBy = query.SelectSingleNode("SortBy", ns);
                if (_sortBy == null)
                    _sortBy = query.SelectSingleNode("WFS:SortBy", ns);
                if (_sortBy == null)
                    _sortBy = query.SelectSingleNode("OGC:SortBy", ns);

                if (query.Attributes["srsName"] != null)
                {
                    _srsName = query.Attributes["srsName"].Value;
                }
            }

            XmlNode typeName = request.SelectSingleNode("WFS:TypeName", ns);
            if (typeName != null)
            {
                _typeName = typeName.InnerText;
            }
            return true;
        }
        #endregion

        #region ErrorReport
        private void WriteError(String msg)
        {
            WriteError(msg, null);
        }

        private void WriteError(String msg, String code)
        {
            

        }

        protected void WriteKMLError(string msg, string code)
        {
            //Response.ClearContent();
            //Response.ClearHeaders();
            //Response.Clear();
            //Response.Buffer = true;

            //Response.ContentType = "application/vnd.google-earth.kml+xml";
            //Response.ContentEncoding = System.Text.Encoding.UTF8;

            String sRet = String.Empty;

            sRet = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            sRet += "<kml xmlns=\"http://earth.google.com/kml/2.0\">";
            sRet += "<Folder>";
            sRet += "<name>ERROR</name>";
            sRet += "<description>" + msg + "</description>";
            sRet += "</Folder>";
            sRet += "</kml>";

            //Response.Write(sRet);
            //Response.Flush();
            //Response.End();
        }
        #endregion

        public WFSRequestType Request
        {
            get { return _requestType; }
            set { _requestType = value; }
        }
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }
        public int MaxFeatures
        {
            get { return _maxFeatures; }
            set { _maxFeatures = value; }
        }
        public XmlNode Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
        public XmlNode SortBy
        {
            get { return _sortBy; }
            set { _sortBy = value; }
        }
        public string SubFields
        {
            get { return _subfields; }
        }
        public string SrsName
        {
            get { return _srsName; }
        }
        private class Parameters
        {
            private Dictionary<string, string> _parameters = new Dictionary<string, string>();

            public Parameters(string[] list)
            {
                if (list == null) return;

                foreach (string l in list)
                {
                    string[] p = l.Split('=');

                    string p1 = p[0].Trim().ToUpper(), pp;
                    string p2 = ((p.Length > 1) ? p[1].Trim() : "");

                    if (_parameters.TryGetValue(p1, out pp)) continue;

                    _parameters.Add(p1, p2);
                }
            }

            public string this[string parameter]
            {
                get
                {
                    string o;
                    if (!_parameters.TryGetValue(parameter.ToUpper(), out o))
                        return null;

                    return o;
                }
            }
        }
    }
}

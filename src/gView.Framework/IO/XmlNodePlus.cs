using System.Collections;
using System.Globalization;
using System.Xml;

namespace gView.Framework.IO
{
    internal class XmlNodePlus
    {
        private Hashtable _hash;
        private NumberFormatInfo _nhi = global::System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

        public XmlNodePlus(XmlNode node, string cultureName = null)
        {
            Node = node;
            _hash = new Hashtable();

            cultureName = Node.Attributes["culture"]?.Value ?? cultureName;
            if (cultureName != null)
            {
                foreach (CultureInfo cu in global::System.Globalization.CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (cu.TextInfo.CultureName == cultureName)
                    {
                        _nhi = cu.NumberFormat;
                        break;
                    }
                }
            }
        }
        public XmlNodePlus(XmlNode node, NumberFormatInfo nhi) : this(node)
        {
            _nhi = nhi;
        }
        public XmlNode Node { get; set; }

        public XmlNode Next(string xPath)
        {
            if (_hash[xPath] == null)
            {
                _hash.Add(xPath, new XmlNodeCursor(Node.SelectNodes(xPath)));
            }
            XmlNodeCursor cursor = (XmlNodeCursor)_hash[xPath];
            if (cursor == null)
            {
                return null;
            }

            return cursor.Next;
        }

        public NumberFormatInfo NumberFormat
        {
            get { return _nhi; }
        }

        public CultureInfo Culture
        {
            set
            {
                if (value == null)
                {
                    return;
                }

                if (Node.Attributes["culture"] != null)
                {
                    Node.Attributes["culture"].Value = value.TextInfo.CultureName;
                }
                else
                {
                    XmlAttribute attr = Node.OwnerDocument.CreateAttribute("culture");
                    attr.Value = value.TextInfo.CultureName;

                    Node.Attributes.Append(attr);
                }

                _nhi = value.NumberFormat;
            }
        }
    }
}

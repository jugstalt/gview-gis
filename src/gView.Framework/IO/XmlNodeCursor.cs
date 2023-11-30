using System.Xml;

namespace gView.Framework.IO
{
    internal class XmlNodeCursor
    {
        private int _pos = 0;
        private XmlNodeList _list;

        public XmlNodeCursor(XmlNodeList list)
        {
            _list = list;
        }

        public XmlNode Next
        {
            get
            {
                if (_list == null)
                {
                    return null;
                }

                if (_pos >= _list.Count)
                {
                    return null;
                }

                return _list[_pos++];
            }
        }
    }
}

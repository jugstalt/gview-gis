using gView.Framework.Core.Data.Cursors;

namespace gView.Framework.Data
{
    public class XmlCursor : IXmlCursor
    {
        private string _xml;

        public XmlCursor(string xml)
        {
            _xml = xml;
        }

        #region IXmlCursor Member

        public string Xml
        {
            get { return _xml; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}

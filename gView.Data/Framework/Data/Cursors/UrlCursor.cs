namespace gView.Framework.Data.Cursors
{
    public class UrlCursor : IUrlCursor
    {
        private string _url;

        public UrlCursor(string url)
        {
            _url = url;
        }

        #region IUrlCursor Member

        public string Url
        {
            get { return _url; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}

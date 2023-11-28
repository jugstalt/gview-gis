using gView.Framework.Core.Data.Cursors;

namespace gView.Framework.Data.Cursors
{
    public class TextCursor : ITextCursor
    {
        private string _text;
        public TextCursor(string text)
        {
            _text = text;
        }
        #region ITextCursor Member

        public string Text
        {
            get { return _text; }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}

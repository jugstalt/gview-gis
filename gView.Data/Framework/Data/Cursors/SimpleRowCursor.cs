using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Data.Cursors
{
    public class SimpleRowCursor : IRowCursor
    {
        private List<IRow> _rows;
        private int _pos = 0;

        public SimpleRowCursor(List<IRow> rows)
        {
            _rows = rows;
        }

        #region IRowCursor Member

        public Task<IRow> NextRow()
        {
            if (_rows == null || _pos >= _rows.Count)
            {
                return Task.FromResult<IRow>(null);
            }

            return Task.FromResult<IRow>(_rows[_pos++]);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}

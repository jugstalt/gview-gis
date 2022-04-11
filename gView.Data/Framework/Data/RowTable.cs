using gView.Framework.Data.Cursors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class RowTable : ITable
    {
        private DataTable _table;
        private string _idFieldName;
        private IRowCursor _cursor;
        private bool _hasMore = true;

        public event RowsAddedToTableEvent RowsAddedToTable;

        public RowTable(IRowCursor cursor, IFieldCollection fields)
        {
            _table = new DataTable(System.Guid.NewGuid().ToString("N"));
            _cursor = cursor;
            CreateColumns(fields);
        }

        public void Dispose()
        {
            _table.Dispose(); _table = null;
        }

        public Task<int> FillAtLeast(List<int> IDs)
        {
            return Task.FromResult<int>(0);
        }

        public Task<int> Fill()
        {
            return Fill(-1);
        }

        async public Task<int> Fill(int next_N_Rows)
        {
            if (_cursor == null)
            {
                return 0;
            }

            int counter = 0;

            IRow feat = await _cursor.NextRow();
            while (feat != null)
            {
                if (_table.Columns.Count > 0)
                {
                    DataRow row = _table.NewRow();
                    foreach (IFieldValue fv in feat.Fields)
                    {
                        if (_table.Columns[fv.Name] == null)
                        {
                            continue;
                        }

                        try
                        {
                            row[fv.Name] = fv.Value;
                        }
                        catch { }
                    }
                    _table.Rows.Add(row);
                }

                counter++;

                if ((counter % 50) == 0)
                {
                    RowsAddedToTable?.Invoke(50);
                }

                if (counter >= next_N_Rows && next_N_Rows > 0)
                {
                    _hasMore = true;
                    return counter;
                }
                feat = await _cursor.NextRow();
            }

            _hasMore = false;
            return counter;
        }
        public bool hasMore
        {
            get { return _hasMore; }
        }
        private bool CreateColumns(IFieldCollection fields)
        {
            try
            {
                if (fields == null)
                {
                    return false;
                }

                foreach (IField pField in fields.ToEnumerable())
                {
                    if (pField.type == FieldType.Shape)
                    {
                        continue;
                    }

                    switch (pField.type)
                    {
                        case FieldType.ID:
                            _idFieldName = pField.name;
                            _table.Columns.Add(pField.name, typeof(int));
                            break;
                        case FieldType.biginteger:
                        case FieldType.integer:
                        case FieldType.smallinteger:
                            _table.Columns.Add(pField.name, typeof(int));
                            break;
                        case FieldType.boolean:
                            _table.Columns.Add(pField.name, typeof(bool));
                            break;
                        case FieldType.Double:
                            _table.Columns.Add(pField.name, typeof(double));
                            break;
                        case FieldType.Float:
                            _table.Columns.Add(pField.name, typeof(float));
                            break;
                        default:
                            _table.Columns.Add(pField.name, typeof(string));
                            break;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region IRowTable Member

        public DataTable Table
        {
            get
            {
                return _table;
            }
        }

        public string IDFieldName
        {
            get
            {
                return _idFieldName;
            }
        }

        #endregion
    }
}

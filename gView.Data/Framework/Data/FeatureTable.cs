using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.system;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class FeatureTable : IFeatureTable
    {
        private DataTable _table;
        private string _idFieldName;
        private Hashtable _geometry;
        private IFeatureCursor _cursor;
        private bool _hasMore = true;
        private ITableClass _tableClass;

        public event RowsAddedToTableEvent RowsAddedToTable;

        public FeatureTable(IFeatureCursor cursor, IFieldCollection fields, ITableClass tableClass)
        {
            _table = new DataTable(System.Guid.NewGuid().ToString("N"));
            _cursor = cursor;
            _tableClass = tableClass;
            _geometry = new Hashtable();
            CreateColumns(fields);
        }

        public void Dispose()
        {
            _table.Dispose(); _table = null;
            _geometry.Clear();
        }

        public void ReleaseCursor()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }
        }

        // Lebt nur noch, weil es im FromDataTableOld vorkommt
        // Bitte nicht mehr verwenden...
        public Task<int> FillAtLeast(List<int> IDs)
        {
            return Fill(IDs, null);
        }
        public Task<int> Fill(List<int> IDs)
        {
            return Fill(IDs, null);
        }
        async public Task<int> Fill(List<int> IDs, ICancelTracker cancelTracker)
        {
            int counter = 0;
            if (_tableClass is IFeatureClass)
            {
                ReleaseCursor();

                IFeatureClass fc = (IFeatureClass)_tableClass;

                RowIDFilter filter = new RowIDFilter(fc.IDFieldName);
                filter.IDs = IDs;
                filter.SubFields = "*";
                _cursor = await fc.GetFeatures(filter);
                if (_cursor == null)
                {
                    return 0;
                }

                return await Fill(cancelTracker);
            }
            return counter;
        }

        public Task<int> Fill()
        {
            return Fill(-1, null);
        }
        public Task<int> Fill(ICancelTracker cancelTracker)
        {
            return Fill(-1, cancelTracker);
        }
        public Task<int> Fill(int next_N_Rows)
        {
            return Fill(next_N_Rows, null);
        }
        async public Task<int> Fill(int next_N_Rows, ICancelTracker cancelTracker)
        {
            if (_cursor == null)
            {
                return 0;
            }

            int counter = 0;

            IFeature feat = await _cursor.NextFeature();
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

                if (feat.Shape != null)
                {
                    _geometry.Remove(feat.OID);
                    _geometry.Add(feat.OID, feat.Shape);
                }

                counter++;

                if (RowsAddedToTable != null)
                {
                    if ((counter % 50) == 0)
                    {
                        RowsAddedToTable(50);
                    }
                }

                if ((counter >= next_N_Rows && next_N_Rows > 0) || (cancelTracker != null && !cancelTracker.Continue))
                {
                    _hasMore = true;
                    return counter;
                }
                feat = (_cursor != null) ? await _cursor.NextFeature() : null;
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
                            DataColumn col = new DataColumn(pField.name, typeof(int));
                            _table.Columns.Add(col);
                            _table.PrimaryKey = new DataColumn[] { col };
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
        #region IFeatureTable Member

        public DataTable Table
        {
            get
            {
                return _table;
            }
        }

        public IGeometry Shape(object ObjectID)
        {
            return (IGeometry)_geometry[ObjectID];
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

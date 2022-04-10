using gView.Framework.FDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Data
{
    public class RowData : IRowData
    {
        protected List<FieldValue> _fields;
        private bool _caseSensitivFieldnameMatching;

        public RowData()
        {
            _fields = new List<FieldValue>();
            _caseSensitivFieldnameMatching = true;
        }

        #region IRowData Member

        public List<FieldValue> Fields
        {
            get
            {
                return _fields;
            }
        }

        public object this[string fieldName]
        {
            get
            {
                if (_caseSensitivFieldnameMatching == true)
                {
                    foreach (FieldValue fv in _fields)
                    {
                        if (fv.Name == fieldName)
                        {
                            return fv.Value;
                        }
                    }
                }
                else
                {
                    fieldName = fieldName.ToLower();
                    foreach (FieldValue fv in _fields)
                    {
                        if (fv.Name.ToLower() == fieldName)
                        {
                            return fv.Value;
                        }
                    }
                }
                return null;
            }
            set
            {
                if (_caseSensitivFieldnameMatching)
                {
                    foreach (FieldValue fv in _fields)
                    {
                        if (fv.Name == fieldName)
                        {
                            fv.Value = value;
                        }
                    }
                }
                else
                {
                    fieldName = fieldName.ToLower();
                    foreach (FieldValue fv in _fields)
                    {
                        if (fv.Name.ToLower() == fieldName)
                        {
                            fv.Value = value;
                            return;
                        }
                    }
                }
            }
        }
        public object this[int fieldIndex]
        {
            get
            {
                if (fieldIndex < 0 || fieldIndex >= _fields.Count)
                {
                    return null;
                }

                return _fields[fieldIndex].Value;
            }
            set
            {
                if (fieldIndex < 0 || fieldIndex >= _fields.Count)
                {
                    return;
                }

                _fields[fieldIndex].Value = value;
            }
        }

        public FieldValue FindField(string name)
        {
            if (_caseSensitivFieldnameMatching)
            {
                foreach (FieldValue field in this.Fields)
                {
                    if (field.Name == name)
                    {
                        return field;
                    }
                }
            }
            else
            {
                name = name.ToLower();
                foreach (FieldValue field in this.Fields)
                {
                    if (field.Name.ToLower() == name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public bool CaseSensitivFieldnameMatching
        {
            get { return _caseSensitivFieldnameMatching; }
            set { _caseSensitivFieldnameMatching = value; }
        }
        #endregion

        #region IDBOperations Member

        async public Task<bool> BeforeInsert(ITableClass tClass)
        {
            if (!(tClass is IFeatureClass))
            {
                return false;
            }

            IFeatureClass fc = (IFeatureClass)tClass;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                if (field is IAutoField)
                {
                    if (!await ((IAutoField)field).OnInsert(fc, this as IFeature))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        async public Task<bool> BeforeUpdate(ITableClass tClass)
        {
            if (!(tClass is IFeatureClass))
            {
                return false;
            }

            IFeatureClass fc = (IFeatureClass)tClass;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                if (field is IAutoField)
                {
                    if (!await ((IAutoField)field).OnUpdate(fc, this as IFeature))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Task<bool> BeforeDelete(ITableClass tClass)
        {
            return Task.FromResult<bool>(true);
        }


        #endregion
    }
}

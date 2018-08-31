using System;
using System.Collections;
using System.Collections.Generic;
using gView.Framework.Geometry;
using gView.Framework.FDB;

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
                        if (fv.Name == fieldName) return fv.Value;
                    }
                }
                else
                {
                    fieldName = fieldName.ToLower();
                    foreach (FieldValue fv in _fields)
                    {
                        if (fv.Name.ToLower() == fieldName) return fv.Value;
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
                if (fieldIndex < 0 || fieldIndex >= _fields.Count) return null;
                return _fields[fieldIndex].Value;
            }
            set
            {
                if (fieldIndex < 0 || fieldIndex >= _fields.Count) return;
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
                        return field;
                }
            }
            else
            {
                name = name.ToLower();
                foreach (FieldValue field in this.Fields)
                {
                    if (field.Name.ToLower() == name)
                        return field;
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

        public bool BeforeInsert(ITableClass tClass)
        {
            if (!(tClass is IFeatureClass)) return false;

            IFeatureClass fc = (IFeatureClass)tClass;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                if (field is IAutoField)
                {
                    if (!((IAutoField)field).OnInsert(fc, this as IFeature)) return false;
                }
            }

            return true;
        }

        public bool BeforeUpdate(ITableClass tClass)
        {
            if (!(tClass is IFeatureClass)) return false;

            IFeatureClass fc = (IFeatureClass)tClass;
            foreach (IField field in fc.Fields.ToEnumerable())
            {
                if (field is IAutoField)
                {
                    if (!((IAutoField)field).OnUpdate(fc, this as IFeature)) return false;
                }
            }

            return true;
        }

        public bool BeforeDelete(ITableClass tClass)
        {
            return true;
        }


        #endregion
    }

    public class Row : RowData, IRow
    {
        protected int _oid;

        public Row()
            : base()
        {
        }

        #region IOID Member

        public int OID
        {
            get
            {
                return _oid;
            }
            set
            {
                _oid = value;
            }
        }

        #endregion
    }

    public class GlobalRow : RowData, IGlobalRow
    {
        protected long _oid;

        public GlobalRow()
            : base()
        {
        }

        #region IOID Member

        public long GlobalOID
        {
            get
            {
                return _oid;
            }
            set
            {
                _oid = value;
            }
        }

        #endregion
    }

	public class Feature : Row, IFeature
	{
		private gView.Framework.Geometry.IGeometry _geometry;

		public Feature()
            : base()
		{
            
		}

        public Feature(IRow row)
            : base()
        {
            if (row == null) return;

            _oid = row.OID;
            _fields = row.Fields;
        }

		#region IFeature Member

		public gView.Framework.Geometry.IGeometry Shape
		{
			get
			{
				return _geometry;
			}
			set
			{
				_geometry=value;
			}
		}

		#endregion

        public static void CopyFrom(IFeature original, IFeature feature)
        {
            if (feature == null || original == null) return;

            original.Shape = feature.Shape;
            if (feature.Fields != null)
            {
                foreach (IFieldValue fv in feature.Fields)
                {
                    original[fv.Name] = fv.Value;
                }
            }
        }
    }

    public class Features : List<IFeature>
    {
    }

    public class GlobalFeature : GlobalRow, IGlobalFeature, IFeature
    {
        private gView.Framework.Geometry.IGeometry _geometry;

        public GlobalFeature()
            : base()
		{
            
		}

        public GlobalFeature(IGlobalRow row)
            : base()
        {
            if (row == null) return;

            _oid = row.GlobalOID;
            _fields = row.Fields;
        }
        #region IGlobalFeature Member

        public gView.Framework.Geometry.IGeometry Shape
        {
            get
            {
                return _geometry;
            }
            set
            {
                _geometry = value;
            }
        }

        #endregion

        #region IOID Member

        public int OID
        {
            get { return (int)(_oid & 0xffffffff); }
        }

        #endregion
    }

	public class FieldValue : gView.Framework.Data.IFieldValue
	{
		private string _name;
		private object _value;

		public FieldValue(string name) 
		{
			_name=name;
		}
		public FieldValue(string name,object val) 
		{
			_name=name;
			_value=val;
		}

        public void Rename(string newName)
        {
            _name = newName;
        }
		#region IFieldValue Member

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value=value;
			}
		}

		#endregion
	}
}

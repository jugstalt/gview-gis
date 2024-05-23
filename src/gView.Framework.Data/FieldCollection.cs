using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace gView.Framework.Data
{
    public class FieldCollection : IFieldCollection, IPersistable
    {
        protected List<IField> _fields = null;
        protected IField _primaryField = null;
        private object _privateLocker = new object();

        public FieldCollection()
        {
            _fields = new List<IField>();
        }
        public FieldCollection(IFieldCollection fields)
            : this(fields?.ToEnumerable())
        {
        }

        public FieldCollection(IEnumerable<IField> fields)
            : this()
        {
            if (fields != null)
            {
                foreach (IField field in fields)
                {
                    if (field == null)
                    {
                        continue;
                    }

                    _fields.Add(field);
                }
            }
        }

        public FieldCollection(DataTable schemaTable)
        {
            _fields = new List<IField>();
            if (schemaTable != null)
            {
                foreach (DataRow row in schemaTable.Rows)
                {
                    this.Add(new Field(row));
                }
            }
        }

        internal void CopyFrom(IFieldCollection fields, IClass Class)
        {
            if (fields != null)
            {
                _fields = new List<IField>();
                foreach (IField field in fields.ToEnumerable())
                {
                    if (Class is ITableClass && ((ITableClass)Class).Fields != null)
                    {
                        IField classField = ((ITableClass)Class).FindField(field.name);
                        if (classField != null)
                        {
                            Field f = new Field(classField);
                            f.visible = field.visible;
                            f.aliasname = field.aliasname;
                            f.IsEditable = field.IsEditable;
                            f.IsRequired = field.IsRequired;
                            f.DefautValue = field.DefautValue;
                            f.Domain = field.Domain;

                            if (field is Field)
                            {
                                f.Priority = ((Field)field).Priority;
                            }
                            _fields.Add(f);
                            if (fields.PrimaryDisplayField != null && f.name == fields.PrimaryDisplayField.name)
                            {
                                _primaryField = f;
                            }
                        }
                    }
                    else
                    {
                        _fields.Add(new Field(field));
                    }
                }
            }

            // Add new fields "invisible" to the layer
            if (Class is ITableClass && ((ITableClass)Class).Fields != null)
            {
                IFieldCollection classFields = ((ITableClass)Class).Fields;
                if (_fields == null)
                {
                    _fields = new List<IField>();
                }

                foreach (IField classField in classFields.ToEnumerable())
                {
                    bool found = false;

                    foreach (IField field in _fields)
                    {
                        if (field.name == classField.name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Field f = new Field(classField);
                        f.visible = false;
                        _fields.Add(f);
                    }
                }
            }


            // Primary Field
            if (_primaryField != null && _fields != null)
            {
                bool found = false;
                foreach (IField field in _fields)
                {
                    if (field.name == _primaryField.name)
                    {
                        _primaryField = field;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _primaryField = null;
                }
            }
            if (_primaryField == null && _fields != null)
            {
                foreach (IField field in _fields)
                {
                    if (field.type == FieldType.String)
                    {
                        _primaryField = field;
                        break;
                    }
                }
            }
            if (_primaryField == null && _fields != null && _fields.Count > 0)
            {
                _primaryField = _fields[0];
            }
        }

        public IField[] ToArray() => _fields.ToArray();

        #region IFields Member
        public IField FindField(string aliasname)
        {
            if (_fields == null)
            {
                return null;
            }

            foreach (IField field in _fields)
            {
                if (field.aliasname == aliasname)
                {
                    return field;
                }
            }

            return null;
        }
        public IField PrimaryDisplayField
        {
            get
            {
                return _primaryField;
            }
            set
            {
                if (_fields == null || !_fields.Contains(value))
                {
                    return;
                }

                _primaryField = value;
            }
        }
        #endregion

        #region IPersistable
        public void Load(IPersistStream stream)
        {
            string primaryField = (string)stream.Load("PrimaryField", "");

            if (_fields == null)
            {
                _fields = new List<IField>();
            }

            IField field;
            while ((field = stream.Load("Field", null, new Field()) as IField) != null)
            {
                _fields.Add(field);
            }

            int priority = 0;
            foreach (IField fieldPriority in this.ToEnumerable())
            {
                if (fieldPriority is Field)
                {
                    ((Field)fieldPriority).Priority = priority++;
                }
            }

            _primaryField = null;
            foreach (IField f in _fields)
            {
                if (f.name == primaryField)
                {
                    _primaryField = f;
                }
            }
        }
        public void Save(IPersistStream stream)
        {
            if (_fields != null)
            {
                foreach (IField field in this.ToEnumerable())
                {
                    stream.Save("Field", field);
                }
            }

            if (_primaryField != null)
            {
                stream.Save("PrimaryField", _primaryField.name);
            }
        }
        #endregion

        #region IEnumerable<IField> Member

        //public IEnumerator<IField> GetEnumerator()
        //{
        //    return new FieldsEnumerator(_fields);
        //}

        #endregion

        #region IEnumerable Member

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        #endregion

        public void Add(IField field)
        {
            if (field is Field && ((Field)field).Priority == -1)
            {
                ((Field)field).Priority = _fields.Count;
            }
            _fields.Add(field);
        }
        public void Insert(int index, IField field)
        {
            if (field is Field && ((Field)field).Priority == -1)
            {
                ((Field)field).Priority = _fields.Count;
            }
            _fields.Insert(index, field);
        }

        public void Clear()
        {
            _fields.Clear();
        }
        public void Remove(IField field)
        {
            _fields.Remove(field);
        }
        private class FieldsEnumerator : IEnumerator<IField>
        {
            List<IField> _fields;
            public FieldsEnumerator(List<IField> fields)
            {
                _fields = fields;
                _fields.Sort(new FieldPriorityComparer());
            }
            int index = 0;
            #region IEnumerator<IField> Member

            public IField Current
            {
                get { return (index > 0 && index <= _fields.Count) ? _fields[index - 1] : null; }
            }

            #endregion

            #region IDisposable Member

            public void Dispose()
            {

            }

            #endregion

            #region IEnumerator Member

            object IEnumerator.Current
            {
                get { return (index > 0 && index <= _fields.Count) ? _fields[index - 1] : null; }
            }

            public bool MoveNext()
            {
                if (_fields == null)
                {
                    return false;
                }

                index++;
                return index <= _fields.Count;
            }

            public void Reset()
            {
                index = 0;
            }

            #endregion
        }

        #region IClone Member

        public object Clone()
        {
            FieldCollection fields = new FieldCollection();

            foreach (IField field in this.ToEnumerable())
            {
                if (field == null)
                {
                    continue;
                }

                fields.Add(new Field(field));
            }

            return fields;
        }

        #endregion

        #region IFields Member

        public IField this[int i]
        {
            get
            {
                if (_fields == null || i < 0 || i >= _fields.Count)
                {
                    return null;
                }

                return _fields[i];
            }
        }

        public int Count
        {
            get
            {
                return (_fields != null) ? _fields.Count : 0;
            }
        }

        public IEnumerable<IField> ToEnumerable()
        {
            lock (_privateLocker)  // Threadsafe
            {
                return _fields.ToArray().OrderBy(f => f is IPriority ? ((IPriority)f).Priority : int.MaxValue);
            }
        }

        #endregion

        #region HelperClasses
        private class FieldPriorityComparer : IComparer<IField>
        {
            #region IComparer<IField> Member

            public int Compare(IField x, IField y)
            {
                if (x is IPriority && y is IPriority)
                {
                    if (((IPriority)x).Priority < ((IPriority)y).Priority)
                    {
                        return -1;
                    }
                    else if (((IPriority)x).Priority < ((IPriority)y).Priority)
                    {
                        return 1;
                    }
                }
                return 0;
            }

            #endregion
        }
        #endregion

        //public IEnumerable<IField> ToArray()
        //{
        //    //List<IField> array = new List<IField>(_fields);
        //    //return array;
        //    return ToEnumerable();
        //}
    }
}

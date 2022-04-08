//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Text;
//using System.Threading.Tasks;
//using gView.Framework.Data;

//namespace gView.Framework.Data
//{
//    public class TableClassBindingSource : IListSource, IList, IEnumerator
//    {
//        private ITableClass _class;
//        private ICursor _cursor = null;
//        private int _count=0;

//        private TableClassBindingSource() { }

//        async static public Task<TableClassBindingSource> Create(ITableClass Class)
//        {
//            var source = new TableClassBindingSource();

//            source._class = Class;
//            if (source._class != null)
//            {
//                source._cursor = await Class.Search(new QueryFilter());
//            }
//            return source;
//        }

//        #region IListSource Member

//        public bool ContainsListCollection
//        {
//            get { return false; }
//        }

//        public System.Collections.IList GetList()
//        {
//            return this;
//        }

//        #endregion

//        #region IList Member

//        public int Add(object value)
//        {
//            return -1;
//        }

//        public void Clear()
//        {
//            Reset();
//        }

//        public bool Contains(object value)
//        {
//            return false;
//        }

//        public int IndexOf(object value)
//        {
//            return -1;
//        }

//        public void Insert(int index, object value)
//        {

//        }

//        public bool IsFixedSize
//        {
//            get { return false; }
//        }

//        public bool IsReadOnly
//        {
//            get { return true; }
//        }

//        public void Remove(object value)
//        {

//        }

//        public void RemoveAt(int index)
//        {

//        }

//        public object this[int index]
//        {
//            get
//            {
//                return null;
//            }
//            set
//            {

//            }
//        }

//        #endregion

//        #region ICollection Member

//        public void CopyTo(Array array, int index)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public int Count
//        {
//            get { return _count; }
//        }

//        public bool IsSynchronized
//        {
//            get { return false; }
//        }

//        public object SyncRoot
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        #endregion

//        #region IEnumerable Member

//        public IEnumerator GetEnumerator()
//        {
//            return this;
//        }

//        #endregion

//        #region IEnumerator Member

//        private object _current;
//        public object Current
//        {
//            get
//            {
//                return _current;
//            }
//        }

//        async public Task<bool> MoveNext()
//        {
//            if (_cursor is IFeatureCursor)
//                _current = await ((IFeatureCursor)_cursor).NextFeature();
//            else if (_cursor is IRowCursor)
//                _current = await ((IRowCursor)_cursor).NextRow();
//            else
//                _current = null;

//            if(_current==null)
//                return false;
//            _count++;

//            return true;
//        }

//        async public Task Reset()
//        {
//            if (_class == null)
//                return;

//            if (_cursor != null) _cursor.Dispose();
//            _cursor = await _class.Search(new QueryFilter());

//            _count=0;
//        }

//        #endregion
//    }

//    public class TableClassBindingSource2 : IBindingList
//    {
//        #region IBindingList Member

//        public void AddIndex(PropertyDescriptor property)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public object AddNew()
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public bool AllowEdit
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool AllowNew
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool AllowRemove
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public int Find(PropertyDescriptor property, object key)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public bool IsSorted
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public event ListChangedEventHandler ListChanged;

//        public void RemoveIndex(PropertyDescriptor property)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public void RemoveSort()
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public ListSortDirection SortDirection
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public PropertyDescriptor SortProperty
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool SupportsChangeNotification
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool SupportsSearching
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool SupportsSorting
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        #endregion

//        #region IList Member

//        public int Add(object value)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public void Clear()
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public bool Contains(object value)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public int IndexOf(object value)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public void Insert(int index, object value)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public bool IsFixedSize
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool IsReadOnly
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public void Remove(object value)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public void RemoveAt(int index)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public object this[int index]
//        {
//            get
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//            set
//            {
//                throw new Exception("The method or operation is not implemented.");
//            }
//        }

//        #endregion

//        #region ICollection Member

//        public void CopyTo(Array array, int index)
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        public int Count
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public bool IsSynchronized
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        public object SyncRoot
//        {
//            get { throw new Exception("The method or operation is not implemented."); }
//        }

//        #endregion

//        #region IEnumerable Member

//        public IEnumerator GetEnumerator()
//        {
//            throw new Exception("The method or operation is not implemented.");
//        }

//        #endregion
//    }
//}

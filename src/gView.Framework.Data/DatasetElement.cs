using gView.Framework.Core.Data;

namespace gView.Framework.Data
{
    public class DatasetElement : DataElementID, IDatasetElement
    {
        protected string _title = "";
        protected IClass _class = null;
        protected int _datasetID = 0;

        public DatasetElement() { }
        public DatasetElement(IClass Class)
        {
            _class = Class;
            if (_class != null)
            {
                _title = _class.Name;
            }
        }
        public DatasetElement(IDatasetElement element)
        {
            CopyFrom(element);
        }

        internal virtual void CopyFrom(IDatasetElement element)
        {
            if (element == null)
            {
                return;
            }

            this.ID = element.ID;
            this.SID = element.HasSID ? element.SID : null;

            _title = element.Title;
            //_class = element.Class;
            _datasetID = element.DatasetID;
        }

        #region IDatasetElement Member

        public string Title { get { return _title; } set { _title = value; } }
        public IClass Class
        {
            get { return _class; }
            set { _class = value; }
        }
        public virtual int DatasetID
        {
            get
            {
                return _datasetID;
            }
            set
            {
                _datasetID = value;
            }
        }
        #endregion

        public IClass Class2
        {
            set { _class = value; }
        }

        #region IDatasetElement Member


        public event PropertyChangedHandler PropertyChanged = null;

        public void FirePropertyChanged()
        {
            Refresh();

            if (PropertyChanged != null)
            {
                PropertyChanged();
            }
        }

        #endregion

        virtual protected void Refresh()
        {
        }
    }
}

using gView.Framework.Core.Data;
using gView.Framework.Core.IO;

namespace gView.Framework.Data
{
    public class WebServiceTheme2 : FeatureSelection, IWebServiceTheme
    {
        private string _id;
        private bool _locked;
        private IWebServiceClass _serviceClass;

        public WebServiceTheme2() { }
        public WebServiceTheme2(IClass Class, string name, string id, bool visible, IWebServiceClass serviceClass) :
            base(Class as IFeatureClass)
        {
            _class = Class;
            _serviceClass = serviceClass;
            _title = name;
            _id = id;
            _visible = visible;

            _locked = false;
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);

            if (element is IWebServiceTheme)
            {
                IWebServiceTheme theme = (IWebServiceTheme)element;

                _locked = theme.Locked;
                _id = theme.LayerID;
            }
        }

        #region IWebServiceTheme Member

        public string LayerID
        {
            get { return _id; }
        }

        //public bool Visible
        //{
        //    get
        //    {
        //        return _visible;
        //    }
        //    set
        //    {
        //        _visible = value;
        //    }
        //}

        public bool Locked
        {
            get
            {
                return _locked;
            }
            set
            {
                _locked = value;
            }
        }

        public IWebServiceClass ServiceClass
        {
            get { return _serviceClass; }
            set { _serviceClass = value; }
        }

        #endregion

        #region IPersistable Member

        public override void Load(IPersistStream stream)
        {
            base.Load(stream);

            _id = (string)stream.Load("id", "");
            _locked = (bool)stream.Load("locked", false);
        }

        public override void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("id", _id);
            stream.Save("locked", _locked);
        }

        #endregion
    }
}

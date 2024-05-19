using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using System.Collections.Generic;

namespace gView.Framework.Data
{
    public class WebServiceLayer : Layer, IWebServiceLayer
    {
        private List<IWebServiceTheme> _themes = null;

        public WebServiceLayer() { }
        public WebServiceLayer(IWebServiceClass webClass)
        {
            _class = webClass;
            if (webClass != null)
            {
                _title = webClass.Name;
            }
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);

            if (element is WebServiceLayer)
            {

            }
        }
        public override int DatasetID
        {
            get
            {
                return base.DatasetID;
            }
            set
            {
                base.DatasetID = value;

                if (WebServiceClass != null)
                {
                    foreach (IWebServiceTheme theme in WebServiceClass.Themes)
                    {
                        theme.DatasetID = value;
                    }
                }
            }
        }

        #region IWebServiceLayer Member

        public IWebServiceClass WebServiceClass
        {
            get { return _class as IWebServiceClass; }
        }

        #endregion

        public void SetWebServiceClass(IWebServiceClass wc)
        {
            _class = wc;
            SerializeThemes();
        }

        private void SerializeThemes()
        {
            if (!(_class is IWebServiceClass) || _themes == null)
            {
                return;
            }

            if (((IWebServiceClass)_class).Themes == null)
            {
                return;
            }

            foreach (IWebServiceTheme theme in ((IWebServiceClass)_class).Themes)
            {
                foreach (IWebServiceTheme t in _themes)
                {
                    if (t.LayerID == theme.LayerID)
                    {
                        theme.Locked = t.Locked;
                        theme.Title = t.Title;
                        theme.MinimumScale = t.MinimumScale;
                        theme.MaximumScale = t.MaximumScale;
                        theme.Visible = t.Visible;
                        theme.ID = t.ID;
                        theme.MinimumLabelScale = t.MinimumLabelScale;
                        theme.MaximumLabelScale = t.MaximumLabelScale;
                        theme.MaximumZoomToFeatureScale = t.MaximumZoomToFeatureScale;
                        theme.FeatureRenderer = t.FeatureRenderer;
                        theme.LabelRenderer = t.LabelRenderer;
                        theme.SelectionRenderer = t.SelectionRenderer;
                        theme.FilterQuery = t.FilterQuery;

                        // DatasetID wird in TOCCoClass.InsertLayer zugewiesen!!!
                        break;
                    }
                }
            }
            _themes = null;
        }
        public override void Load(IPersistStream stream)
        {
            base.Load(stream);

            WebServiceTheme theme;
            while ((theme = (WebServiceTheme)stream.Load("IWebServiceTheme", null, new WebServiceTheme())) != null)
            {
                if (_themes == null)
                {
                    _themes = new List<IWebServiceTheme>();
                }

                _themes.Add(theme);
            }

            SerializeThemes();
        }

        public override void Save(IPersistStream stream)
        {
            base.Save(stream);

            if (_class is IWebServiceClass && ((IWebServiceClass)_class).Themes != null)
            {
                foreach (IWebServiceTheme theme in ((IWebServiceClass)_class).Themes)
                {
                    if (theme is IPersistable)
                    {
                        stream.Save("IWebServiceTheme", theme);
                    }
                }
            }
        }
    }
}

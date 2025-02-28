using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using System;

namespace gView.Framework.Data
{

    /// <summary>
    /// 
    /// </summary>
    public class Layer : DatasetElement, ILayer, IPersistable
    {
        protected bool _visible = true;
        protected double _minimumScale = -1, _maximumScale = -1;
        protected double _minimumLabelScale = -1, _maximumLabelScale = -1;
        protected double _MaximumZoomToFeatureScale = 100;
        protected IGroupLayer _groupLayer = null;
        protected string _namespace = String.Empty;

        //protected QueryResult m_queryResult;

        public Layer()
        {
        }

        public Layer(ILayer layer)
        {
            CopyFrom(layer);
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);

            if (element is ILayer)
            {
                ILayer layer = element as ILayer;

                if (layer == null)
                {
                    return;
                }

                _visible = layer.Visible;
                _minimumScale = layer.MinimumScale;
                _maximumScale = layer.MaximumScale;

                _minimumLabelScale = layer.MinimumLabelScale;
                _maximumLabelScale = layer.MaximumLabelScale;

                _MaximumZoomToFeatureScale = layer.MaximumZoomToFeatureScale;

            }
        }

        static private void MinimumTocDependentScale(ILayer layer, ref double scale)
        {
            if (layer?.GroupLayer is null)
            {
                return;
            }

            if (scale <= 1.0)
            {
                scale = layer.GroupLayer.MinimumScale;
            }
            else if (layer.GroupLayer.MinimumScale > 1.0)
            {
                scale = Math.Max(layer.GroupLayer.MinimumScale, scale);
            }
        }
        static private void MaximumTocDependentScale(ILayer layer, ref double scale)
        {
            if (layer?.GroupLayer is null)
            {
                return;
            }

            if (scale <= 1.0)
            {
                scale = layer.GroupLayer.MaximumScale;
            }
            else if (layer.GroupLayer.MaximumScale > 1.0)
            {
                scale = Math.Min(layer.GroupLayer.MaximumScale, scale);
            }
        }

        static private void minimumLabelScale(ILayer layer, ref double scale)
        {
            if (layer == null || layer.GroupLayer == null)
            {
                return;
            }

            if (scale <= 1.0)
            {
                scale = layer.GroupLayer.MinimumLabelScale;
            }
            else if (layer.GroupLayer.MinimumLabelScale > 1.0)
            {
                scale = Math.Max(layer.GroupLayer.MinimumLabelScale, scale);
            }
        }
        static private void maximumLabelScale(ILayer layer, ref double scale)
        {
            if (layer == null || layer.GroupLayer == null)
            {
                return;
            }

            if (scale <= 1.0)
            {
                scale = layer.GroupLayer.MaximumLabelScale;
            }
            else if (layer.GroupLayer.MaximumLabelScale > 1.0)
            {
                scale = Math.Min(layer.GroupLayer.MaximumLabelScale, scale);
            }
        }

        static private void maximumZoomToFeatureScale(ILayer layer, ref double scale)
        {
            if (layer == null || layer.GroupLayer == null)
            {
                return;
            }

            if (scale <= 1.0)
            {
                scale = layer.GroupLayer.MaximumZoomToFeatureScale;
            }
            else if (layer.GroupLayer.MaximumZoomToFeatureScale > 1.0)
            {
                scale = Math.Max(layer.GroupLayer.MaximumZoomToFeatureScale, scale);
            }
        }

        static private void visible(ILayer layer, ref bool visible)
        {
            if (layer == null || layer.GroupLayer == null || visible == false)
            {
                return;
            }

            if (layer.GroupLayer.Visible == false)
            {
                visible = false;
            }
        }

        #region ILayer Member

        public bool Visible
        {
            get
            {
                bool visible = _visible;
                Layer.visible(this, ref visible);
                return visible;
            }
            set
            {
                _visible = value;
            }
        }

        public double MinimumScale
        {
            get
            {
                double scale = _minimumScale;
                Layer.MinimumTocDependentScale(this, ref scale);
                return scale;
            }
            set { _minimumScale = value; }
        }
        public double MaximumScale
        {
            get
            {
                double scale = _maximumScale;
                Layer.MaximumTocDependentScale(this, ref scale);
                return scale;
            }
            set { _maximumScale = value; }
        }

        public double MinimumLabelScale
        {
            get
            {
                double scale = _minimumLabelScale;
                Layer.minimumLabelScale(this, ref scale);
                return scale;
            }
            set { _minimumLabelScale = value; }
        }
        public double MaximumLabelScale
        {
            get
            {
                double scale = _maximumLabelScale;
                Layer.maximumLabelScale(this, ref scale);
                return scale;
            }
            set { _maximumLabelScale = value; }
        }

        public double MaximumZoomToFeatureScale
        {
            get
            {
                double scale = _MaximumZoomToFeatureScale;
                Layer.maximumZoomToFeatureScale(this, ref scale);
                return scale;
            }
            set { _MaximumZoomToFeatureScale = value; }
        }

        public IGroupLayer GroupLayer
        {
            get { return _groupLayer; }
            set { _groupLayer = value; }
        }
        #endregion

        #region INamespace Member

        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                _namespace = value;
            }
        }

        #endregion

        #region IPersistable Member

        virtual public void Load(IPersistStream stream)
        {
            this.ID = (int)stream.Load("ID", 0);
            this.SID = (string)stream.Load("SID", null);

            _title = (string)stream.Load("Title", "");
            _datasetID = (int)stream.Load("DatasetID");

            _namespace = (string)stream.Load("Namespace", String.Empty);

            _visible = (bool)stream.Load("visible", true);
            _minimumScale = (double)stream.Load("MinimumScale", 0.0);
            _maximumScale = (double)stream.Load("MaximumScale", 0.0);

            _minimumLabelScale = (double)stream.Load("MinimumLabelScale", 0.0);
            _maximumLabelScale = (double)stream.Load("MaximumLabelScale", 0.0);

            _MaximumZoomToFeatureScale = (double)stream.Load("MaximumZoomToFeatureScale", 0.0);
        }

        virtual public void Save(IPersistStream stream)
        {
            stream.Save("ID", this.ID);
            if (HasSID)
            {
                stream.Save("SID", this.SID);
            }

            stream.Save("Title", _title);
            stream.Save("DatasetID", _datasetID);

            stream.Save("Namespace", _namespace);

            stream.Save("visible", _visible);
            stream.Save("MinimumScale", _minimumScale);
            stream.Save("MaximumScale", _maximumScale);

            stream.Save("MinimumLabelScale", _minimumLabelScale);
            stream.Save("MaximumLabelScale", _maximumLabelScale);

            stream.Save("MaximumZoomToFeatureScale", _MaximumZoomToFeatureScale);
        }

        #endregion
    }
}

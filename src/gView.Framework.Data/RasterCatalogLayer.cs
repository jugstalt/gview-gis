using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.GraphicsEngine.Filters;

namespace gView.Framework.Data
{
    public class RasterCatalogLayer : FeatureLayer2, IRasterCatalogLayer
    {
        private InterpolationMethod _interpolMethod = InterpolationMethod.Fast;
        private float _transparency = 0.0f;
        private GraphicsEngine.ArgbColor _transColor = GraphicsEngine.ArgbColor.Transparent;
        private FilterImplementations _filter = FilterImplementations.Default;

        public RasterCatalogLayer() { }
        public RasterCatalogLayer(IRasterCatalogClass rasterClass) :
            base(rasterClass as IFeatureClass)
        {
            _class = rasterClass;
            if (rasterClass != null)
            {
                _title = rasterClass.Name;
            }
        }
        public RasterCatalogLayer(IRasterCatalogLayer layer)
            : base(layer as IFeatureLayer)
        {
            CopyFrom(layer);
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);

            if (element is IRasterLayer)
            {
                IRasterLayer layer = (IRasterLayer)element;

                _interpolMethod = layer.InterpolationMethod;
                _transparency = layer.Opacity;
                _transColor = layer.TransparentColor;
            }
        }

        #region IRasterLayer Member

        public InterpolationMethod InterpolationMethod
        {
            get
            {
                return _interpolMethod;
            }
            set
            {
                _interpolMethod = value;
            }
        }

        public float Opacity
        {
            get
            {
                return _transparency;
            }
            set
            {
                _transparency = value;
            }
        }

        public GraphicsEngine.ArgbColor TransparentColor
        {
            get
            {
                return _transColor;
            }
            set
            {
                _transColor = value;
            }
        }

        public GraphicsEngine.Filters.FilterImplementations FilterImplementation
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
            }
        }

        public IRasterClass RasterClass
        {
            get { return _class as IRasterClass; }
        }

        #endregion

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        override public void Load(IPersistStream stream)
        {
            base.Load(stream);

            _interpolMethod = (InterpolationMethod)stream.Load("interpolation", (int)InterpolationMethod.Fast);
            _transparency = (float)stream.Load("transparency", 0f);
            _transColor = GraphicsEngine.ArgbColor.FromArgb((int)stream.Load("transcolor", GraphicsEngine.ArgbColor.Transparent.ToArgb()));
            _filter = (FilterImplementations)stream.Load("filter", (int)FilterImplementations.Default);
        }

        override public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("interpolation", (int)_interpolMethod);
            stream.Save("transparency", _transparency);
            stream.Save("transcolor", _transColor.ToArgb());
            stream.Save("filter", (int)_filter);
        }

        #endregion
    }
}

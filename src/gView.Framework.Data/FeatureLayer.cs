using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Data.Filters;
using System;

namespace gView.Framework.Data
{
    public class FeatureLayer : Layer, IFeatureLayer, IFeatureLayerComposition
    {
        protected IFeatureRenderer _renderer = null, _selectionrenderer = null;
        protected ILabelRenderer _labelRenderer = null;
        protected IQueryFilter _filterQuery = null;
        protected FieldCollection _fields = new FieldCollection();
        protected bool _applyRefScale = true, _applyLabelRefScale = true;
        protected FeatureLayerJoins _joins = null;
        protected GeometryType _geometryType = GeometryType.Unknown;

        public FeatureLayer()
        {
        }
        public FeatureLayer(IFeatureClass featureClass)
        {
            this.FeatureClass = featureClass;
            if (featureClass != null)
            {
                _title = _class.Name;

                //if (featureClass.Fields != null)
                //{
                //    _fields = new Fields();
                //    foreach (IField field in featureClass.Fields)
                //    {
                //        Field f = new Field(field);
                //        _fields.Add(f);
                //        if (_fields.PrimaryDisplayField == null && f.type == FieldType.String)
                //        {
                //            _fields.PrimaryDisplayField = f;
                //        }
                //    }
                //    if (_fields.PrimaryDisplayField == null)
                //    {
                //        foreach (IField field in _fields)
                //        {
                //            if (field.type != FieldType.Shape &&
                //                field.type != FieldType.binary &&
                //                field.type != FieldType.unknown)
                //            {
                //                _fields.PrimaryDisplayField = field;
                //                break;
                //            }
                //        }
                //    }
                //}
            }
        }

        public FeatureLayer(IFeatureLayer layer)
        {
            CopyFrom(layer);
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);

            if (element is IFeatureLayer)
            {
                IFeatureLayer layer = (IFeatureLayer)element;

                if (layer == null)
                {
                    return;
                }

                _renderer = layer.FeatureRenderer;
                _selectionrenderer = layer.SelectionRenderer;
                _labelRenderer = layer.LabelRenderer;
                _filterQuery = layer.FilterQuery;

                _applyRefScale = layer.ApplyRefScale;
                _applyLabelRefScale = layer.ApplyLabelRefScale;

                this.MaxRefScaleFactor = layer.MaxRefScaleFactor;
                this.MaxLabelRefScaleFactor = layer.MaxLabelRefScaleFactor;

                _fields.CopyFrom(layer.Fields, this.Class);
                _geometryType = layer.LayerGeometryType;

                if (layer.Joins != null)
                {
                    this.Joins = (FeatureLayerJoins)layer.Joins.Clone();
                }

                if (layer is IFeatureLayerComposition)
                {
                    this.CompositionMode = ((IFeatureLayerComposition)layer).CompositionMode;
                    this.CompositionModeCopyTransparency = ((IFeatureLayerComposition)layer).CompositionModeCopyTransparency;
                }
            }
        }

        private void RefreshFields()
        {
            FieldCollection newFields = new FieldCollection();
            if (_class is ITableClass)
            {
                foreach (IField field in ((ITableClass)_class).Fields?.ToEnumerable() ?? Array.Empty<IField>())
                {
                    Field f = new Field(field);
                    newFields.Add(f);
                    if (newFields.PrimaryDisplayField == null && f.type == FieldType.String)
                    {
                        newFields.PrimaryDisplayField = f;
                    }
                }
                if (newFields.PrimaryDisplayField == null && _fields != null)
                {
                    foreach (IField field in _fields.ToEnumerable())
                    {
                        if (field.type != FieldType.Shape &&
                            field.type != FieldType.binary &&
                            field.type != FieldType.unknown)
                        {
                            newFields.PrimaryDisplayField = field;
                            break;
                        }
                    }
                }

                if (_fields != null)
                {
                    foreach (IField field in _fields.ToEnumerable())
                    {
                        IField f = newFields.FindField(field.name);
                        if (f != null)
                        {
                            ((Field)f).aliasname = field.aliasname;
                            ((Field)f).IsEditable = field.IsEditable;
                            ((Field)f).IsRequired = field.IsRequired;
                            ((Field)f).DefautValue = field.DefautValue;
                            ((Field)f).Domain = field.Domain;
                            f.visible = field.visible;
                        }
                    }

                    if (_fields.PrimaryDisplayField != null)
                    {
                        IField f = newFields.FindField(_fields.PrimaryDisplayField.name);
                        if (f != null)
                        {
                            newFields.PrimaryDisplayField = f;
                        }
                    }
                }
            }
            _fields = newFields;
        }

        protected override void Refresh()
        {
            this.FeatureClass = (IFeatureClass)_class; // Refresh WrappedClass!
        }

        #region IFeatureLayer Member

        public IFeatureRenderer FeatureRenderer
        {
            get { return _renderer; }
            set { _renderer = value; }
        }
        public IFeatureRenderer SelectionRenderer
        {
            get { return _selectionrenderer; }
            set { _selectionrenderer = value; }
        }
        public ILabelRenderer LabelRenderer
        {
            get { return _labelRenderer; }
            set { _labelRenderer = value; }
        }
        virtual public IFeatureClass FeatureClass
        {
            get { return _class as IFeatureClass; }
            private set
            {
                _class = value as IFeatureClass;
                if (_class == null)
                {
                    return;
                }

                if (_joins != null)
                {
                    if (_class is WrappedFeatureClassWithJoins)
                    {
                        ((WrappedFeatureClassWithJoins)_class).Joins = _joins;
                    }
                    else
                    {
                        _class = new WrappedFeatureClassWithJoins((IFeatureClass)_class, _joins);
                    }
                }
                else
                {
                    if (_class is WrappedFeatureClassWithJoins)
                    {
                        _class = ((WrappedFeatureClassWithJoins)_class).WrappedFeatureclass;
                    }
                }

                if (_class is IFeatureClass &&
                    ((IFeatureClass)_class).GeometryType != GeometryType.Unknown)
                {
                    _geometryType = GeometryType.Unknown;
                }

                RefreshFields();
            }
        }
        public IQueryFilter FilterQuery
        {
            get { return _filterQuery; }
            set { _filterQuery = value; }
        }

        public bool ApplyRefScale
        {
            get
            {
                return _applyRefScale;
            }
            set
            {
                _applyRefScale = value;
            }
        }
        public bool ApplyLabelRefScale
        {
            get
            {
                return _applyLabelRefScale;
            }
            set
            {
                _applyLabelRefScale = value;
            }
        }

        public float MaxRefScaleFactor { get; set; }
        public float MaxLabelRefScaleFactor { get; set; }

        public IFieldCollection Fields
        {
            get { return _fields; }
        }

        public FeatureLayerJoins Joins
        {
            get { return _joins; }
            set
            {
                _joins = value;
                if (_joins != null)
                {
                    this.FeatureClass = (IFeatureClass)_class; // Refresh WrappedClass!
                }
            }
        }

        public GeometryType LayerGeometryType
        {
            get
            {
                if (this.FeatureClass != null && this.FeatureClass.GeometryType != GeometryType.Unknown)
                {
                    return this.FeatureClass.GeometryType;
                }

                return _geometryType;
            }
            set { _geometryType = value; }
        }

        #endregion

        #region IFeatureLayerComposition

        public FeatureLayerCompositionMode CompositionMode { get; set; }
        public float CompositionModeCopyTransparency { get; set; }

        #endregion

        #region IPersistable Member

        override public void Load(IPersistStream stream)
        {
            base.Load(stream);

            //_featureclass=new FeatureClass();
            //((FeatureClass)_featureclass).Name=(string)stream.Load("Featureclass","");

            _renderer = (IFeatureRenderer)stream.Load("IRenderer");
            _labelRenderer = (ILabelRenderer)stream.Load("ILabelRenderer");
            _selectionrenderer = (IFeatureRenderer)stream.Load("ISelectionRenderer");
            _fields = stream.Load("IFields", new FieldCollection(), new FieldCollection()) as FieldCollection;

            _applyRefScale = (bool)stream.Load("applyRefScale", true);
            _applyLabelRefScale = (bool)stream.Load("applyLRefScale", true);

            this.MaxRefScaleFactor = (float)stream.Load("maxRefScaleFactor", 0f);
            this.MaxLabelRefScaleFactor = (float)stream.Load("maxLabelRefScaleFactor", 0f);

            _geometryType = (GeometryType)stream.Load("geomType", (int)GeometryType.Unknown);

            string filterQuery = (string)stream.Load("FilterQuery", "");
            string filterOrderBy = (string)stream.Load("FilterOrderBy", "");
            if (!String.IsNullOrEmpty(filterQuery) || !String.IsNullOrEmpty(filterOrderBy))
            {
                QueryFilter filter = new QueryFilter();
                filter.WhereClause = filterQuery;
                filter.OrderBy = filterOrderBy;
                this.FilterQuery = filter;
            }

            this.Joins = (FeatureLayerJoins)stream.Load("Joins", null, new FeatureLayerJoins());

            if (this is IFeatureLayerComposition)
            {
                var flComposition = (IFeatureLayerComposition)this;
                flComposition.CompositionMode = (FeatureLayerCompositionMode)(int)stream.Load("composition_mode", (int)FeatureLayerCompositionMode.Over);
                flComposition.CompositionModeCopyTransparency = (float)stream.Load("compostion_copy_transp", 1f);
            }
        }

        override public void Save(IPersistStream stream)
        {
            base.Save(stream);

            //stream.Save("Featureclass", FeatureClass != null ? FeatureClass.Name : _title);

            if (_renderer != null)
            {
                stream.Save("IRenderer", _renderer);
            }

            if (_labelRenderer != null)
            {
                stream.Save("ILabelRenderer", _labelRenderer);
            }

            if (_selectionrenderer != null)
            {
                stream.Save("ISelectionRenderer", _selectionrenderer);
            }

            if (_fields != null)
            {
                stream.Save("IFields", _fields);
            }

            if (_applyRefScale == false)
            {
                stream.Save("applyRefScale", _applyRefScale);
            }

            if (_applyLabelRefScale == false)
            {
                stream.Save("applyLRefScale", _applyLabelRefScale);
            }

            if (this.MaxRefScaleFactor > 0f)
            {
                stream.Save("maxRefScaleFactor", this.MaxRefScaleFactor);
            }

            if (this.MaxLabelRefScaleFactor > 0f)
            {
                stream.Save("maxLabelRefScaleFactor", this.MaxLabelRefScaleFactor);
            }

            if (_geometryType != GeometryType.Unknown)
            {
                stream.Save("geomType", (int)_geometryType);
            }

            if (this.FilterQuery != null)
            {
                if (!String.IsNullOrEmpty(this.FilterQuery.JsonWhereClause))
                {
                    stream.Save("FilterQuery", this.FilterQuery.JsonWhereClause);
                }
                else
                {
                    stream.Save("FilterQuery", this.FilterQuery.WhereClause);
                }
                stream.Save("FilterOrderBy", this.FilterQuery.OrderBy);
            }

            if (_joins != null)
            {
                stream.Save("Joins", _joins);
            }

            if (this is IFeatureLayerComposition)
            {
                var flComposition = (IFeatureLayerComposition)this;
                stream.Save("composition_mode", (int)flComposition.CompositionMode);
                stream.Save("compostion_copy_transp", flComposition.CompositionModeCopyTransparency);
            }
        }

        #endregion
    }
}

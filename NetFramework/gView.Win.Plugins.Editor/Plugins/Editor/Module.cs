using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Editor.Core;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology;
using gView.Framework.system;
using gView.Framework.UI;
using gView.GraphicsEngine;
using gView.Plugins.Editor.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.Editor
{
    [gView.Framework.system.RegisterPlugIn(Globals.ModuleGuidString, PluginUsage.Desktop)]
    public class Module : IMapApplicationModule, IModule, IPersistable
    {
        public enum EditTask { None = 0, CreateNewFeature = 1, ModifyFeature = 2 }

        public delegate void OnChangeSelectedFeatureEventHandler(Module sender, IFeature feature);
        public event OnChangeSelectedFeatureEventHandler OnChangeSelectedFeature = null;

        public delegate void OnChangeEditTaskEventHandler(Module sender, EditTask task);
        public event OnChangeEditTaskEventHandler OnChangeEditTask = null;

        public delegate void OnCreateStandardFeatureEventHandler(Module sender, IFeature feature);
        public event OnCreateStandardFeatureEventHandler OnCreateStandardFeature = null;

        private IMapDocument _doc = null;
        private string _lastMsg = String.Empty;
        private IFeatureClass _fc = null;
        private IFeature _feature = null;
        private IGraphicsContainer _sketchContainer, _moverContainer;
        private EditTask _task = EditTask.CreateNewFeature;
        private FormAttributeEditor _attributEditor;
        private List<IEditLayer> _editLayers = new List<IEditLayer>();
        private IEditLayer _selectedEditLayer = null;

        public Module()
        {
            _sketchContainer = new gView.Framework.Carto.GraphicsContainer();
            _moverContainer = new gView.Framework.Carto.GraphicsContainer();

            _moverContainer.Elements.Add(new MoverGraphics(this));
            _attributEditor = new FormAttributeEditor(this);
        }

        public EditTask ActiveEditTask
        {
            get { return _task; }
            internal set
            {
                if (value != _task)
                {
                    _task = value;
                    if (OnChangeEditTask != null)
                    {
                        OnChangeEditTask(this, _task);
                    }

                    this.Feature = null;
                }
            }
        }

        public IDockableWindow AttributeEditorWindow
        {
            get
            {
                return _attributEditor;
            }
        }

        #region IMapApplicationModule Member

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                if (_doc != null)
                {
                    _doc.LayerAdded -= new LayerAddedEvent(_doc_LayerAdded);
                    _doc.LayerRemoved -= new LayerRemovedEvent(_doc_LayerRemoved);
                    _doc.MapAdded -= new MapAddedEvent(_doc_MapAdded);
                    _doc.MapDeleted -= new MapDeletedEvent(_doc_MapDeleted);
                    if (_doc.Application is IMapApplication)
                    {
                        ((IMapApplication)_doc.Application).AfterLoadMapDocument -= new AfterLoadMapDocumentEvent(Module_AfterLoadMapDocument);
                    }
                }
                _doc = (IMapDocument)hook;

                _doc.LayerAdded += new LayerAddedEvent(_doc_LayerAdded);
                _doc.LayerRemoved += new LayerRemovedEvent(_doc_LayerRemoved);
                _doc.MapAdded += new MapAddedEvent(_doc_MapAdded);
                _doc.MapDeleted += new MapDeletedEvent(_doc_MapDeleted);
                if (_doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).AfterLoadMapDocument += new AfterLoadMapDocumentEvent(Module_AfterLoadMapDocument);
                }
            }
        }
        #endregion

        #region Fire Events
        public bool PerformBeginEditFeature(IFeatureClass fc, IFeature feature)
        {
            EditorEventArgument args = new EditorEventArgument(fc, feature);
            if (OnBeginEditFeature != null)
            {
                OnBeginEditFeature(this, args);
            }

            if (args.Cancel)
            {
                _lastMsg = args.Message;
                return false;
            }

            return true;
        }

        public bool PerformCreateFeature(IFeatureClass fc, IFeature feature)
        {
            EditorEventArgument args = new EditorEventArgument(fc, feature);
            if (OnCreateFeature != null)
            {
                OnCreateFeature(this, args);
            }

            if (args.Cancel)
            {
                _lastMsg = args.Message;
                return false;
            }

            return true;
        }

        async public Task<bool> PerformInsertFeature(IFeatureClass fc, IFeature feature)
        {
            if (fc == null || feature == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureUpdater))
            {
                return false;
            }

            if (_attributEditor != null && !_attributEditor.CommitValues())
            {
                MessageBox.Show(_attributEditor.LastErrorMessage, "ERROR: Commit Values...");
                return false;
            }

            EditorEventArgument args = new EditorEventArgument(fc, feature);
            if (OnInsertFeature != null)
            {
                OnInsertFeature(this, args);
            }

            if (args.Cancel)
            {
                _lastMsg = args.Message;
                return false;
            }

            IGeometry shape = feature.Shape;
            if (_doc != null &&
                _doc.FocusMap != null &&
                _doc.FocusMap.Display != null &&
                _doc.FocusMap.Display.SpatialReference != null &&
                !_doc.FocusMap.Display.SpatialReference.Equals(_fc.SpatialReference))
            {
                feature.Shape = GeometricTransformerFactory.Transform2D(
                    feature.Shape,
                    _doc.FocusMap.Display.SpatialReference,
                    _fc.SpatialReference);
            }
            bool ret = await ((IFeatureUpdater)fc.Dataset.Database).Insert(fc, feature);
            feature.Shape = shape;

            if (!ret)
            {
                _lastMsg = fc.Dataset.Database.LastErrorMessage;
            }

            return ret;
        }

        async public Task<bool> PerformUpdateFeature(IFeatureClass fc, IFeature feature)
        {
            if (fc == null || feature == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureUpdater))
            {
                return false;
            }

            if (_attributEditor != null && !_attributEditor.CommitValues())
            {
                MessageBox.Show(_attributEditor.LastErrorMessage, "ERROR: Commit Values...");
                return false;
            }

            EditorEventArgument args = new EditorEventArgument(fc, feature);
            if (OnUpdateFeature != null)
            {
                OnUpdateFeature(this, args);
            }

            if (args.Cancel)
            {
                _lastMsg = args.Message;
                return false;
            }

            IGeometry shape = feature.Shape;
            if (_doc != null &&
                _doc.FocusMap != null &&
                _doc.FocusMap.Display != null &&
                _doc.FocusMap.Display.SpatialReference != null &&
                !_doc.FocusMap.Display.SpatialReference.Equals(_fc.SpatialReference))
            {
                feature.Shape = GeometricTransformerFactory.Transform2D(
                    feature.Shape,
                    _doc.FocusMap.Display.SpatialReference,
                    _fc.SpatialReference);
            }
            bool ret = await ((IFeatureUpdater)fc.Dataset.Database).Update(fc, feature);
            feature.Shape = shape;

            if (!ret)
            {
                _lastMsg = fc.Dataset.Database.LastErrorMessage;
            }

            return ret;
        }

        async public Task<bool> PerformDeleteFeature(IFeatureClass fc, IFeature feature)
        {
            if (fc == null || feature == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureUpdater))
            {
                return false;
            }

            EditorEventArgument args = new EditorEventArgument(fc, feature);
            if (OnDeleteFeature != null)
            {
                OnDeleteFeature(this, args);
            }

            if (args.Cancel)
            {
                _lastMsg = args.Message;
                return false;
            }

            bool ret = await ((IFeatureUpdater)fc.Dataset.Database).Delete(fc, feature.OID);
            if (!ret)
            {
                _lastMsg = fc.Dataset.Database.LastErrorMessage;
            }

            return ret;
        }
        #endregion

        #region IModuleEvents Member

        public event OnBeginEditFeatureEventHandler OnBeginEditFeature = null;

        public event OnCreateFeatureEventHandler OnCreateFeature = null;

        public event OnInsertFeatureEventHandler OnInsertFeature = null;

        public event OnUpdateFeatureEventHandler OnUpdateFeature = null;

        public event OnDeleteFeatureEventHandler OnDeleteFeature = null;

        public event OnEditLayerCollectionChangedEventHandler OnEditLayerCollectionChanged = null;
        #endregion

        #region Properties
        internal IFeatureClass FeatureClass
        {
            get { return _fc; }
        }
        internal IFeature Feature
        {
            get { return _feature; }
            set
            {
                _feature = value;

                if (_feature == null || _feature.Shape == null)
                {
                    Sketch = null;
                }
                else
                {
                    Sketch = new EditSketch(_feature.Shape);
                }

                if (OnChangeSelectedFeature != null)
                {
                    OnChangeSelectedFeature(this, _feature);
                }
            }
        }
        internal string LastMessage
        {
            get { return _lastMsg; }
        }
        //private void SetFeatureClassAndFeature(IFeatureClass fc, IFeature feature)
        //{
        //    if (_fc == fc && _feature == feature) return;

        //    _fc = fc;
        //    _feature = feature;

        //    if (_feature == null || feature.Shape == null)
        //        Sketch = null;
        //    else
        //        Sketch = new EditSketch(feature.Shape);

        //    if (OnChangeSelectedFeature != null)
        //        OnChangeSelectedFeature(this, fc, feature);
        //}
        internal void CreateStandardFeature()
        {
            if (_fc == null)
            {
                return;
            }

            Feature feature = new Feature();

            GeometryType gType = _fc.GeometryType;
            if (gType == GeometryType.Unknown)
            {
                FormChooseGeometry dlg = new FormChooseGeometry();
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    this.Sketch = null;
                    return;
                }
                gType = dlg.GeometryType;
            }
            switch (gType)
            {
                case GeometryType.Point:
                    feature.Shape = new gView.Framework.Geometry.Point();
                    break;
                case GeometryType.Multipoint:
                    feature.Shape = new MultiPoint();
                    break;
                case GeometryType.Polyline:
                    feature.Shape = new Polyline();
                    break;
                case GeometryType.Polygon:
                    feature.Shape = new Polygon();
                    break;
                default:
                    return;
            }
            _feature = feature;

            if (OnCreateStandardFeature != null)
            {
                OnCreateStandardFeature(this, _feature);
            }

            Sketch = new EditSketch(_feature.Shape);
        }
        internal IMapDocument MapDocument
        {
            get { return _doc; }
        }
        internal List<IEditLayer> EditLayers
        {
            get
            {
                return ListOperations<IEditLayer>.Clone(_editLayers);
            }
        }
        internal IEditLayer SelectedEditLayer
        {
            get
            {
                return _selectedEditLayer;
            }
            set
            {
                if (!_editLayers.Contains(value))
                {
                    return;
                }

                _selectedEditLayer = value;
                if (_selectedEditLayer == null || _selectedEditLayer.FeatureLayer == null)
                {
                    _fc = null;
                }
                else
                {
                    _fc = _selectedEditLayer.FeatureLayer.FeatureClass;
                }

                this.Feature = null;
            }
        }

        internal void FireOnEditLayerCollectionChanged()
        {
            if (OnEditLayerCollectionChanged != null)
            {
                OnEditLayerCollectionChanged(this);
            }
        }
        #endregion

        #region Sketch Properties
        internal EditSketch Sketch
        {
            get
            {
                if (_sketchContainer.Elements.Count == 1)
                {
                    return _sketchContainer.Elements[0] as EditSketch;
                }

                return null;
            }
            set
            {
                if (_doc == null || _doc.FocusMap == null ||
                    _doc.FocusMap.Display == null || _doc.FocusMap.Display.GraphicsContainer == null)
                {
                    return;
                }

                foreach (IGraphicElement grElement in _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Clone())
                {
                    if (grElement is EditSketch)
                    {
                        _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Remove(grElement);
                    }
                }

                foreach (IGraphicElement grElement in _doc.FocusMap.Display.GraphicsContainer.Elements.Clone())
                {
                    if (grElement is EditSketch)
                    {
                        _doc.FocusMap.Display.GraphicsContainer.Elements.Remove(grElement);
                    }
                }

                EditSketch sketch = null;
                if (_sketchContainer.Elements.Count == 1)
                {
                    sketch = _sketchContainer.Elements[0] as EditSketch;
                }
                // hat sich sketch nicht verändert...
                if (value != null && value.Equals(sketch))
                {
                    return;
                }

                if (value == null && sketch == null)
                {
                    return;
                }

                _sketchContainer.Elements.Clear();
                if (value != null)
                {
                    _sketchContainer.Elements.Add(value);
                    _doc.FocusMap.Display.GraphicsContainer.Elements.Add(value);
                    _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Add(value);
                }

                // neu zeichnen, wenn nicht gerade Afterloadmap läuft
                // und Sketch sich geändert hat;
                if (_doc.Application is IMapApplication &&
                    !_afterLoadMapDocument)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
                }
            }
        }

        internal void RedrawhSketch()
        {
            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
            {
                _doc.FocusMap.Display.DrawOverlay(_sketchContainer, true);
            }
        }

        internal IPoint Mover
        {
            set
            {
                if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
                {
                    ((MoverGraphics)_moverContainer.Elements[0]).Mover = value;
                    if (((MoverGraphics)_moverContainer.Elements[0]).IsValid)
                    {
                        _doc.FocusMap.Display.DrawOverlay(_moverContainer, true);
                    }
                }
            }
        }
        #endregion

        #region HelperClasses
        private class MoverGraphics : IGraphicElement
        {
            private SimpleLineSymbol _symbol;
            private Module _module;
            private IPoint _mover = null;

            public MoverGraphics(Module module)
            {
                _module = module;

                _symbol = new SimpleLineSymbol();
                _symbol.Color = ArgbColor.Gray;
            }

            public IPoint Mover
            {
                get { return _mover; }
                set { _mover = value; }
            }

            #region IGraphicElement Member

            public void Draw(IDisplay display)
            {
                if (_mover == null || _module == null || _module.Sketch == null)
                {
                    return;
                }

                switch (_module.Sketch.GeometryType)
                {
                    case GeometryType.Polyline:
                        IPointCollection pColl1 = _module.Sketch.Part;
                        if (pColl1 == null || pColl1.PointCount == 0)
                        {
                            return;
                        }

                        IPolyline pLine1 = new Polyline();
                        IPath path1 = new Path();
                        pLine1.AddPath(path1);

                        path1.AddPoint(pColl1[pColl1.PointCount - 1]);
                        path1.AddPoint(_mover);

                        display.Draw(_symbol, pLine1);
                        break;
                    case GeometryType.Polygon:
                        IPointCollection pColl2 = _module.Sketch.Part;
                        if (pColl2 == null || pColl2.PointCount < 2)
                        {
                            return;
                        }

                        IPolyline pLine2 = new Polyline();
                        IPath path2 = new Path();
                        pLine2.AddPath(path2);

                        path2.AddPoint(pColl2[pColl2.PointCount - 1]);
                        path2.AddPoint(_mover);
                        path2.AddPoint(pColl2[0]);

                        display.Draw(_symbol, pLine2);
                        break;
                }
            }

            #endregion

            public bool IsValid
            {
                get
                {
                    if (_mover == null || _module == null || _module.Sketch == null)
                    {
                        return false;
                    }

                    switch (_module.Sketch.GeometryType)
                    {
                        case GeometryType.Point:
                            return false;
                        case GeometryType.Polyline:
                            IPointCollection pColl1 = _module.Sketch.Part;
                            if (pColl1 == null || pColl1.PointCount == 0)
                            {
                                return false;
                            }

                            break;
                        case GeometryType.Polygon:
                            IPointCollection pColl2 = _module.Sketch.Part;
                            if (pColl2 == null || pColl2.PointCount < 2)
                            {
                                return false;
                            }

                            break;
                    }

                    return true;
                }
            }
        }
        private class MapEditLayerPersist : IPersistable
        {
            private Module _module;
            private IMap _map;

            public MapEditLayerPersist(Module module, IMap map)
            {
                _module = module;
                _map = map;
            }

            public Module Module { get { return _module; } }
            public IMap Map { get { return _map; } }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                if (stream == null ||
                    _module == null ||
                    _module.MapDocument == null)
                {
                    return;
                }

                int index = (int)stream.Load("index", -1);
                if (index == -1 || index >= _module.MapDocument.Maps.Count())
                {
                    return;
                }

                string name = (string)stream.Load("name", string.Empty);

                var mapsList = _module.MapDocument.Maps.ToList();
                if (mapsList[index] == null ||
                    mapsList[index].Name != name)
                {
                    return;
                }

                _map = mapsList[index];

                EditLayer eLayer;
                while ((eLayer = (EditLayer)stream.Load("EditLayer", null, new EditLayer())) != null)
                {
                    if (eLayer.SetLayer(_map))
                    {
                        _module.AddEditLayer(eLayer);
                    }
                }
            }

            public void Save(IPersistStream stream)
            {
                if (stream == null ||
                    _module == null ||
                    _module.MapDocument == null ||
                    _map == null)
                {
                    return;
                }

                int index = _module.MapDocument.Maps.ToList().IndexOf(_map);
                if (index == -1)
                {
                    return;
                }

                stream.Save("index", index);
                stream.Save("name", _map.Name);
                foreach (IEditLayer editLayer in _module.EditLayers)
                {
                    if (editLayer == null)
                    {
                        continue;
                    }

                    stream.Save("EditLayer", editLayer);
                }
            }

            #endregion
        }
        #endregion

        #region Helper
        static internal void SetValueOrAppendFieldValueIfNotExist(IFeature feature, string fieldName, object val)
        {
            if (feature == null || feature.Fields == null)
            {
                return;
            }

            foreach (IFieldValue fv in feature.Fields)
            {
                if (fv == null)
                {
                    continue;
                }

                if (fv.Name == fieldName)
                {
                    fv.Value = val;
                    return;
                }
            }
            if (val != null && String.IsNullOrEmpty(val.ToString()))
            {
                // wenn nix im Attributeditor steht, soll objekt auch 
                // nicht zwanghaft angelegt werden.
                // Sonst gibt oft Probleme beim neuen Anlegen von
                // Features (Datenbanktypen nicht verträglich...)
                return;
            }
            feature.Fields.Add(new FieldValue(fieldName, val));
        }
        internal IFeatureLayer GetFeatureClassLayer(IFeatureClass fc)
        {
            if (_doc == null || _doc.FocusMap == null)
            {
                return null;
            }

            foreach (IDatasetElement element in _doc.FocusMap.MapElements)
            {
                if (element is IFeatureLayer && element.Class == fc)
                {
                    return element as IFeatureLayer;
                }
            }
            return null;
        }
        private IEditLayer EditLayerByFeatureLayer(IFeatureLayer layer)
        {
            if (_editLayers == null)
            {
                return null;
            }

            foreach (IEditLayer editLayer in _editLayers)
            {
                if (editLayer != null && editLayer.FeatureLayer == layer)
                {
                    return editLayer;
                }
            }
            return null;
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _editLayers.Clear();
            if (_doc == null || stream == null)
            {
                return;
            }

            MapEditLayerPersist mapEditLayers;
            while ((mapEditLayers = (MapEditLayerPersist)stream.Load("MapEditLayers", null, new MapEditLayerPersist(this, null))) != null)
            {
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_doc == null || _doc.Maps == null)
            {
                return;
            }

            foreach (IMap map in _doc.Maps)
            {
                if (map == null)
                {
                    continue;
                }

                stream.Save("MapEditLayers", new MapEditLayerPersist(this, map));
            }
        }

        #endregion

        #region Document Events
        void _doc_MapDeleted(IMap map)
        {
            if (map == null)
            {
                return;
            }

            bool found = false;
            foreach (IEditLayer editLayer in ListOperations<IEditLayer>.Clone(_editLayers))
            {
                if (editLayer.FeatureLayer == null)
                {
                    _editLayers.Remove(editLayer);
                    found = true;
                }
                if (map[editLayer.FeatureLayer] != null)
                {
                    _editLayers.Remove(editLayer);
                    found = true;
                }
            }

            if (found &&
                OnEditLayerCollectionChanged != null)
            {
                OnEditLayerCollectionChanged(this);
            }
        }

        void _doc_MapAdded(IMap map)
        {

        }

        void _doc_LayerRemoved(IMap sender, ILayer layer)
        {
            if (sender == null || layer == null || _doc == null)
            {
                return;
            }

            bool found = false;
            foreach (IEditLayer editLayer in ListOperations<IEditLayer>.Clone(_editLayers))
            {
                if (editLayer.FeatureLayer == null)
                {
                    _editLayers.Remove(editLayer);
                    found = true;
                }
                else if (editLayer.FeatureLayer == layer)
                {
                    _editLayers.Remove(editLayer);
                    found = true;
                }
            }

            if (found &&
                OnEditLayerCollectionChanged != null)
            {
                OnEditLayerCollectionChanged(this);
            }
        }

        void _doc_LayerAdded(IMap sender, ILayer layer)
        {
            if (!(layer is IFeatureLayer))
            {
                return;
            }

            IFeatureLayer fl = (IFeatureLayer)layer;
            if (fl.Class == null || fl.Class.Dataset == null ||
                !(fl.Class.Dataset.Database is IEditableDatabase))
            {
                return;
            }

            EditLayer editLayer = new EditLayer(fl, EditStatements.NONE);

            _editLayers.Add(editLayer);
            if (OnEditLayerCollectionChanged != null)
            {
                OnEditLayerCollectionChanged(this);
            }
        }
        private bool _afterLoadMapDocument = false;
        void Module_AfterLoadMapDocument(IMapDocument mapDocument)
        {
            if (_doc != mapDocument)
            {
                OnCreate(mapDocument);
            }

            if (_doc == null)
            {
                return;
            }

            _afterLoadMapDocument = true;
            foreach (IMap map in _doc.Maps)
            {
                foreach (IDatasetElement element in map.MapElements)
                {
                    if (!(element is IFeatureLayer))
                    {
                        continue;
                    }

                    if (EditLayerByFeatureLayer(element as IFeatureLayer) == null)
                    {
                        _editLayers.Add(new EditLayer(element as IFeatureLayer, EditStatements.NONE));
                    }
                }
            }

            if (OnEditLayerCollectionChanged != null)
            {
                OnEditLayerCollectionChanged(this);
            }

            _afterLoadMapDocument = false;
        }
        #endregion

        private void AddEditLayer(IEditLayer eLayer)
        {
            if (eLayer != null)
            {
                _editLayers.Add(eLayer);
            }
        }
    }

    class EditLayer : IEditLayer, IPersistable
    {
        private IFeatureLayer _layer;
        private EditStatements _statements;

        internal EditLayer()
            : this(null, EditStatements.NONE)
        {
            persistLayerID = -1;
            persistClassName = String.Empty;
        }

        public EditLayer(IFeatureLayer layer, EditStatements statements)
        {
            _layer = layer;
            _statements = statements;
        }

        private int persistLayerID;
        private string persistClassName;
        internal bool SetLayer(IMap map)
        {
            if (map == null || map.MapElements == null)
            {
                return false;
            }

            foreach (IDatasetElement element in map.MapElements)
            {
                if (!(element is IFeatureLayer) || element.Class == null)
                {
                    continue;
                }

                if (element.ID == persistLayerID && element.Class.Name == persistClassName)
                {
                    _layer = (IFeatureLayer)element;
                    persistLayerID = -1;
                    persistClassName = String.Empty;
                    return true;
                }
            }

            return false;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            persistLayerID = (int)stream.Load("id", -1);
            persistClassName = (string)stream.Load("class", String.Empty);
            _statements = (EditStatements)stream.Load("statement", (int)EditStatements.NONE);
        }

        public void Save(IPersistStream stream)
        {
            if (_layer == null || _layer.Class == null)
            {
                return;
            }

            stream.Save("id", _layer.ID);
            stream.Save("class", _layer.Class.Name);
            stream.Save("statement", (int)_statements);
        }

        #endregion

        #region IEditLayer Member

        public IFeatureLayer FeatureLayer
        {
            get { return _layer; }
        }

        public EditStatements Statements
        {
            get { return _statements; }
            internal set { _statements = value; }
        }

        public int LayerId { get { return _layer.ID; } }
        public string ClassName { get { return _layer?.Class?.Name; } }

        #endregion
    }
}

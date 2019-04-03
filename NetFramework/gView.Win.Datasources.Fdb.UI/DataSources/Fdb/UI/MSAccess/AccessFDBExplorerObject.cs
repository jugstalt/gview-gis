using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.FDB;
using gView.Framework.system.UI;
using gView.Framework.system;
using gView.DataSources.Fdb.UI.MSSql;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.UI.Controls.Filter;

namespace gView.DataSources.Fdb.UI.MSAccess
{
    [gView.Framework.system.RegisterPlugIn("C361EE6D-546A-4ba8-AE8A-F2AD92AC3FC6")]
    public class AccessFDBExplorerObject : ExplorerParentObject, IExplorerFileObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectCreatable
    {
        private AccessFDBIcon _icon = new AccessFDBIcon();
        private string _filename = "", _errMsg = "";

        public AccessFDBExplorerObject() : base(null, null, 0) { }
        public AccessFDBExplorerObject(IExplorerObject parent, string filename)
            : base(parent, null, 0)
        {
            _filename = filename;
        }

        #region IExplorerObject Members

        public string Filter
        {
            get { return "*.mdb"; }
        }

        public string Name
        {
            get
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_filename);
                    return fi.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get
            {
                return _filename;
            }
        }

        public string Type
        {
            get { return "Access Feature Database"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public object Object { get { return null; } }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            string f = filename.ToLower();
            if (f.EndsWith(".jpg.mdb") ||
                f.EndsWith(".png.mdb") ||
                f.EndsWith(".tif.mdb") ||
                f.EndsWith(".tiff.mdb")) return null;

            if (!f.ToLower().EndsWith(".mdb")) return null;

            try
            {
                if (!(new FileInfo(f).Exists)) return null;
                using (AccessFDB fdb = new AccessFDB())
                {
                    if (!fdb.Open(f) || !fdb.IsValidAccessFDB)
                        return null;
                }
            }
            catch { return null; }

            return new AccessFDBExplorerObject(parent, filename);
        }
        #endregion

        private string[] DatasetNames
        {
            get
            {
                try
                {
                    AccessFDB fdb = new AccessFDB();
                    if (!fdb.Open(_filename))
                    {
                        _errMsg = fdb.lastErrorMsg;
                        return null;
                    }
                    string[] ds = fdb.DatasetNames;
                    string[] dsMod = new string[ds.Length];

                    int i = 0;
                    foreach (string dsname in ds)
                    {
                        string imageSpace;
                        if (fdb.IsImageDataset(dsname, out imageSpace))
                        {
                            dsMod[i++] = "#" + dsname;
                        }
                        else
                        {
                            dsMod[i++] = dsname;
                        }
                    }
                    if (ds == null) _errMsg = fdb.lastErrorMsg;
                    fdb.Dispose();

                    return dsMod;
                }
                catch (Exception ex)
                {
                    _errMsg = ex.Message;
                    return null;
                }
            }
        }

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            string[] ds = DatasetNames;
            if (ds != null)
            {
                foreach (string dsname in ds)
                {
                    if (dsname == "") continue;
                    base.AddChildObject(new AccessFDBDatasetExplorerObject(this, _filename, dsname));
                }
            }
        }

        #endregion

        #region IExplorerObjectCommandParameters Members

        public Dictionary<string, string> Parameters
        {
            get
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("AccessFDB", _filename);
                return parameters;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            IExplorerObject obj = (cache.Contains(FullName)) ? cache[FullName] : CreateInstance(null, FullName);
            cache.Append(obj);
            return obj;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject == null) return false;
            return (PlugInManager.PlugInID(parentExObject) == KnownExplorerObjectIDs.Directory);
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "New AccessFDB...";
            dlg.Filter = "MSAccess DB(*.mdb)|*.mdb";
            dlg.FileName = parentExObject.FullName + @"\fdb.mdb";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                AccessFDB fdb = new AccessFDB();
                if (!fdb.Create(dlg.FileName))
                {
                    MessageBox.Show(fdb.lastErrorMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                _filename = dlg.FileName;
                return this;
            }
            return null;
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            try
            {
                FileInfo fi = new FileInfo(_filename);
                fi.Delete();
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("1E4715AB-1BB7-4665-B79C-976FCBA13156")]
    public class AccessFDBDatasetExplorerObject : ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectCreatable, IExplorerObjectContextMenu, IExplorerObjectRenamable
    {
        private IExplorerIcon _icon = null;
        private string _filename = "";
        private ToolStripItem[] _contextItems = null;
        private bool _isImageDataset = false;
        //private AccessFDBDataset _dataset = null;

        public AccessFDBDatasetExplorerObject() : base(null, typeof(AccessFDBDataset)) { }

        public AccessFDBDatasetExplorerObject(IExplorerObject parent, string filename, string dsname)
            : base(parent, typeof(AccessFDBDataset))
        {
            _filename = filename;

            if (dsname.IndexOf("#") == 0)
            {
                _isImageDataset = true;
                dsname = dsname.Substring(1, dsname.Length - 1);
                _icon = new AccessFDBImageDatasetIcon();
            }
            else
            {
                _isImageDataset = false;
                _icon = new AccessFDBDatasetIcon();
            }
            _dsname = dsname;

            _dataset = new AccessFDBDataset();
            _dataset.ConnectionString = "mdb=" + _filename + ";dsname=" + _dsname;
            _dataset.Open();

            _contextItems = new ToolStripItem[2];
            _contextItems[0] = new ToolStripMenuItem("Spatial Reference...");
            _contextItems[0].Click += new EventHandler(SpatialReference_Click);
            _contextItems[1] = new ToolStripMenuItem("Shrink Spatial Indices...");
            _contextItems[1].Click += new EventHandler(ShrinkSpatialIndices_Click);
        }

        void SpatialReference_Click(object sender, EventArgs e)
        {
            if (_dataset == null || _fdb == null)
            {
                Refresh();
                if (_dataset == null || _fdb == null)
                {
                    MessageBox.Show("Can't open dataset...");
                    return;
                }
            }

            FormSpatialReference dlg = new FormSpatialReference(_dataset.SpatialReference);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int id = _fdb.CreateSpatialReference(dlg.SpatialReference);
                if (id == -1)
                {
                    MessageBox.Show("Can't create Spatial Reference!\n", _fdb.lastErrorMsg);
                    return;
                }
                if (!_fdb.SetSpatialReferenceID(_dataset.DatasetName, id))
                {
                    MessageBox.Show("Can't set Spatial Reference!\n", _fdb.lastErrorMsg);
                    return;
                }
                _dataset.SpatialReference = dlg.SpatialReference;
            }
        }

        void ShrinkSpatialIndices_Click(object sender, EventArgs e)
        {
            if (_dataset == null) return;

            List<IClass> classes = new List<IClass>();
            foreach (IDatasetElement element in _dataset.Elements)
            {
                if (element == null || element.Class == null) continue;
                classes.Add(element.Class);
            }

            SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
            rebuilder.RebuildIndices(classes);
        }

        internal bool IsImageDataset
        {
            get { return _isImageDataset; }
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _dsname; }
        }

        public string FullName
        {
            get
            {
                return _filename + ((_filename != "") ? @"\" : "") + _dsname;
            }
        }
        public string Type
        {
            get { return "Access Feature Database Dataset"; }
        }
        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new AccessFDBDatasetIcon();
                return _icon;
            }
        }
        public override void Dispose()
        {
            base.Dispose();

            _fdb = null;
            if (_dataset != null)
            {
                _dataset.Dispose();
                _dataset = null;
            }
        }
        public object Object { get { return _dataset; } }

        #endregion

        #region IExplorerParentObject Members

        public override void Refresh()
        {
            base.Refresh();
            this.Dispose();

            _dataset = new AccessFDBDataset();
            _dataset.ConnectionString = "mdb=" + _filename + ";dsname=" + _dsname;
            if (_dataset.Open())
            {
                foreach (IDatasetElement element in _dataset.Elements)
                {
                    base.AddChildObject(new AccessFDBFeatureClassExplorerObject(this, _filename, _dsname, element));
                }
            }
            _fdb = (AccessFDB)_dataset.Database;
        }

        #endregion

        #region IExplorerObjectCommandParameters Members

        public Dictionary<string, string> Parameters
        {
            get
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("AccessFDB", _filename);
                parameters.Add("Dataset", _dsname);
                return parameters;
            }
        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string mdbName = FullName.Substring(0, lastIndex);
            string dsName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            AccessFDBExplorerObject fdbObject = new AccessFDBExplorerObject();
            fdbObject = (AccessFDBExplorerObject)fdbObject.CreateInstanceByFullName(mdbName, cache);
            if (fdbObject == null || fdbObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in fdbObject.ChildObjects)
            {
                if (exObject.Name == dsName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        internal bool DeleteFeatureClass(string name)
        {
            if (_dataset == null || !(_dataset.Database is IFeatureDatabase)) return false;

            if (!((IFeatureDatabase)_dataset.Database).DeleteFeatureClass(name))
            {
                MessageBox.Show(_dataset.Database.lastErrorMsg);
                return false;
            }
            return true;
        }

        internal bool DeleteDataset(string dsname)
        {
            if (_dataset == null || !(_dataset.Database is IFeatureDatabase)) return false;

            if (!((IFeatureDatabase)_dataset.Database).DeleteDataset(dsname))
            {
                MessageBox.Show(_dataset.Database.lastErrorMsg);
                return false;
            }
            return true;
        }

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is AccessFDBExplorerObject) return true;

            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            FormNewDataset dlg = new FormNewDataset();
            dlg.IndexTypeIsEditable = false;
            if (dlg.ShowDialog() != DialogResult.OK) return null;

            AccessFDB fdb = new AccessFDB();
            fdb.Open(parentExObject.FullName);
            int dsID = -1;

            string datasetName = dlg.DatasetName;
            switch (dlg.DatasetType)
            {
                case FormNewDataset.datasetType.FeatureDataset:
                    dsID = fdb.CreateDataset(datasetName, dlg.SpatialReferene);
                    break;
                case FormNewDataset.datasetType.ImageDataset:
                    dsID = fdb.CreateImageDataset(datasetName, dlg.SpatialReferene, null, dlg.ImageSpace, dlg.AdditionalFields);
                    datasetName = "#" + datasetName;
                    break;
            }

            if (dsID == -1)
            {
                MessageBox.Show(fdb.lastErrorMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return new AccessFDBDatasetExplorerObject(parentExObject, parentExObject.FullName, datasetName);
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get { return _contextItems; }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (DeleteDataset(_dsname))
            {
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return true;
            }
            return false;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            if (newName == this.Name) return false;

            if (_dataset == null || !(_dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rename dataset...\nUncorrect dataset !!!");
                return false;
            }
            if (!((AccessFDB)_dataset.Database).RenameDataset(this.Name, newName))
            {
                MessageBox.Show("Can't rename dataset...\n" + ((AccessFDB)_dataset.Database).lastErrorMsg);
                return false;
            }

            _dsname = newName;

            if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            return true;
        }

        #endregion

        public override void Content_DragEnter(DragEventArgs e)
        {
            if (_icon is AccessFDBImageDatasetIcon)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                base.Content_DragEnter(e);
            }
        }

        internal string FileName { get { return _filename; } }
    }

    [gView.Framework.system.RegisterPlugIn("A610B342-E911-4c52-8E35-72A69B52440A")]
    public class AccessFDBFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectContextMenu, IExplorerObjectRenamable, IExplorerObjectCreatable
    {
        private string _filename = "", _dsname = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private IRasterClass _rc = null;
        private AccessFDBDatasetExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;

        public AccessFDBFeatureClassExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public AccessFDBFeatureClassExplorerObject(AccessFDBDatasetExplorerObject parent, string filename, string dsname, IDatasetElement element)
            : base(parent, typeof(FeatureClass), 1)
        {
            if (element == null) return;

            _parent = parent;
            _filename = filename;
            _dsname = dsname;
            _fcname = element.Title;

            string typePrefix = String.Empty;
            bool isLinked = false;
            if (element.Class is LinkedFeatureClass)
            {
                typePrefix = "Linked ";
                isLinked = true;
            }

            if (element.Class is IRasterCatalogClass)
            {
                _icon = new AccessFDBRasterIcon();
                _type = typePrefix + "Raster Catalog Layer";
                _rc = (IRasterClass)element.Class;
            }
            else if (element.Class is IRasterClass)
            {
                _icon = new AccessFDBRasterIcon();
                _type = typePrefix + "Raster Layer";
                _rc = (IRasterClass)element.Class;
            }
            else if (element.Class is IFeatureClass)
            {
                _fc = (IFeatureClass)element.Class;
                switch (_fc.GeometryType)
                {
                    case geometryType.Envelope:
                    case geometryType.Polygon:
                        if (isLinked)
                            _icon = new AccessFDBLinkedPolygonIcon();
                        else
                            _icon = new AccessFDBPolygonIcon();
                        _type = typePrefix + "Polygon Featureclass";
                        break;
                    case geometryType.Multipoint:
                    case geometryType.Point:
                        if (isLinked)
                            _icon = new AccessFDBLinkedPointIcon();
                        else
                            _icon = new AccessFDBPointIcon();
                        _type = typePrefix + "Point Featureclass";
                        break;
                    case geometryType.Polyline:
                        if (isLinked)
                            _icon = new AccessFDBLinkedLineIcon();
                        else
                            _icon = new AccessFDBLineIcon();
                        _type = typePrefix + "Polyline Featureclass";
                        break;
                }
            }

            _contextItems = new ToolStripItem[1];
            _contextItems[0] = new ToolStripMenuItem("Tasks");

            //_contextItems = new ToolStripItem[1];
            //_contextItems[0] = new ToolStripMenuItem("Rebuild Spatial Index...");
            //_contextItems[0].Click += new EventHandler(RebuildSpatialIndex_Click);
            ToolStripMenuItem item = new ToolStripMenuItem("Shrink Spatial Index...");
            item.Click += new EventHandler(ShrinkSpatialIndex_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            item = new ToolStripMenuItem("Spatial Index Definition...");
            item.Click += new EventHandler(SpatialIndexDef_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            item = new ToolStripMenuItem("Repair Spatial Index...");
            item.Click += new EventHandler(RepairSpatialIndex_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(new ToolStripSeparator());
            item = new ToolStripMenuItem("Truncate");
            item.Click += new EventHandler(Truncate_Click);
            ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        }

        #region IExplorerObject Members

        public string Name
        {
            get { return _fcname; }
        }

        public string FullName
        {
            get
            {
                return _filename + ((_filename != "") ? @"\" : "") + _dsname + ((_dsname != "") ? @"\" : "") + _fcname;
            }
        }
        public string Type
        {
            get { return String.IsNullOrEmpty(_type) ? "Feature Class" : _type; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                if (_icon == null)
                    return new AccessFDBPolygonIcon();
                return _icon;
            }
        }
        public void Dispose()
        {
            if (_fc != null)
            {
                _fc = null;
            }
            if (_rc != null)
            {
                _rc = null;
            }
        }
        public object Object
        {
            get
            {
                if (_fc != null) return _fc;
                if (_rc != null) return _rc;
                return null;
            }
        }
        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName)) return cache[FullName];

            FullName = FullName.Replace("/", @"\");
            int lastIndex = FullName.LastIndexOf(@"\");
            if (lastIndex == -1) return null;

            string dsName = FullName.Substring(0, lastIndex);
            string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

            AccessFDBDatasetExplorerObject dsObject = new AccessFDBDatasetExplorerObject();
            dsObject = dsObject.CreateInstanceByFullName(dsName, cache) as AccessFDBDatasetExplorerObject;
            if (dsObject == null || dsObject.ChildObjects == null) return null;

            foreach (IExplorerObject exObject in dsObject.ChildObjects)
            {
                if (exObject.Name == fcName)
                {
                    cache.Append(exObject);
                    return exObject;
                }
            }
            return null;
        }

        #endregion

        #region IExplorerObjectContextMenu Member

        public ToolStripItem[] ContextMenuItems
        {
            get
            {
                return _contextItems;
            }
        }

        #endregion

        void ShrinkSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
                return;
            }

            List<IClass> classes = new List<IClass>();
            classes.Add(_fc);

            SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
            rebuilder.RebuildIndices(classes);
        }

        void SpatialIndexDef_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRebuildSpatialIndexDef dlg = new FormRebuildSpatialIndexDef((AccessFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void RepairSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((AccessFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void Truncate_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
                return;
            }

            ((AccessFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
        }

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted = null;

        public bool DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            if (_parent == null) return false;
            if (_parent.DeleteFeatureClass(_fcname))
            {
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return true;
            }
            return false;
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public bool RenameExplorerObject(string newName)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is AccessFDB))
            {
                MessageBox.Show("Can't rename featureclass...\nUncorrect feature class !!!");
                return false;
            }

            if (!((AccessFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
            {
                MessageBox.Show("Can't rename featureclass...\n" + ((AccessFDB)_fc.Dataset.Database).lastErrorMsg);
                return false;
            }

            _fcname = newName;

            if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
            return true;
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is AccessFDBDatasetExplorerObject &&
                !((AccessFDBDatasetExplorerObject)parentExObject).IsImageDataset) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            if (!(parentExObject.Object is IFeatureDataset) || !(((IDataset)parentExObject.Object).Database is AccessFDB))
            {
                return null;
            }
            AccessFDB fdb = ((IDataset)parentExObject.Object).Database as AccessFDB;

            FormNewFeatureclass dlg = new FormNewFeatureclass(parentExObject.Object as IFeatureDataset);
            if (dlg.ShowDialog() != DialogResult.OK) return null;

            IGeometryDef gDef = dlg.GeometryDef;

            int FCID = fdb.CreateFeatureClass(
                parentExObject.Name,
                dlg.FeatureclassName,
                gDef,
                dlg.Fields);

            if (FCID < 0)
            {
                MessageBox.Show("ERROR: " + fdb.lastErrorMsg);
                return null;
            }

            ISpatialIndexDef sIndexDef = fdb.SpatialIndexDef(parentExObject.Name);
            fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.FeatureclassName];
            return new AccessFDBFeatureClassExplorerObject(
                parentExObject as AccessFDBDatasetExplorerObject,
                _filename,
                parentExObject.Name,
                element);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("30A0EB20-B4DE-4A5B-9899-FE7972F8D42E")]
    public class AccessFDBLinkedFeatureclassExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is AccessFDBDatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            SqlFDBDatasetExplorerObject parent = (SqlFDBDatasetExplorerObject)parentExObject;

            IFeatureDataset dataset = parent.Object as IFeatureDataset;
            if (dataset == null)
                return null;

            AccessFDB fdb = dataset.Database as AccessFDB;
            if (fdb == null)
                return null;

            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureclassFilter());
            ExplorerDialog dlg = new ExplorerDialog("Select Featureclass", filters, true);

            IExplorerObject ret = null;

            if (dlg.ShowDialog() == DialogResult.OK &&
                dlg.ExplorerObjects != null)
            {
                foreach (IExplorerObject exObj in dlg.ExplorerObjects)
                {
                    if (exObj.Object is IFeatureClass)
                    {
                        int fcid = fdb.CreateLinkedFeatureClass(dataset.DatasetName, (IFeatureClass)exObj.Object);
                        if (ret == null)
                        {
                            IDatasetElement element = dataset[((IFeatureClass)exObj.Object).Name];
                            if (element != null)
                            {
                                ret = new AccessFDBFeatureClassExplorerObject(
                                    parentExObject as AccessFDBDatasetExplorerObject,
                                    ((AccessFDBDatasetExplorerObject)parentExObject).FileName,
                                    parentExObject.Name,
                                    element);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get { return "Linked Featureclass"; }
        }

        public string FullName
        {
            get { return "Linked Featureclass"; }
        }

        public string Type
        {
            get { return String.Empty; }
        }

        public IExplorerIcon Icon
        {
            get { return new AccessFDBGeographicViewIcon(); }
        }

        public IExplorerObject ParentExplorerObject
        {
            get { return null; }
        }

        public new object Object
        {
            get { return null; }
        }

        public Type ObjectType
        {
            get { return null; }
        }

        public int Priority { get { return 1; } }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion
    }

    internal class AccessFDBIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("153A56DA-1B0B-4f5b-B331-1ABA2BB5117B");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return (new Icons()).imageList2.Images[1];
            }
        }

        #endregion
    }

    public class AccessFDBDatasetIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("9664822B-F97C-4833-9F6F-C6E4C2775BC1"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.dataset; }
        }

        #endregion
    }

    public class AccessFDBImageDatasetIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("CCD0622D-A7B7-489f-A21A-D51F68342A61"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.imagedataset; }
        }

        #endregion
    }

    public class AccessFDBPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("A713AF39-D76C-4a78-AE84-9147B0E1D26B"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.field_geom_point; }
        }

        #endregion
    }

    public class AccessFDBLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("2FBE97B1-6604-4ee6-BEDF-E5795B9A4F88"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.field_geom_line; }
        }

        #endregion
    }

    public class AccessFDBPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("BE872D72-5919-452e-B8E4-C2D2F9E9C36C"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.field_geom_polygon; }
        }

        #endregion
    }

    public class AccessFDBRasterIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("9C242F6F-08A1-4279-9180-1EB4286BEB58"); }
        }

        public System.Drawing.Image Image
        {
            get { return (new Icons()).imageList2.Images[6]; }
        }

        #endregion
    }

    public class AccessFDBNetworkIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("A8D4371B-3B9B-43c1-945F-955576CB17C4"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.network; }
        }

        #endregion
    }

    public class AccessFDBGeographicViewIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("cab758ab-6d04-45db-8053-01bdc2fbc285"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.table_relationship_16; }
        }

        #endregion
    }

    public class AccessFDBLinkedPointIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("41EFA4B6-EAB3-466B-AF84-C416D681E520"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.linked_geom_point; }
        }

        #endregion
    }

    public class AccessFDBLinkedLineIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("5727A47B-A79E-42DD-855C-98F866422A6F"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.linked_geom_line; }
        }

        #endregion
    }

    public class AccessFDBLinkedPolygonIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return new Guid("E8C922D7-E1BF-4C55-A8F0-6285B2CD33D7"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.DataSources.Fdb.UI.Properties.Resources.linked_geom_polygon; }
        }

        #endregion
    }
}

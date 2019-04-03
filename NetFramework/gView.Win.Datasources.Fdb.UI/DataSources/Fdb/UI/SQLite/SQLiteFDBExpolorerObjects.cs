using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.SQLite;
using gView.DataSources.Fdb.UI.MSAccess;
using gView.DataSources.Fdb.UI.MSSql;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.UI.Dialogs;
using gView.Framework.UI.Dialogs.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gView.DataSources.Fdb.UI.SQLite
{
    [gView.Framework.system.RegisterPlugIn("A4F900EC-C5E4-4518-BAB9-213AF660E8F1")]
    public class SQLiteFDBExplorerObject : ExplorerParentObject, IExplorerFileObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectCreatable
    {
        private SQLiteFDBIcon _icon = new SQLiteFDBIcon();
        private string _filename = "", _errMsg = "";

        public SQLiteFDBExplorerObject() : base(null, null, 2) { }
        public SQLiteFDBExplorerObject(IExplorerObject parent, string filename)
            : base(parent, null, 2)
        {
            _filename = filename;
        }

        #region IExplorerObject Members

        public string Filter
        {
            get { return "*.fdb"; }
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
            get { return "SQLite Feature Database"; }
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
            if (!f.ToLower().EndsWith(".fdb")) return null;

            try
            {
                if (!(new FileInfo(f).Exists)) return null;
                using (SQLiteFDB fdb = new SQLiteFDB())
                {
                    if (!fdb.Open(f) || !fdb.IsValidAccessFDB)
                        return null;
                }
            }
            catch { return null; }

            return new SQLiteFDBExplorerObject(parent, filename);
        }
        #endregion

        private string[] DatasetNames
        {
            get
            {
                try
                {
                    SQLiteFDB fdb = new SQLiteFDB();
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
                    base.AddChildObject(new SQLiteFDBDatasetExplorerObject(this, _filename, dsname));
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
                parameters.Add("SQLiteFDB", _filename);
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
            dlg.Title = "New SQLite Feature Database...";
            dlg.Filter = "SQLite DB(*.fdb)|*.fdb";
            dlg.FileName = parentExObject.FullName + @"\database1.fdb";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SQLiteFDB fdb = new SQLiteFDB();
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

    [gView.Framework.system.RegisterPlugIn("E76179C7-FD21-4C0F-ADA0-1DC260E8C50E")]
    public class SQLiteFDBDatasetExplorerObject : ExplorerObjectFeatureClassImport, IExplorerSimpleObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectCreatable, IExplorerObjectContextMenu, IExplorerObjectRenamable
    {
        private IExplorerIcon _icon = null;
        private string _filename = "";
        private ToolStripItem[] _contextItems = null;
        private bool _isImageDataset = false;
        //private AccessFDBDataset _dataset = null;

        public SQLiteFDBDatasetExplorerObject() : base(null, typeof(SQLiteFDBDataset)) { }

        public SQLiteFDBDatasetExplorerObject(IExplorerObject parent, string filename, string dsname)
            : base(parent, typeof(SQLiteFDBDataset))
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

            _dataset = new SQLiteFDBDataset();
            _dataset.ConnectionString = "Data Source=" + _filename + ";dsname=" + _dsname;
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
            get { return "SQLite Feature Database Dataset"; }
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

            _dataset = new SQLiteFDBDataset();
            _dataset.ConnectionString = "Data Source=" + _filename + ";dsname=" + _dsname;
            
            if (_dataset.Open())
            {
                foreach (IDatasetElement element in _dataset.Elements)
                {
                    base.AddChildObject(new SQLiteFDBFeatureClassExplorerObject(this, _filename, _dsname, element));
                }
            }
            _fdb = (SQLiteFDB)_dataset.Database;
        }

        #endregion

        #region IExplorerObjectCommandParameters Members

        public Dictionary<string, string> Parameters
        {
            get
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("SQLiteFDB", _filename);
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

            SQLiteFDBExplorerObject fdbObject = new SQLiteFDBExplorerObject();
            fdbObject = (SQLiteFDBExplorerObject)fdbObject.CreateInstanceByFullName(mdbName, cache);
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
            if (parentExObject is SQLiteFDBExplorerObject) return true;

            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            FormNewDataset dlg = new FormNewDataset();
            dlg.IndexTypeIsEditable = false;
            if (dlg.ShowDialog() != DialogResult.OK) return null;

            SQLiteFDB fdb = new SQLiteFDB();
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

            return new SQLiteFDBDatasetExplorerObject(parentExObject, parentExObject.FullName, datasetName);
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

            if (_dataset == null || !(_dataset.Database is SQLiteFDB))
            {
                MessageBox.Show("Can't rename dataset...\nUncorrect dataset !!!");
                return false;
            }
            if (!((SQLiteFDB)_dataset.Database).RenameDataset(this.Name, newName))
            {
                MessageBox.Show("Can't rename dataset...\n" + ((SQLiteFDB)_dataset.Database).lastErrorMsg);
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

        public string FileName { get { return _filename; } }
    }

    [gView.Framework.system.RegisterPlugIn("16DB07EC-5C30-4C2E-85AC-B49A44188B1A")]
    public class SQLiteFDBFeatureClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectContextMenu, IExplorerObjectRenamable, IExplorerObjectCreatable
    {
        private string _filename = "", _dsname = "", _fcname = "", _type = "";
        private IExplorerIcon _icon = null;
        private IFeatureClass _fc = null;
        private IRasterClass _rc = null;
        private SQLiteFDBDatasetExplorerObject _parent = null;
        private ToolStripItem[] _contextItems = null;
        private bool _isNetwork = false;

        public SQLiteFDBFeatureClassExplorerObject() : base(null, typeof(FeatureClass), 1) { }
        public SQLiteFDBFeatureClassExplorerObject(SQLiteFDBDatasetExplorerObject parent, string filename, string dsname, IDatasetElement element)
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
                    case geometryType.Network:
                        _icon = new AccessFDBNetworkIcon();
                        _type = "Networkclass";
                        _isNetwork = true;
                        break;
                }
            }

            if (!_isNetwork)
            {
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

            SQLiteFDBDatasetExplorerObject dsObject = new SQLiteFDBDatasetExplorerObject();
            dsObject = dsObject.CreateInstanceByFullName(dsName, cache) as SQLiteFDBDatasetExplorerObject;
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRebuildSpatialIndexDef dlg = new FormRebuildSpatialIndexDef((SQLiteFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void RepairSpatialIndex_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
            {
                MessageBox.Show("Can't show spatial index definition...\nUncorrect feature class !!!");
                return;
            }

            FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((SQLiteFDB)_fc.Dataset.Database, _fc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
            }
        }

        void Truncate_Click(object sender, EventArgs e)
        {
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
            {
                MessageBox.Show("Can't rebuild index...\nUncorrect feature class !!!");
                return;
            }

            ((SQLiteFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
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
            if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
            {
                MessageBox.Show("Can't rename featureclass...\nUncorrect feature class !!!");
                return false;
            }

            if (!((SQLiteFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
            {
                MessageBox.Show("Can't rename featureclass...\n" + ((SQLiteFDB)_fc.Dataset.Database).lastErrorMsg);
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
            if (parentExObject is SQLiteFDBDatasetExplorerObject &&
                !((SQLiteFDBDatasetExplorerObject)parentExObject).IsImageDataset) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!CanCreate(parentExObject)) return null;

            if (!(parentExObject.Object is IFeatureDataset) || !(((IDataset)parentExObject.Object).Database is SQLiteFDB))
            {
                return null;
            }
            SQLiteFDB fdb = ((IDataset)parentExObject.Object).Database as SQLiteFDB;

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
            return new SQLiteFDBFeatureClassExplorerObject(
                parentExObject as SQLiteFDBDatasetExplorerObject,
                _filename,
                parentExObject.Name,
                element);
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("EEDCCBB2-588E-418A-B048-4B6C210A25AE")]
    public class SQLiteFDBLinkedFeatureclassExplorerObject : IExplorerSimpleObject, IExplorerObjectCreatable
    {
        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            return parentExObject is SQLiteFDBDatasetExplorerObject;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            SQLiteFDBDatasetExplorerObject parent = (SQLiteFDBDatasetExplorerObject)parentExObject;

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
                            IDatasetElement element=dataset[((IFeatureClass)exObj.Object).Name];
                            if (element != null)
                            {
                                ret = new SQLiteFDBFeatureClassExplorerObject(
                                    parent,
                                    parent.FileName,
                                    parent.Name,
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

    [gView.Framework.system.RegisterPlugIn("AFDE90FF-D063-4224-BD31-1B30C266D55B")]
    public class SQLiteFDBNetworkClassExplorerObject : ExplorerObjectCls, IExplorerSimpleObject, IExplorerObjectCreatable
    {
        public SQLiteFDBNetworkClassExplorerObject() : base(null, typeof(SQLiteFDBNetworkFeatureClass), 1) { }

        #region IExplorerObject Member

        public string Name
        {
            get { return String.Empty; }
        }

        public string FullName
        {
            get { return String.Empty; }
        }

        public string Type
        {
            get { return "Network Class"; }
        }

        public IExplorerIcon Icon
        {
            get { return new AccessFDBNetworkIcon(); }
        }

        public new object Object
        {
            get { return null; }
        }

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

        #region IExplorerObjectCreatable Member

        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is SQLiteFDBDatasetExplorerObject) return true;
            return false;
        }

        public IExplorerObject CreateExplorerObject(IExplorerObject parentExObject)
        {
            if (!(parentExObject is SQLiteFDBDatasetExplorerObject))
                return null;

            SQLiteFDBDatasetExplorerObject parent = (SQLiteFDBDatasetExplorerObject)parentExObject;

            IFeatureDataset dataset = ((SQLiteFDBDatasetExplorerObject)parentExObject).Object as IFeatureDataset;
            if (dataset == null || !(dataset.Database is SQLiteFDB))
                return null;

            FormNewNetworkclass dlg = new FormNewNetworkclass(dataset, typeof(CreateFDBNetworkFeatureclass));
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            CreateFDBNetworkFeatureclass creator = new CreateFDBNetworkFeatureclass(
                dataset, dlg.NetworkName,
                dlg.EdgeFeatureclasses,
                dlg.NodeFeatureclasses);
            creator.SnapTolerance = dlg.SnapTolerance;
            creator.ComplexEdgeFcIds = dlg.ComplexEdgeFcIds;
            creator.GraphWeights = dlg.GraphWeights;
            creator.SwitchNodeFcIdAndFieldnames = dlg.SwitchNodeFcIds;
            creator.NodeTypeFcIds = dlg.NetworkNodeTypeFcIds;

            FormProgress progress = new FormProgress();
            progress.ShowProgressDialog(creator, null, creator.Thread);

            IDatasetElement element = ((IFeatureDataset)parentExObject.Object)[dlg.NetworkName];
            return new SQLiteFDBFeatureClassExplorerObject(
                                    parent,
                                    parent.FileName,
                                    parent.Name,
                                    element);
        }

        #endregion
    }

    internal class SQLiteFDBIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get
            {
                return new Guid("301A2AEF-F26E-4B2C-8060-86087948EB64");
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::gView.DataSources.Fdb.UI.Properties.Resources.file_fdb;
            }
        }
        
        #endregion
    }
}

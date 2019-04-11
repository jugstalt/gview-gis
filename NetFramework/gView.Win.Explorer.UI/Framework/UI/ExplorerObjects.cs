using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Management;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.system.UI;
using System.Windows.Forms;
using gView.Framework.FDB;
using gView.Framework.Data;
using gView.Framework.UI.Dialogs;
using System.Threading;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System.Threading.Tasks;
using gView.Framework.Sys.UI;

namespace gView.Framework.UI
{
    [gView.Framework.system.RegisterPlugIn("458E62A0-4A93-45cf-B14D-2F958D67E522")]
    public class DirectoryObject : ExplorerParentObject, IExplorerObject, IExplorerObjectCommandParameters, ISerializableExplorerObject, IExplorerObjectDeletable, IExplorerObjectRenamable, IExplorerObjectCreatable, IExplorerObjectContentDragDropEvents2
    {
        static private IExplorerIcon _icon = new DirectoryExplorerIcon();
        string _path = "";

        public DirectoryObject() : base(null, null, 1) { }
        public DirectoryObject(IExplorerObject parent, string path)
            : base(parent, null, 1)
        {
            _path = path;
        }

        #region IExplorerObject Members

        public string Filter
        {
            get { return ""; }
        }

        public string Name
        {
            get
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(_path);
                    return di.Name;
                }
                catch { return ""; }
            }
        }

        public string FullName
        {
            get { return _path; }

        }

        public string Type
        {
            get { return "Directory"; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return _icon;
            }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            try
            {
                if (!(new DirectoryInfo(FullName)).Exists) return null;
            }
            catch { return null; }
            return new DirectoryObject(this, FullName);
        }

        #endregion

        #region IExplorerObjectCommandParameters Members

        public Dictionary<string, string> Parameters
        {
            get
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("Path", _path + @"\");

                return parameters;
            }
        }

        #endregion

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            List<IExplorerObject> childs = await DirectoryObject.Refresh(this, this.FullName);
            if (childs == null)
                return false;

            foreach (IExplorerObject child in childs)
            {
                base.AddChildObject(child);
            }

            return true;
        }

        #endregion

        async internal static Task<List<IExplorerObject>> Refresh(IExplorerObject parent, string FullName)
        {
            List<IExplorerObject> childs = new List<IExplorerObject>();

            try
            {
                foreach (string subdir in Directory.GetDirectories(FullName))
                {
                    DirectoryInfo di = new DirectoryInfo(subdir);
                    childs.Add(new DirectoryObject(parent, di.FullName));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return null;
            }

            gView.Framework.system.PlugInManager manager = new gView.Framework.system.PlugInManager();

            foreach (var exObjectType in manager.GetPlugins(Plugins.Type.IExplorerObject))
            {
                var exObj = manager.CreateInstance<IExplorerObject>(exObjectType);
                if (!(exObj is IExplorerFileObject)) continue;

                foreach (string filter in ((IExplorerFileObject)exObj).Filter.Split('|'))
                {
                    foreach (string file in Directory.GetFiles(FullName, filter))
                    {
                        FileInfo fi = new FileInfo(file);
                        IExplorerFileObject obj = await ((IExplorerFileObject)exObj).CreateInstance(parent, fi.FullName);
                        if (obj == null) continue;

                        childs.Add(obj);
                    }
                }
            }

            return childs;
        }

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            try
            {
                DirectoryInfo di = new DirectoryInfo(FullName);
                if (!di.Exists) return null;

                DirectoryObject dObject = new DirectoryObject(this, FullName);
                cache.Append(dObject);
                return Task.FromResult<IExplorerObject>(dObject);
            }
            catch
            {
                return Task.FromResult<IExplorerObject>(null);
            }
        }

        #endregion

        #region IExplorerObjectDeletable Member

        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(_path);
                di.Delete();
                if (ExplorerObjectDeleted != null) ExplorerObjectDeleted(this);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: " + ex.Message);
                return Task.FromResult(false);
            }
        }

        #endregion

        #region IExplorerObjectRenamable Member

        public event ExplorerObjectRenamedEvent ExplorerObjectRenamed;

        public Task<bool> RenameExplorerObject(string newName)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(_path);
                string newPath = di.Parent.FullName + @"\" + newName;
                Microsoft.VisualBasic.FileSystem.Rename(
                    _path, newPath);
                _path = newPath;

                if (ExplorerObjectRenamed != null) ExplorerObjectRenamed(this);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: " + ex.Message);
                return Task.FromResult(false);
            }
        }

        #endregion

        #region IExplorerObjectCreatable Member

        public event ExplorerObjectCreatedEvent ExplorerObjectCreated = null;
        public bool CanCreate(IExplorerObject parentExObject)
        {
            if (parentExObject is DirectoryObject ||
                parentExObject is DriveObject) return true;

            return false;
        }

        public Task<IExplorerObject> CreateExplorerObject(IExplorerObject parentExObject)
        {
            string newName = Microsoft.VisualBasic.Interaction.InputBox("New Directory", "Create", "New Directory", 200, 300);
            if (newName.Trim().Equals(String.Empty)) return null;

            try
            {
                DirectoryInfo di = new DirectoryInfo(parentExObject.FullName + @"\" + newName);
                di.Create();

                return Task.FromResult<IExplorerObject>( new DirectoryObject(parentExObject, di.FullName));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: " + ex.Message);
                return Task.FromResult<IExplorerObject>(null);
            }
        }

        #endregion

        #region IExplorerObjectContentDragDropEvents Member

        public virtual void Content_DragDrop(System.Windows.Forms.DragEventArgs e)
        {
            Content_DragDrop(e, null);
        }

        public virtual void Content_DragEnter(System.Windows.Forms.DragEventArgs e)
        {
            Content_DragEnter(e, null);
        }

        public virtual void Content_DragLeave(EventArgs e)
        {
            Content_DragLeave(e, null);
        }

        public virtual void Content_DragOver(System.Windows.Forms.DragEventArgs e)
        {
            Content_DragOver(e, null);
        }

        public virtual void Content_GiveFeedback(System.Windows.Forms.GiveFeedbackEventArgs e)
        {
            Content_GiveFeedback(e, null);
        }

        public virtual void Content_QueryContinueDrag(System.Windows.Forms.QueryContinueDragEventArgs e)
        {
            Content_QueryContinueDrag(e, null);
        }

        #endregion

        #region IExplorerObjectContentDragDropEvents2 Member

        async public void Content_DragDrop(DragEventArgs e, IUserData userdata)
        {
            PlugInManager compMan = new PlugInManager();
            List<IFileFeatureDatabase> databases = new List<IFileFeatureDatabase>();
            foreach (var dbType in compMan.GetPlugins(Plugins.Type.IFileFeatureDatabase))
            {
                var db = compMan.CreateInstance<IFileFeatureDatabase>(dbType);
                if (db == null) continue;

                databases.Add(db);
            }
            if (databases.Count == 0) return;

            bool schemaOnly = false;
            if (userdata != null &&
                userdata.GetUserData("gView.Framework.UI.BaseTools.PasteSchema") != null &&
                userdata.GetUserData("gView.Framework.UI.BaseTools.PasteSchema").Equals(true))
            {
                schemaOnly = true;
            }

            foreach (string format in e.Data.GetFormats())
            {
                object ob = e.Data.GetData(format);
                if (ob is IEnumerable<IExplorerObjectSerialization>)
                {
                    ExplorerObjectManager exObjectManager = new ExplorerObjectManager();

                    List<IExplorerObject> exObjects = new List<IExplorerObject>(await exObjectManager.DeserializeExplorerObject((IEnumerable<IExplorerObjectSerialization>)ob));
                    if (exObjects == null) return;

                    foreach (IExplorerObject exObject in ListOperations<IExplorerObject>.Clone(exObjects))
                    {
                        IFeatureClass fc = await exObject.GetInstanceAsync() as IFeatureClass;
                        if (fc == null) continue;
                    }
                    if (exObjects.Count == 0) return;

                    FormFeatureclassCopy dlg = await FormFeatureclassCopy.Create(exObjects, databases, this.FullName);
                    dlg.SchemaOnly = schemaOnly;
                    if (dlg.ShowDialog() != DialogResult.OK) continue;

                    if (dlg.SelectedFeatureDatabase == null) return;
                    IFileFeatureDatabase fileDB = dlg.SelectedFeatureDatabase;
                    _dataset = await fileDB.GetDataset(this.FullName);
                    if (_dataset == null) return;

                    //_dataset = new ImportFeatureDataset(dlg.SelectedFeatureDatabase);

                    foreach (FeatureClassListViewItem fcItem in dlg.FeatureClassItems)
                    {
                        ImportDatasetObject(fcItem, schemaOnly).Wait();
                    }

                    exObjectManager.Dispose(); // alle ExplorerObjects wieder löschen...
                }
            }
        }

        public void Content_DragEnter(DragEventArgs e, IUserData userdata)
        {
            PlugInManager compMan = new PlugInManager();
            bool found = false;
            foreach (var dbType in compMan.GetPlugins(Plugins.Type.IFileFeatureDatabase))
            {
                IFileFeatureDatabase db = compMan.CreateInstance(dbType) as IFileFeatureDatabase;
                if (db == null) continue;

                found = true;
            }
            if (!found) return;

            foreach (string format in e.Data.GetFormats())
            {
                object ob = e.Data.GetData(format);

                if (ob is List<IExplorerObjectSerialization>)
                {
                    foreach (IExplorerObjectSerialization ser in (List<IExplorerObjectSerialization>)ob)
                    {
                        if (ser.ObjectTypes.Contains(typeof(IFeatureDataset)) ||
                            ser.ObjectTypes.Contains(typeof(IFeatureClass)))
                        {
                            e.Effect = DragDropEffects.Copy;
                            return;
                        }
                    }
                }
            }
        }

        public void Content_DragLeave(EventArgs e, IUserData userdata)
        {

        }

        public void Content_DragOver(DragEventArgs e, IUserData userdata)
        {

        }

        public void Content_GiveFeedback(GiveFeedbackEventArgs e, IUserData userdata)
        {

        }

        public void Content_QueryContinueDrag(QueryContinueDragEventArgs e, IUserData userdata)
        {

        }

        #endregion

        #region Import
        private FeatureImport _import = null;
        private IFeatureDataset _dataset = null;

        async private Task ImportDatasetObject(object datasetObject, bool schemaOnly)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in await dataset.Elements())
                {
                    if (element is IFeatureLayer)
                    {
                        await ImportDatasetObject(((IFeatureLayer)element).FeatureClass, schemaOnly);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ImportAsync));

                if (_import == null)
                    _import = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }
                _import.SchemaOnly = schemaOnly;

                FeatureClassImportProgressReporter reporter = await FeatureClassImportProgressReporter.Create(_import, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Import Featureclass: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _import = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ImportAsync));

                if (_import == null)
                    _import = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }
                _import.SchemaOnly = schemaOnly;

                FeatureClassImportProgressReporter reporter = await FeatureClassImportProgressReporter.Create(_import, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Import Featureclass: " + ((FeatureClassListViewItem)datasetObject).Text;
                progress.ShowDialog();
                _import = null;
            }
        }

        // Thread
        private void ImportAsync(object element)
        {
            if (_import == null) return;

            if (element is IFeatureClass)
            {
                if (!_import.ImportToNewFeatureclass(
                    _dataset,
                    ((IFeatureClass)element).Name,
                    (IFeatureClass)element,
                    null,
                    true).Result)
                {
                    MessageBox.Show(_import.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                if (!_import.ImportToNewFeatureclass(
                    _dataset,
                    item.TargetName,
                    item.FeatureClass,
                    item.ImportFieldTranslation,
                    true).Result)
                {
                    MessageBox.Show(_import.lastErrorMsg);
                }
            }
        }

        class FeatureClassImportProgressReporter : IProgressReporter
        {
            private ProgressReport _report = new ProgressReport();
            private ICancelTracker _cancelTracker = null;

            private FeatureClassImportProgressReporter() { }

            async static public Task<FeatureClassImportProgressReporter> Create(FeatureImport import, IFeatureClass source)
            {
                var reporter = new FeatureClassImportProgressReporter();

                if (import == null)
                    return reporter;

                reporter._cancelTracker = import.CancelTracker;

                if (source != null) reporter._report.featureMax = await source.CountFeatures();
                import.ReportAction += new FeatureImport.ReportActionEvent(reporter.import_ReportAction);
                import.ReportProgress += new FeatureImport.ReportProgressEvent(reporter.import_ReportProgress);
                import.ReportRequest += new FeatureImport.ReportRequestEvent(reporter.import_ReportRequest);

                return reporter;
            }

            void import_ReportRequest(FeatureImport sender, RequestArgs args)
            {
                args.Result = MessageBox.Show(
                    args.Request,
                    "Warning",
                    args.Buttons,
                    MessageBoxIcon.Warning);
            }

            void import_ReportProgress(FeatureImport sender, int progress)
            {
                if (ReportProgress == null) return;

                _report.featureMax = Math.Max(_report.featureMax, progress);
                _report.featurePos = progress;

                ReportProgress(_report);
            }

            void import_ReportAction(FeatureImport sender, string action)
            {
                if (ReportProgress == null) return;

                _report.featurePos = 0;
                _report.Message = action;

                ReportProgress(_report);
            }

            #region IProgressReporter Member

            public event ProgressReporterEvent ReportProgress;

            public ICancelTracker CancelTracker
            {
                get { return _cancelTracker; }
            }
            #endregion
        }
        #endregion
    }

    internal class DirectoryExplorerIcon : IExplorerIcon
    {
        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return KnownExplorerObjectIDs.Directory; }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return (new Icons()).imageList1.Images[1];
            }
        }

        #endregion
    }

    //[gView.Framework.system.RegisterPlugIn("CB2915F4-DB1A-461a-A14E-73F3A259F0BA")]
    public class DriveObject : ExplorerParentObject, IExplorerObject
    {
        private string _drive = "";
        private int _imageIndex = 0;
        private string _type = "";

        public DriveObject(IExplorerObject parent, string drive, uint type)
            : base(parent, null, -1)
        {
            _drive = drive;
            switch (type)
            {
                case 2: _imageIndex = 7;
                    _type = "Floppy Disk (" + _drive + ")";
                    break;
                case 5: _imageIndex = 4;
                    _type = "CD-ROM Drive (" + _drive + ")";
                    break;
                case 4: _imageIndex = 5;
                    _type = "Mapped Drive (" + _drive + ")";
                    break;
                case 999: _imageIndex = 5;
                    _type = drive;
                    break;
                default: _imageIndex = 6;
                    _type = "Local Drive (" + _drive + ")";
                    break;
            }
        }

        public int ImageIndex { get { return _imageIndex; } }

        #region IExplorerObject Members

        public string Filter
        {
            get { return ""; }
        }

        public string Name
        {
            get
            {
                return _type;
            }
        }

        public string FullName
        {
            get { return _drive + @"\"; }
        }

        public string Type
        {
            get { return _type; }
        }

        public IExplorerIcon Icon
        {
            get
            {
                return new ObjectIcon(_imageIndex);
            }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        public IExplorerObject CreateInstanceByFullName(string FullName)
        {
            return null;
        }

        #endregion

        #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();
            List<IExplorerObject> childs = await DirectoryObject.Refresh(this, this.FullName);
            if (childs == null)
                return false;

            foreach (IExplorerObject child in childs)
            {
                base.AddChildObject(child);
            }

            return true;
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (cache.Contains(FullName))
                return Task.FromResult<IExplorerObject>(cache[FullName]);

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion
    }

    internal class MappedDriveObject : DriveObject, IExplorerObjectDeletable
    {
        public MappedDriveObject(IExplorerObject parent, string drive)
            : base(parent, drive, 999)
        {
        }

        #region IExplorerObjectDeletable
        public event ExplorerObjectDeletedEvent ExplorerObjectDeleted;

        public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
        {
            ConfigConnections configStream = new ConfigConnections("directories");
            configStream.Remove(this.Name);

            if (ExplorerObjectDeleted != null)
                ExplorerObjectDeleted(this);

            return Task.FromResult(true);
        }
        #endregion
    }

    internal class ObjectIcon : IExplorerIcon
    {
        int _imageIndex = 0;
        public ObjectIcon(int imageIndex)
        {
            _imageIndex = imageIndex;
        }

        #region IExplorerIcon Members

        public Guid GUID
        {
            get { return Guid.NewGuid(); }
        }

        public System.Drawing.Image Image
        {
            get
            {
                System.Windows.Forms.ImageList imageList = (new Icons()).imageList1;
                if (_imageIndex >= imageList.Images.Count) return null;
                return imageList.Images[_imageIndex];
                //return FormExplorer.globalImageList.Images[_imageIndex];
            }
        }

        #endregion
    }

    public class ComputerObject : ExplorerParentObject, IExplorerObject
    {
        IApplication _application = null;

        public ComputerObject(IApplication application)
            : base(null, null, 0)
        {
            _application = application;
        }

        #region IExplorerObject Member

        public string Name
        {
            get { return "Computer"; }
        }

        public string FullName
        {
            get { return ""; }
        }

        public string Type
        {
            get { return ""; }
        }

        public IExplorerIcon Icon
        {
            get { return new ObjectIcon(0); }
        }

        public Task<object> GetInstanceAsync()
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        #region ISerializableExplorerObject Member

        public Task<IExplorerObject> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            if (FullName == String.Empty)
                return Task.FromResult<IExplorerObject>(new ComputerObject(_application));

            return Task.FromResult<IExplorerObject>(null);
        }

        #endregion

      #region IExplorerParentObject Member

        async public override Task<bool> Refresh()
        {
            await base.Refresh();

            string[] drives = System.IO.Directory.GetLogicalDrives();

            foreach (string drive in drives)
            {
                System.IO.DriveInfo info = new System.IO.DriveInfo(drive);

                DriveObject exObject = new DriveObject(this, info.Name.Replace("\\", ""), (uint)info.DriveType);
                base.AddChildObject(exObject);
            }

            ConfigConnections configStream = new ConfigConnections("directories");
            Dictionary<string, string> networkDirectories = configStream.Connections;
            if (networkDirectories != null)
            {
                foreach (string dir in networkDirectories.Keys)
                {
                    MappedDriveObject exObject = new MappedDriveObject(this, networkDirectories[dir]);
                    base.AddChildObject(exObject);
                }
            }

            PlugInManager compMan = new PlugInManager();
            foreach (var exObjectType in compMan.GetPlugins(Plugins.Type.IExplorerObject))
            {
                var exObject = compMan.CreateInstance<IExplorerObject>(exObjectType);
                if (!(exObject is IExplorerGroupObject)) continue;

                base.AddChildObject(exObject);
            }

            return true;
        }

        #endregion
    }

    internal interface IOutput
    {
        void Append2StandardOutput(string text);
    }
}

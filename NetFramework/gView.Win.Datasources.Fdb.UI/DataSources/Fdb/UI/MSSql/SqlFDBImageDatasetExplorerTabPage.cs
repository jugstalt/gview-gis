using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.DataSources.Raster.File;
using gView.Framework.IO;
using gView.Framework.FDB;
using gView.Framework.system;
using gView.Framework.UI.Dialogs;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.SQLite;

namespace gView.DataSources.Fdb.UI.MSSql
{
    [gView.Framework.system.RegisterPlugIn("3308E2AD-5F3C-416b-B1FE-1214389083AB")]
    public partial class SqlFDBImageDatasetExplorerTabPage : UserControl, IExplorerTabPage
    {
        IExplorerObject _exObject = null;
        IExplorerApplication _exAppl = null;

        public SqlFDBImageDatasetExplorerTabPage()
        {
            InitializeComponent();

            _gui_worker = new BackgroundWorker();
            _gui_worker.WorkerSupportsCancellation = true;

            _gui_worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //_gui_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        #region IExplorerTabPage Members

        public new Control Control
        {
            get { return this; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
            {
                _exAppl = (IExplorerApplication)hook;
            }
        }

        public void OnShow()
        {
            if (listView.Items.Count == 0 && !_gui_worker.IsBusy)
                RefreshList();
        }
        public void OnHide()
        {
            //CancelRefreshList();
        }

        public IExplorerObject ExplorerObject
        {
            get
            {
                return _exObject;
            }
            set
            {
                if (_exObject == value) return;
                listView.Items.Clear();
                _exObject = value;
                CancelRefreshList();
            }
        }

        public bool ShowWith(IExplorerObject exObject)
        {
            if (exObject == null) return false;
            if (TypeHelper.Match(exObject.ObjectType, typeof(gView.DataSources.Fdb.MSSql.SqlFDBImageCatalogClass))) return true;
            if (TypeHelper.Match(exObject.ObjectType, typeof(gView.DataSources.Fdb.MSAccess.AccessFDBImageCatalogClass))) return true;
            if (TypeHelper.Match(exObject.ObjectType, typeof(gView.DataSources.Fdb.PostgreSql.pgImageCatalogClass))) return true;
            if (TypeHelper.Match(exObject.ObjectType, typeof(gView.DataSources.Fdb.SQLite.SQLiteFDBImageCatalogClass))) return true;

            if (exObject.Object is gView.DataSources.Fdb.MSSql.SqlFDBImageCatalogClass) return true;
            if (exObject.Object is gView.DataSources.Fdb.MSAccess.AccessFDBImageCatalogClass) return true;
            if (exObject.Object is gView.DataSources.Fdb.PostgreSql.pgImageCatalogClass) return true;
            if (exObject.Object is gView.DataSources.Fdb.SQLite.SQLiteFDBImageCatalogClass) return true;

            return false;
        }

        public string Title
        {
            get { return "Images"; }
        }

        public void RefreshContents()
        {
            RefreshList();
        }
        #endregion

        #region GUI Worker
        BackgroundWorker _gui_worker = new BackgroundWorker();
        IFeatureCursor _cursor = null;

        private delegate void RefreshListCallback();
        private void RefreshList()
        {
            if (listView.InvokeRequired)
            {
                RefreshListCallback d = new RefreshListCallback(RefreshList);
                this.BeginInvoke(d);
            }
            else
            {
                CancelRefreshList();
                listView.Items.Clear();
                _itemCollection.Clear();

                if (_exObject == null) return;
                if (_exObject.Object is SqlFDBImageCatalogClass)
                {
                    SqlFDBImageCatalogClass layer = (SqlFDBImageCatalogClass)_exObject.Object;

                    _cancelWorker = false;
                    _gui_worker.RunWorkerAsync(layer.ImageList);
                }
                else if (_exObject.Object is AccessFDBImageCatalogClass)
                {
                    AccessFDBImageCatalogClass layer = (AccessFDBImageCatalogClass)_exObject.Object;

                    _cancelWorker = false;
                    //_gui_worker.RunWorkerAsync(layer.ImageList);
                    //
                    // Wirft eine Fehler beim Lesen, wenn List in einem 
                    // Workerthread ausgeführt wird...
                    // funzt nur bei SQL Server!!
                    //
                    worker_DoWork(_gui_worker, new DoWorkEventArgs(layer.ImageList));
                }
                else if (_exObject.Object is pgImageCatalogClass)
                {
                    pgImageCatalogClass layer = (pgImageCatalogClass)_exObject.Object;

                    _cancelWorker = false;
                    _gui_worker.RunWorkerAsync(layer.ImageList);
                }
                else if (_exObject.Object is SQLiteFDBImageCatalogClass)
                {
                    SQLiteFDBImageCatalogClass layer = (SQLiteFDBImageCatalogClass)_exObject.Object;

                    _cancelWorker = false;
                    _gui_worker.RunWorkerAsync(layer.ImageList);
                }
            }
        }

        private void CancelRefreshList()
        {
            /*
            if (_gui_worker.IsBusy)
            {
                _gui_worker.CancelAsync();
                while (_gui_worker.IsBusy)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
             * */

            _cancelWorker = true;
        }

        private bool _cancelWorker = false;
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!(e.Argument is IFeatureCursor)) return;

            _cursor = (IFeatureCursor)e.Argument;
            IFeature row;
            while ((row = _cursor.NextFeature) != null)
            {
                row.CaseSensitivFieldnameMatching = false;  // sollte auch pgSQL funktionieren

                string[] items = { row["PATH"].ToString(), 
                    row["MANAGED"].ToString() };

                ListViewItem item = new ListViewItem(items);
                try
                {
                    FileInfo fi = new FileInfo(items[0]);

                    if (fi.Exists)
                    {
                        DateTime t = (DateTime)row["LAST_MODIFIED"];
                        int span = (int)Math.Abs(((TimeSpan)(fi.LastWriteTimeUtc - t)).TotalSeconds);
                        try
                        {
                            FileInfo fi2 = new FileInfo(row["PATH2"].ToString());
                            if (fi2.Exists)
                            {
                                t = (DateTime)row["LAST_MODIFIED2"];
                                span = Math.Max((int)Math.Abs(((TimeSpan)(fi2.LastWriteTimeUtc - t)).TotalSeconds), span);
                            }
                        }
                        catch { }
                        if (span < 1)
                        {
                            item.ImageIndex = 0;
                        }
                        else
                        {
                            item.ImageIndex = 1;
                        }
                    }
                    else
                    {
                        item.ImageIndex = 3;
                    }
                }
                catch
                {
                    item.ImageIndex = 3;
                }

                AddListItemCached(item);
                if (_cancelWorker) break;
            }

            AddListItems(_itemCollection);
            _itemCollection.Clear();
            SetStatusLabel1Text(listView.Items.Count + " Items...");

            _cursor.Dispose();
            _cursor = null;
        }

        private delegate void AddListItemCallback(ListViewItem item);
        private void AddListItem(ListViewItem item)
        {
            if (listView.InvokeRequired)
            {
                AddListItemCallback d = new AddListItemCallback(AddListItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                listView.Items.Add(item);
            }
        }

        private delegate void AddListItemsCallback(List<ListViewItem> items);
        private void AddListItems(List<ListViewItem> items)
        {
            if (items == null || items.Count == 0) return;
            if (listView.InvokeRequired)
            {
                AddListItemsCallback d = new AddListItemsCallback(AddListItems);
                this.Invoke(d, new object[] { items });
            }
            else
            {
                listView.Items.AddRange(items.ToArray());
            }
        }

        private List<ListViewItem> _itemCollection = new List<ListViewItem>();
        private void AddListItemCached(ListViewItem item)
        {
            _itemCollection.Add(item);
            if (_itemCollection.Count > 100)
            {
                AddListItems(_itemCollection);
                _itemCollection.Clear();
                SetStatusLabel1Text(listView.Items.Count + " Items...");
            }
        }

        //private delegate void SetStatusLabel1TextCallback(string text);
        private void SetStatusLabel1Text(string text)
        {
            StatusLabel1.Text = text;
        }
        #endregion

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || _exObject == null || _exObject.Object == null) return;
            string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (data == null || data.Length == 0) return;

            ImportFiles(data, true, null);
        }

        private bool ImportFiles(string[] data, bool refreshList, Dictionary<string,Guid> providers)
        {
            IRasterClass rc = _exObject.Object as IRasterClass;
            if (rc == null) return false;
            IRasterDataset rDS = rc.Dataset as IRasterDataset;
            if (rDS == null || rDS.Database == null) return false;

            Thread thread = new Thread(new ParameterizedThreadStart(Import));
            FDBImageDataset operations = new FDBImageDataset(rDS.Database as IImageDB, rDS.DatasetName);
            // refreshList = false wenn ganzen Verzeichnis durchsucht wird...
            // dann sollen auch keine Fehler verursacht werden wenn ein bild nicht gereferenziert ist,
            // in diesem fall bild einfach ignorieren
            operations.handleNonGeorefAsError = refreshList;

            ImageImportProgressReporter reporter = new ImageImportProgressReporter(operations, data.Length);

            ImportArguments args = new ImportArguments(operations, rDS, data, providers);

            FormProgress progress = new FormProgress(reporter, thread, args);
            progress.Text = "Import Images: " + rDS.DatasetName;
            progress.Mode = ProgressMode.ProgressDisk;
            progress.ShowDialog();

            if (refreshList) RefreshList();

            if (!reporter.CancelTracker.Continue)
            {
                MessageBox.Show("Cancel...");
                return false;
            }

            if (operations.lastErrorMessage != String.Empty)
            {
                MessageBox.Show("ERROR: " + operations.lastErrorMessage);
                return false;
            }

            rDS.RefreshClasses();
            return true;
        }

        void Import(object arg)
        {
            if (!(arg is ImportArguments)) return;

            ImportArguments args = (ImportArguments)arg;

            bool succeeded = true;
            foreach (string filename in args.Data)
            {
                succeeded = args.Operator.Import(filename, args.Providers);
                if (!succeeded) break;
            }

            AccessFDB fdb = args.dataset.Database as AccessFDB;
            if (fdb != null)
            {
                IFeatureClass fc = fdb.GetFeatureclass(args.dataset.DatasetName, args.dataset.DatasetName + "_IMAGE_POLYGONS");
                if (fc != null) fdb.CalculateExtent(fc);
                fdb.ShrinkSpatialIndex(args.dataset.DatasetName + "_IMAGE_POLYGONS");
                //if (!fdb.CalculateSpatialIndex(fdb.GetFeatureclass(args.dataset.DatasetName, args.dataset.DatasetName + "_IMAGE_POLYGONS"), 10, 0))
                //{
                //    MessageBox.Show("ERROR: CalculateSpatialIndex - " + fdb.lastErrorMsg);
                //}
            }
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (_exAppl == null) return;

            if (e.Data == null) return;

            if (e.Data.GetData(DataFormats.FileDrop) == null) return;
            e.Effect = DragDropEffects.Copy;
        }

        private void Script(DragEventArgs e)
        {
            try
            {
                if (_exAppl == null) return;

                if (e.Data == null) return;
                string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (data == null || data.Length == 0) return;

                StringBuilder sb = new StringBuilder();
                foreach (string filename in data)
                {
                    DirectoryInfo di = new DirectoryInfo(filename);
                    if (_exObject.Object is SqlFDBImageCatalogClass)
                    {
                        SqlFDBImageCatalogClass layer = (SqlFDBImageCatalogClass)_exObject.Object;
                        if (layer._fdb == null) return;

                        if (di.Exists)
                        {
                            sb.Append("\r\nCreateRasterCatalog -connectionstring \"" + layer._fdb.ConnectionString + "\" -ds \"" + layer.Name + "\" -root \"" + filename + "\" -managed");
                        }
                        else
                        {
                            //gView.Raster.File.RasterFileClass rLayer = new gView.Raster.File.RasterFileClass(null, filename);
                            //if (!rLayer.isValid) continue;
                            RasterFileDataset rDataset = new RasterFileDataset();
                            IRasterLayer rLayer = rDataset.AddRasterFile(filename);
                            if (rLayer != null)
                            {
                                sb.Append("\r\nCreateRasterCatalog -connectionstring \"" + layer._fdb.ConnectionString + "\" -ds \"" + layer.Name + "\" -append \"" + filename + "\" -managed");
                            }
                        }
                    }
                    else if (_exObject.Object is AccessFDBImageCatalogClass)
                    {
                        AccessFDBImageCatalogClass layer = (AccessFDBImageCatalogClass)_exObject.Object;
                        if (layer.Dataset == null) return;

                        if (di.Exists)
                        {
                            sb.Append("\r\nCreateRasterCatalog -mdb \"" + ConfigTextStream.ExtractValue(layer.Dataset.ConnectionString, "mdb") + "\" -ds \"" + layer.Name + "\" -root \"" + filename + "\" -managed");
                        }
                        else
                        {
                            //gView.Raster.File.RasterFileClass rLayer = new gView.Raster.File.RasterFileClass(null, filename);
                            //if (!rLayer.isValid) continue;
                            RasterFileDataset rDataset = new RasterFileDataset();
                            IRasterLayer rLayer = rDataset.AddRasterFile(filename);
                            if (rLayer != null)
                            {
                                sb.Append("\r\nCreateRasterCatalog -mdb \"" + ConfigTextStream.ExtractValue(layer.Dataset.ConnectionString, "mdb") + "\" -ds \"" + layer.Name + "\" -append \"" + filename + "\" -managed");
                            }
                        }
                    }
                }
                if (sb.ToString() != "")
                {
                    sb.Append(" -recalcspatialindex\r\n");
                    _exAppl.ExecuteBatch(sb.ToString(), new ExplorerExecuteBatchCallback(this.RefreshList));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
        #region contextMenu
        private void removeImageFromDatasetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_exObject == null || _contextItem == null) return;

            FDBImageDataset iDataset = null;
            if (_exObject.Object is SqlFDBImageCatalogClass)
            {
                SqlFDBImageCatalogClass layer = (SqlFDBImageCatalogClass)_exObject.Object;
                iDataset = new FDBImageDataset(layer._fdb, layer.Name);
            }
            else if (_exObject.Object is AccessFDBImageCatalogClass)
            {
                AccessFDBImageCatalogClass layer = (AccessFDBImageCatalogClass)_exObject.Object;
                iDataset = new FDBImageDataset(layer._fdb, layer.Name);
            }
            if (iDataset == null) return;

            if (listView.SelectedItems.Contains(_contextItem))
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    int ID = iDataset.GetImageDatasetItemID(item.Text);
                    if (ID == -1)
                    {
                        MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                        return;
                    }
                    if (!iDataset.DeleteImageDatasetItem(ID, false))
                    {
                        MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                        return;
                    }
                    listView.Items.Remove(item);
                }
            }
            else
            {
                int ID = iDataset.GetImageDatasetItemID(_contextItem.Text);
                if (ID == -1)
                {
                    MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                    return;
                }
                if (!iDataset.DeleteImageDatasetItem(ID, false))
                {
                    MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                    return;
                }
                listView.Items.Remove(_contextItem);
            }
            SetStatusLabel1Text(listView.Items.Count + " Items...");
        }

        private void updateImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_exObject == null || _contextItem == null) return;
            FDBImageDataset iDataset = null;
            if (_exObject.Object is SqlFDBImageCatalogClass)
            {
                SqlFDBImageCatalogClass layer = (SqlFDBImageCatalogClass)_exObject.Object;
                iDataset = new FDBImageDataset(layer._fdb, layer.Name);
            }
            else if (_exObject.Object is AccessFDBImageCatalogClass)
            {
                AccessFDBImageCatalogClass layer = (AccessFDBImageCatalogClass)_exObject.Object;
                iDataset = new FDBImageDataset(layer._fdb, layer.Name);
            }
            if (iDataset == null) return;

            int ID = iDataset.GetImageDatasetItemID(_contextItem.Text);
            if (ID == -1)
            {
                MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                return;
            }
            if (!iDataset.UpdateImageDatasetBitmap(ID))
            {
                MessageBox.Show(iDataset.lastErrorMessage, "ERROR");
                return;
            }

            _contextItem.ImageIndex = 0;
        }
        #endregion

        #region MouseEvents
        private int _mouseX = 0, _mouseY = 0;
        private ListViewItem _contextItem;
        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseX = e.X;
            _mouseY = e.Y;
        }
        private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _contextItem = listView.GetItemAt(e.X, e.Y);
                if (_contextItem != null)
                {
                    updateImageToolStripMenuItem.Enabled = _contextItem.ImageIndex == 1;
                    contextMenuStrip1.Show(listView, new Point(e.X, e.Y));
                }
            }
        }

        #endregion

        private void Key_pressed(object sender, KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case 116:
                    RefreshList();
                    break;
                case 27:
                    CancelRefreshList();
                    break;
                default:
                    //MessageBox.Show(e.KeyValue.ToString());
                    break;
            }
        }

        #region IOrder Members

        public int SortOrder
        {
            get { return 0; }
        }

        #endregion

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            if (openImageFiles.ShowDialog() == DialogResult.OK)
            {
                ImportFiles(openImageFiles.FileNames, true, null);
            }

            IRasterClass rc = _exObject.Object as IRasterClass;
            if (rc == null) return;
            IRasterDataset rDS = rc.Dataset as IRasterDataset;
            if (rDS == null) return;

            rDS.RefreshClasses();
        }

        private void btnImportDirectory_Click(object sender, EventArgs e)
        {
            FormSelectImageDatasetFolderAndFilter dlg = new FormSelectImageDatasetFolderAndFilter();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string rootPath = dlg.SelectedFolder;
                    string[] filters = dlg.FormatFilters;
                    Dictionary<string, Guid> providers = dlg.ProviderGuids;

                    int counter = 0;
                    if (!ImportDirectory(new DirectoryInfo(rootPath), filters, ref counter, providers))
                    {
                        //MessageBox.Show("Canceled...");
                    }
                    RefreshList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            IRasterClass rc = _exObject.Object as IRasterClass;
            if (rc == null) return;
            IRasterDataset rDS = rc.Dataset as IRasterDataset;
            if (rDS == null) return;

            rDS.RefreshClasses();
        }

        private bool ImportDirectory(DirectoryInfo di, string[] filters, ref int dirCount, Dictionary<string,Guid> providers)
        {
            //if (dirCount++ > 100) return;

            foreach (string filter in filters)
            {
                if (filter == String.Empty) continue;

                FileInfo[] fis = di.GetFiles(filter);

                if (fis.Length > 0)
                {
                    string[] filenames = new string[fis.Length];
                    for (int i = 0; i < fis.Length; i++)
                        filenames[i] = fis[i].FullName;

                    if (!ImportFiles(filenames, false, providers))
                    {
                        return false;
                    }
                }
            }

            foreach (DirectoryInfo sub in di.GetDirectories())
                if (!ImportDirectory(sub, filters, ref dirCount, providers))
                {
                    return false;
                }

            return true;
        }
    }


    internal class ImageImportProgressReporter : IProgressReporter
    {
        private ProgressReport _report = new ProgressReport();
        private ICancelTracker _cancelTracker = null;
        private int _progress = 0;

        public ImageImportProgressReporter(FDBImageDataset import, int max)
        {
            if (import == null) return;
            _cancelTracker = import.CancelTracker;

            _report.featureMax = max;
            import.ReportAction += new FDBImageDataset.ReportActionEvent(import_ReportAction);
            import.ReportProgress += new FDBImageDataset.ReportProgressEvent(import_ReportProgress);
            import.ReportRequest += new FDBImageDataset.ReportRequestEvent(import_ReportRequest);
        }

        void import_ReportRequest(FDBImageDataset sender, RequestArgs args)
        {
            args.Result = MessageBox.Show(
                args.Request,
                "Warning",
                args.Buttons,
                MessageBoxIcon.Warning);
        }

        void import_ReportProgress(FDBImageDataset sender, int progress)
        {
            if (ReportProgress == null) return;

            _progress += progress;
            _report.featureMax = Math.Max(_report.featureMax, _progress);
            _report.featurePos = _progress;

            ReportProgress(_report);
        }

        void import_ReportAction(FDBImageDataset sender, string action)
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

    internal struct ImportArguments
    {
        public ImportArguments(FDBImageDataset op, IRasterDataset ds, string[] data, Dictionary<string,Guid> providers)
        {
            Operator = op;
            dataset = ds;
            Data = data;
            Providers = providers;
        }
        public FDBImageDataset Operator;
        public IRasterDataset dataset;
        public string[] Data;
        public Dictionary<string, Guid> Providers;
    }
}

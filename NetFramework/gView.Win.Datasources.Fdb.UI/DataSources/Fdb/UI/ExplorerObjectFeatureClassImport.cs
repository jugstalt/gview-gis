using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI.Dialogs;
using gView.Framework.FDB;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;

namespace gView.DataSources.Fdb.UI
{
    public abstract class ExplorerObjectFeatureClassImport : ExplorerParentObject, IExplorerObjectContentDragDropEvents2
    {
        protected AccessFDB _fdb = null;
        protected string _dsname = "";
        protected IFeatureDataset _dataset;
        private FDBImport _import = null;

        public ExplorerObjectFeatureClassImport(IExplorerObject parent, Type type)
           : base(parent, type, 0)
        {
        }

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

        public void Content_DragDrop(DragEventArgs e, IUserData userdata)
        {
            if (_fdb == null) return;

            bool schemaOnly = false;
            if (userdata != null &&
                userdata.GetUserData("gView.Framework.UI.BaseTools.PasteSchema") != null &&
                userdata.GetUserData("gView.Framework.UI.BaseTools.PasteSchema").Equals(true))
            {
                schemaOnly = true;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
            }
            else
            {
                foreach (string format in e.Data.GetFormats())
                {
                    object ob = e.Data.GetData(format);
                    if (ob is IEnumerable<IExplorerObjectSerialization>)
                    {
                        ExplorerObjectManager exObjectManager = new ExplorerObjectManager();

                        List<IExplorerObject> exObjects = new List<IExplorerObject>(exObjectManager.DeserializeExplorerObject((IEnumerable<IExplorerObjectSerialization>)ob));
                        if (exObjects == null) return;

                        foreach (IExplorerObject exObject in ListOperations<IExplorerObject>.Clone(exObjects))
                        {
                            IFeatureClass fc = exObject.Object as IFeatureClass;
                            if (fc == null) continue;

                            if (fc.Dataset != null && fc.Dataset.DatasetName == _dsname &&
                                _fdb != null &&
                                fc.Dataset.Database is AccessFDB &&
                                ((AccessFDB)fc.Dataset.Database)._conn.ConnectionString.ToLower() ==
                                _fdb._conn.ConnectionString.ToLower())
                            {
                                exObjects.Remove(exObject);
                            }
                        }
                        if (exObjects.Count == 0) return;

                        FormFeatureclassCopy dlg = new FormFeatureclassCopy(exObjects, _dataset);
                        dlg.SchemaOnly = schemaOnly;
                        if (dlg.ShowDialog() != DialogResult.OK) continue;

                        foreach (FeatureClassListViewItem fcItem in dlg.FeatureClassItems)
                        {
                            ImportDatasetObject(fcItem, schemaOnly);
                        }
                        /*
                        foreach (IExplorerObject exObject in exObjects)
                        {
                            ImportDatasetObject(exObject.Object);
                        }
                         * */
                        exObjectManager.Dispose(); // alle ExplorerObjects wieder löschen...
                    }
                }
            }
            //_dataset.RefreshClasses();
            this.Refresh();
        }

        public void Content_DragEnter(DragEventArgs e, IUserData userdata)
        {
            if (_fdb == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    if (file.ToLower().EndsWith(".shp"))
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            else
            {
                foreach (string format in e.Data.GetFormats())
                {
                    object ob = e.Data.GetData(format);

                    if (ob is List<IExplorerObjectSerialization>)
                    {
                        //List<IExplorerObject> exObjects = ComponentManager.DeserializeExplorerObject((List<IExplorerObjectSerialization>)ob);
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
                    /*
                    if (ob is IExplorerObject)
                    {
                        IExplorerObject exObject = (IExplorerObject)ob;
                        
                        if (exObject.Object is IFeatureDataset || exObject.Object is IFeatureClass)
                        {
                            e.Effect = DragDropEffects.Copy;
                            return;
                        }
                    }
                    if (ob is List<IExplorerObject>)
                    {
                        foreach (IExplorerObject exObject in (List<IExplorerObject>)ob)
                        {
                            if (exObject.Object is IFeatureDataset || exObject.Object is IFeatureClass)
                            {
                                e.Effect = DragDropEffects.Copy;
                                return;
                            }
                        }
                    }
                     * */
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

        private void ImportDatasetObject(object datasetObject, bool schemaOnly)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements)
                {
                    if (element is IFeatureLayer)
                    {
                        ImportDatasetObject(((IFeatureLayer)element).FeatureClass, schemaOnly);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ImportAsync));

                if (_import == null)
                    _import = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }
                _import.SchemaOnly = schemaOnly;
                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_import, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Import Featureclass: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _import = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ImportAsync));

                if (_import == null)
                    _import = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }
                _import.SchemaOnly = schemaOnly;
                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_import, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Import Featureclass: " + ((FeatureClassListViewItem)datasetObject).Text;
                progress.ShowDialog();
                _import = null;
            }
        }

        private void ImportAsync(object element)
        {
            if (_fdb == null || _import == null) return;

            int maxPerNode = 256;
            int maxLevels = 30;
            if (_fdb is SqlFDB)
            {
                maxLevels = 62;
            }

            ISpatialIndexDef sIndexDef = null;
            if (_fdb is AccessFDB)
            {
                sIndexDef = ((AccessFDB)_fdb).SpatialIndexDef(_dsname);
            }

            if (element is IFeatureClass)
            {
                if (!_import.ImportToNewFeatureclass(
                    _fdb,
                    _dsname,
                    ((IFeatureClass)element).Name,
                    (IFeatureClass)element,
                    null,
                    true,
                    null,
                    sIndexDef))
                {
                    MessageBox.Show(_import.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                MSSpatialIndex msIndex=new MSSpatialIndex();
                msIndex.GeometryType = GeometryFieldType.MsGeometry;
                msIndex.SpatialIndexBounds = item.FeatureClass.Envelope;
                msIndex.Level1=msIndex.Level2=msIndex.Level3=msIndex.Level4=MSSpatialIndexLevelSize.LOW;

                if (!_import.ImportToNewFeatureclass(
                    _fdb,
                    _dsname,
                    item.TargetName,
                    item.FeatureClass,
                    item.ImportFieldTranslation,
                    true,
                    null,
                    sIndexDef))
                {
                    MessageBox.Show(_import.lastErrorMsg);
                }
            }
        }
    }

    class FeatureClassImportProgressReporter : IProgressReporter
    {
        private ProgressReport _report=new ProgressReport();
        private ICancelTracker _cancelTracker = null;

        public FeatureClassImportProgressReporter(FDBImport import,IFeatureClass source)
        {
            if (import == null) return;
            _cancelTracker = import.CancelTracker;

            if (source != null) _report.featureMax = source.CountFeatures;
            import.ReportAction += new FDBImport.ReportActionEvent(import_ReportAction);
            import.ReportProgress += new FDBImport.ReportProgressEvent(import_ReportProgress);
            import.ReportRequest += new FDBImport.ReportRequestEvent(import_ReportRequest);
        }

        void import_ReportRequest(FDBImport sender, RequestArgs args)
        {
            args.Result = MessageBox.Show(
                args.Request,
                "Warning",
                args.Buttons,
                MessageBoxIcon.Warning);
        }

        void import_ReportProgress(FDBImport sender, int progress)
        {
            if (ReportProgress == null) return;

            _report.featureMax = Math.Max(_report.featureMax, progress);
            _report.featurePos = progress;
            
            ReportProgress(_report);
        }

        void import_ReportAction(FDBImport sender, string action)
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
}

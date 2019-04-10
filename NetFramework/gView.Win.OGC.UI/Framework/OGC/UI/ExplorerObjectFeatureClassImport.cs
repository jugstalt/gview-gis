using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.UI.Dialogs;
using gView.Framework.Sys.UI;
using System.Threading.Tasks;

namespace gView.Framework.OGC.UI
{
    public class ExplorerObjectFeatureClassImport : ExplorerParentObject, IExplorerObjectContentDragDropEvents
    {
        public ExplorerObjectFeatureClassImport(IExplorerObject parent, Type type)
            : base(parent, type, 0)
        {
        }

        #region IExplorerObjectContentDragDropEvents Member

        async public void Content_DragDrop(System.Windows.Forms.DragEventArgs e)
        {
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

                        List<IExplorerObject> exObjects = await exObjectManager.DeserializeExplorerObject((IEnumerable<IExplorerObjectSerialization>)ob);
                        if (exObjects == null) return;

                        _dataset = await ((IExplorerObject)this).GetInstanceAsync() as IFeatureDataset;

                        foreach (IExplorerObject exObject in ListOperations<IExplorerObject>.Clone(exObjects))
                        {
                            IFeatureClass fc = await exObject.GetInstanceAsync() as IFeatureClass;
                            if (fc == null) continue;

                            if (fc.Dataset != null && fc.Dataset.ConnectionString.ToLower() == _dataset.ConnectionString.ToLower())
                            {
                                exObjects.Remove(exObject);
                            }
                        }
                        if (exObjects.Count == 0) return;

                        FormFeatureclassCopy dlg = await FormFeatureclassCopy.Create(exObjects, _dataset);
                        if (dlg.ShowDialog() != DialogResult.OK) continue;

                        foreach (FeatureClassListViewItem fcItem in dlg.FeatureClassItems)
                        {
                            await ImportDatasetObject(fcItem);
                        }
                        exObjectManager.Dispose(); // alle ExplorerObjects wieder löschen...
                    }
                }
            }
            await this.Refresh();
        }

        public void Content_DragEnter(System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
            }
            else
            {
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
        }

        public void Content_DragLeave(EventArgs e)
        {

        }

        public void Content_DragOver(System.Windows.Forms.DragEventArgs e)
        {

        }

        public void Content_GiveFeedback(System.Windows.Forms.GiveFeedbackEventArgs e)
        {

        }

        public void Content_QueryContinueDrag(System.Windows.Forms.QueryContinueDragEventArgs e)
        {

        }

        #endregion

        #region Import
        private FeatureImport _import = null;
        private IFeatureDataset _dataset = null;

        async private Task ImportDatasetObject(object datasetObject)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements().Result)
                {
                    if (element is IFeatureLayer)
                    {
                        await ImportDatasetObject(((IFeatureLayer)element).FeatureClass);
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

                FeatureClassImportProgressReporter reporter = await FeatureClassImportProgressReporter.Create(_import, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Import Featureclass: " + ((FeatureClassListViewItem)datasetObject).Text;
                progress.ShowDialog();
                _import = null;
            }
        }

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

                if (source != null)
                    reporter._report.featureMax = await source.CountFeatures();

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
}

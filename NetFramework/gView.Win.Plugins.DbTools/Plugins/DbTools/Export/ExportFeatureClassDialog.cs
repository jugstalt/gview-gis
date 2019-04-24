using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Explorer.UI;
using gView.Framework.FDB;
using gView.Framework.system;
using gView.Framework.system.UI;
using System.Threading;
using gView.Framework.UI.Dialogs;
using gView.Framework.Geometry;
using gView.Framework.Carto;
using gView.Framework.Globalisation;
using gView.DataSources.Fdb.UI;
using gView.Framework.UI.Controls.Filter;
using gView.DataSources.Fdb.MSAccess;

namespace gView.Plugins.DbTools.Export
{
    public partial class ExportFeatureClassDialog : Form
    {
        private IFeatureClass _sourceFeatureClass = null;
        private int _panelIndex = 0;
        private FeatureClassListViewItem _listViewItem;

        public ExportFeatureClassDialog(IDisplay display, IFeatureLayer sourceFeatureLayer)
        {
            InitializeComponent();

            _sourceFeatureClass = sourceFeatureLayer.Class as IFeatureClass;

            #region Filter
            //All Features
            //Selected Features
            //Features in actual extent
            cmbExport.Items.Add(new ExportMethodItem("All Features", null));

            if (sourceFeatureLayer is IFeatureSelection &&
                ((IFeatureSelection)sourceFeatureLayer).SelectionSet != null &&
                ((IFeatureSelection)sourceFeatureLayer).SelectionSet.Count > 0)
            {
                ISelectionSet selectionSet = ((IFeatureSelection)sourceFeatureLayer).SelectionSet;
                IQueryFilter selFilter = null;
                if (selectionSet is IIDSelectionSet)
                {
                    selFilter = new RowIDFilter(_sourceFeatureClass.IDFieldName, ((IIDSelectionSet)selectionSet).IDs);
                }
                else if (selectionSet is IGlobalIDSelectionSet)
                {
                    selFilter = new GlobalRowIDFilter(_sourceFeatureClass.IDFieldName, ((IGlobalIDSelectionSet)selectionSet).IDs);
                }
                else if (selectionSet is IQueryFilteredSelectionSet)
                {
                    selFilter = ((IQueryFilteredSelectionSet)selectionSet).QueryFilter.Clone() as IQueryFilter;
                }

                if (selFilter != null)
                {
                    selFilter.SubFields = "*";
                    ExportMethodItem item = new ExportMethodItem("Selected Features", selFilter);
                    cmbExport.Items.Add(item);
                    cmbExport.SelectedItem = item;
                }
            }

            if (display != null && display.Envelope != null)
            {
                SpatialFilter dispFilter = new SpatialFilter();
                dispFilter.SubFields = "*";

                dispFilter.FilterSpatialReference = display.SpatialReference;
                dispFilter.Geometry = display.Envelope;
                dispFilter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

                cmbExport.Items.Add(new ExportMethodItem("Features in actual extent", dispFilter));
            }

            if (cmbExport.SelectedIndex == -1)
                cmbExport.SelectedIndex = 0;
            #endregion

            _listViewItem = new FeatureClassListViewItem(_sourceFeatureClass, 255);
            gvFields.DataSource = _listViewItem.Fields;

            panelStep1.Dock = panelStep2.Dock = DockStyle.Fill;
            SetPanelVisibity();
        }

        async private void btnSelect_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureDatasetFilter());

            ExplorerDialog exDlg = new ExplorerDialog("New Target Featureclass",
                SaveFeatureClassFilters.AllFilters,
                false);

            if (exDlg.ShowDialog() == DialogResult.OK &&
                exDlg.ExplorerObjects.Count == 1)
            {
                IExplorerObject parentObject = exDlg.ExplorerObjects[0];
                var instance = await parentObject.GetInstanceAsync();

                if (instance is IFeatureDataset &&
                    ((IDataset)instance).Database is IFeatureUpdater)
                {
                    _dataset = (IFeatureDataset)instance;
                }
                else if (exDlg.SelectedExplorerDialogFilter.FilterObject is IFeatureDataset &&
                    ((IDataset)exDlg.SelectedExplorerDialogFilter.FilterObject).Database is IFileFeatureDatabase)
                {
                    IFileFeatureDatabase fileDB = (IFileFeatureDatabase)((IFeatureDataset)exDlg.SelectedExplorerDialogFilter.FilterObject).Database;

                    _dataset = await fileDB.GetDataset(parentObject.FullName);
                }
                else
                {
                    MessageBox.Show("Can't determine target featureclass!");
                    return;
                }

                txtDatasetName.Text = _dataset.Database.ToString();
                txtDatasetLocation.Text = parentObject.FullName;
                txtTargetClass.Text = _listViewItem.TargetName = exDlg.TargetName;
            }
            btnNext.Enabled = exDlg.TargetName != String.Empty;
        }

        async private void btnNext_Click(object sender, EventArgs e)
        {
            if (btnNext.Text == LocalizedResources.GetResString("String.Export", "Export"))
            {
                if (_listViewItem != null)
                {
                    ExportDatasetObject(_listViewItem);
                    _destDatasetElement = await _dataset.Element(_listViewItem.TargetName);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                _panelIndex++;
                SetPanelVisibity();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _panelIndex--;
            SetPanelVisibity();
        }

        private void cmbExport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbExport.SelectedItem is ExportMethodItem)
            {
                _filter = ((ExportMethodItem)cmbExport.SelectedItem).QueryFilter;
            }
        }

        private void SetPanelVisibity()
        {
            switch (_panelIndex)
            {
                case 0:
                    panelStep2.Visible = false;
                    panelStep1.Visible = true;
                    btnBack.Visible = false;
                    btnNext.Enabled = (txtTargetClass.Text != String.Empty);
                    btnNext.Text = LocalizedResources.GetResString("String.Next", "Next") + " >";
                    break;
                case 1:
                    panelStep1.Visible = false;
                    panelStep2.Visible = true;
                    btnBack.Visible = true;
                    btnNext.Text = LocalizedResources.GetResString("String.Export", "Export");

                    if (_listViewItem.MaxFieldLength > MaximumFieldLength(_dataset))
                    {
                        _listViewItem = new FeatureClassListViewItem(_sourceFeatureClass, MaximumFieldLength(_dataset));
                        gvFields.DataSource = _listViewItem.Fields;
                    }
                    break;
            }
        }

        private IDatasetElement _destDatasetElement = null;
        public IDatasetElement DestinationDatasetElement
        {
            get { return _destDatasetElement; }
        }

        #region Helper
        private int MaximumFieldLength(IDataset dataset)
        {
            MaximumFieldnameLength maxLength = Attribute.GetCustomAttribute(dataset.GetType(), typeof(MaximumFieldnameLength)) as MaximumFieldnameLength;
            if (maxLength != null)
                return maxLength.Value;

            return 255;
        }
        private class ExportMethodItem
        {
            private string _text;
            private IQueryFilter _filter;
            public ExportMethodItem(string text, IQueryFilter filter)
            {
                _text = text;
                _filter = filter;
            }

            public IQueryFilter QueryFilter
            {
                get { return _filter; }
            }

            public override string ToString()
            {
                return _text;
            }
        }
        #endregion

        #region Export
        private FeatureImport _export = null;
        private FDBImport _fdbExport = null;
        private IFeatureDataset _dataset = null;
        private IQueryFilter _filter = null;

        private void ExportDatasetObject(object datasetObject)
        {
            if (_dataset.Database is AccessFDB)
            {
                ExportDatasetObject_fdb(datasetObject);
            }
            else
            {
                ExportDatasetObject_db(datasetObject);
            }
        }

        #region FDB
        private void ExportDatasetObject_fdb(object datasetObject)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements().Result)
                {
                    if (element is IFeatureLayer)
                    {
                        ExportDatasetObject(((IFeatureLayer)element).FeatureClass);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_fdb));

                if (_fdbExport == null)
                    _fdbExport = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_fdbExport, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _fdbExport = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_fdb));

                if (_fdbExport == null)
                    _fdbExport = new FDBImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_fdbExport, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((FeatureClassListViewItem)datasetObject).TargetName;
                progress.ShowDialog();
                _fdbExport = null;
            }
        }
        private void ExportAsync_fdb(object element)
        {
            if (_fdbExport == null) return;

            List<IQueryFilter> filters = null;
            if (_filter != null)
            {
                filters = new List<IQueryFilter>();
                filters.Add(_filter);
            }

            if (element is IFeatureClass)
            {
                if (!_fdbExport.ImportToNewFeatureclass(
                    _dataset.Database as IFeatureDatabase,
                    _dataset.DatasetName,
                    _listViewItem.TargetName,
                    (IFeatureClass)element,
                    null,
                    true,
                    filters).Result)
                {
                    MessageBox.Show(_export.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                if (!_fdbExport.ImportToNewFeatureclass(
                    _dataset.Database as IFeatureDatabase,
                    _dataset.DatasetName,
                    item.TargetName,
                    item.FeatureClass,
                    item.ImportFieldTranslation,
                    true,
                    filters).Result)
                {
                    MessageBox.Show(_fdbExport.lastErrorMsg);
                }
            }
        }
        #endregion

        #region All other DBs
        private void ExportDatasetObject_db(object datasetObject)
        {
            if (datasetObject is IFeatureDataset)
            {
                IFeatureDataset dataset = (IFeatureDataset)datasetObject;
                foreach (IDatasetElement element in dataset.Elements().Result)
                {
                    if (element is IFeatureLayer)
                    {
                        ExportDatasetObject(((IFeatureLayer)element).FeatureClass);
                    }
                }
            }
            if (datasetObject is IFeatureClass)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_db));

                if (_export == null)
                    _export = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_export, (IFeatureClass)datasetObject);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((IFeatureClass)datasetObject).Name;
                progress.ShowDialog();
                _export = null;
            }
            if (datasetObject is FeatureClassListViewItem)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ExportAsync_db));

                if (_export == null)
                    _export = new FeatureImport();
                else
                {
                    MessageBox.Show("ERROR: Import already runnung");
                    return;
                }

                FeatureClassImportProgressReporter reporter = new FeatureClassImportProgressReporter(_export, ((FeatureClassListViewItem)datasetObject).FeatureClass);

                FormProgress progress = new FormProgress(reporter, thread, datasetObject);
                progress.Text = "Export Features: " + ((FeatureClassListViewItem)datasetObject).TargetName;
                progress.ShowDialog();
                _export = null;
            }
        }
        private void ExportAsync_db(object element)
        {
            if (_export == null) return;

            List<IQueryFilter> filters = null;
            if (_filter != null)
            {
                filters = new List<IQueryFilter>();
                filters.Add(_filter);
            }

            if (element is IFeatureClass)
            {
                if (!_export.ImportToNewFeatureclass(
                    _dataset,
                    ((IFeatureClass)element).Name,
                    (IFeatureClass)element,
                    null,
                    true,
                    filters).Result)
                {
                    MessageBox.Show(_export.lastErrorMsg);
                }
            }
            else if (element is FeatureClassListViewItem)
            {
                FeatureClassListViewItem item = element as FeatureClassListViewItem;
                if (item.FeatureClass == null) return;

                if (!_export.ImportToNewFeatureclass(
                    _dataset,
                    item.TargetName,
                    item.FeatureClass,
                    item.ImportFieldTranslation,
                    true,
                    filters).Result)
                {
                    MessageBox.Show(_export.lastErrorMsg);
                }
            }
        }
        #endregion

        class FeatureClassImportProgressReporter : IProgressReporter
        {
            private ProgressReport _report = new ProgressReport();
            private ICancelTracker _cancelTracker = null;

            public FeatureClassImportProgressReporter(object import, IFeatureClass source)
            {
                if (import == null) return;

                if (import is FDBImport)
                {
                    _cancelTracker = ((FDBImport)import).CancelTracker;

                    if (source != null) _report.featureMax = source.CountFeatures().Result;
                    ((FDBImport)import).ReportAction += new FDBImport.ReportActionEvent(FeatureClassImportProgressReporter_ReportAction);
                    ((FDBImport)import).ReportProgress += new FDBImport.ReportProgressEvent(FeatureClassImportProgressReporter_ReportProgress);
                    ((FDBImport)import).ReportRequest += new FDBImport.ReportRequestEvent(FeatureClassImportProgressReporter_ReportRequest);
                }
                if (import is FeatureImport)
                {
                    _cancelTracker = ((FeatureImport)import).CancelTracker;

                    if (source != null) _report.featureMax = source.CountFeatures().Result;
                    ((FeatureImport)import).ReportAction += new FeatureImport.ReportActionEvent(import_ReportAction);
                    ((FeatureImport)import).ReportProgress += new FeatureImport.ReportProgressEvent(import_ReportProgress);
                    ((FeatureImport)import).ReportRequest += new FeatureImport.ReportRequestEvent(import_ReportRequest);
                }
            }

            #region FDB
            void FeatureClassImportProgressReporter_ReportRequest(FDBImport sender, gView.DataSources.Fdb.UI.RequestArgs args)
            {
                args.Result = MessageBox.Show(
                    args.Request,
                    "Warning",
                    args.Buttons,
                    MessageBoxIcon.Warning);
            }

            void FeatureClassImportProgressReporter_ReportProgress(FDBImport sender, int progress)
            {
                if (ReportProgress == null) return;

                _report.featureMax = Math.Max(_report.featureMax, progress);
                _report.featurePos = progress;

                ReportProgress(_report);
            }

            void FeatureClassImportProgressReporter_ReportAction(FDBImport sender, string action)
            {
                if (ReportProgress == null) return;

                _report.featurePos = 0;
                _report.Message = action;

                ReportProgress(_report);
            }
            #endregion

            void import_ReportRequest(FeatureImport sender, gView.Framework.system.UI.RequestArgs args)
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
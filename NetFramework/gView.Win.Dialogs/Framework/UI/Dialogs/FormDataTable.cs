using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormDataTable : UserControl, IDockableWindow
    {
        private enum ViewMode { all, selected, mapextent }
        private enum ExecuteMethod { Refresh = 0, Pause = 1, Continue = 2 }

        private ViewMode _viewMode = ViewMode.all;
        private ExecuteMethod _executeMethod = ExecuteMethod.Refresh;

        private IDatasetElement _dsElement = null;
        private ITableClass _class = null;
        private IQueryFilter _filter = null;
        private FillTableThread _tabWorker;
        private Thread _workerThread;
        private ICancelTracker _cancelTracker;
        private IMapDocument _doc = null;
        private Color _selectionColor = Color.Aqua;
        private bool _startOnShown = true;
        private string _filterQueryClause = String.Empty;

        public FormDataTable()
        {
            //Init();
        }

        public FormDataTable(IDatasetElement dsElement)
            : this()
        {
            InitializeComponent();

            _dsElement = dsElement;
            if (_dsElement is IFeatureLayer &&
                ((IFeatureLayer)_dsElement).FilterQuery != null)
            {
                _filterQueryClause = ((IFeatureLayer)_dsElement).FilterQuery.WhereClause;
            }

            if (_dsElement != null)
            {
                _dsElement.PropertyChanged += new PropertyChangedHandler(dsElement_PropertyChanged);
            }

            if (dsElement is IFeatureLayer)
            {
                TableClass = ((IFeatureLayer)dsElement).FeatureClass;
            }
            else if (dsElement is ITableLayer)
            {
                TableClass = ((ITableLayer)dsElement).TableClass;
            }

            if (_dsElement is IFeatureSelection)
            {
                ((IFeatureSelection)_dsElement).FeatureSelectionChanged += new FeatureSelectionChangedEvent(OnFeatureSelectionChanged);
                ((IFeatureSelection)_dsElement).BeforeClearSelection += new BeforeClearSelectionEvent(OnBeforeClearSelection);
            }
        }

        void dsElement_PropertyChanged()
        {
            if (_dsElement is IFeatureLayer)
            {
                IFeatureLayer fLayer = (IFeatureLayer)_dsElement;
                if (fLayer.FilterQuery != null &&
                    !(fLayer.FilterQuery.WhereClause.Equals(_filterQueryClause)))
                {
                    _filterQueryClause = fLayer.FilterQuery.WhereClause;
                    this.TableClass = fLayer.FeatureClass;
                    StartWorkerThread();
                }
            }
        }

        #region IDockableWindow Members

        private DockWindowState _dockState = DockWindowState.none;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _dockState;
            }
            set
            {
                _dockState = value;
            }
        }

        public Image Image
        {
            get { return null; }
        }

        #endregion

        public IMapDocument MapDocument
        {
            set
            {
                _doc = value;
                HookAllMaps();
            }
        }

        public ITableClass TableClass
        {
            get { return _class; }
            set
            {
                _class = value;
                gridView.Columns.Clear();
                if (_tabWorker != null)
                {
                    _tabWorker.TableClass = _class;
                }

                if (_class == null)
                {
                    ReleaseCursor();
                    return;
                }
                Init();
                MakeFilter(null);
            }
        }
        public string FilterQueryClause
        {
            get { return _filterQueryClause; }
            set { _filterQueryClause = value; }
        }

        private void Init()
        {
            _cancelTracker = new CancelTracker();
            _workerThread = null;

            if (_tabWorker != null)
            {
                _tabWorker.ReleaseCursor();
            }

            _tabWorker = new FillTableThread(this.TableClass, _cancelTracker);
            _tabWorker.FirstFillMaximum = 1000;

            _tabWorker.Progress += new FillTableThread.ProgressEvent(tabWorker_Progress);
            _tabWorker.Paused += new FillTableThread.PausedEvent(tabWorker_Paused);
            _tabWorker.ThreadFinished += new FillTableThread.ThreadFinishedEvent(tabWorker_ThreadFinished);
        }

        public void ReleaseCursor()
        {
            if (_tabWorker != null)
            {
                _tabWorker.ReleaseCursor();
            }
        }
        internal bool StartWorkerOnShown
        {
            get { return _startOnShown; }
            set { _startOnShown = value; }
        }
        internal bool ShowExtraButtons
        {
            set
            {
                toolStripDropDownButtonShow.Visible =
                    toolStripSeparator2.Visible =
                    toolStripDropDownButton1.Visible =
                    toolStripButton1.Visible = value;

            }
        }
        private void MakeFilter(IEnvelope env)
        {
            if (env == null)
            {
                _filter = new QueryFilter();
            }
            else
            {
                _filter = new SpatialFilter();
                ((ISpatialFilter)_filter).Geometry = env;
            }

            RefreshFilterFields();

            if (_filterQueryClause != null)
            {
                _filter.WhereClause = _filterQueryClause;
            }
        }
        private void RefreshFilterFields()
        {
            if (_filter != null)
            {
                _filter.SubFields = String.Empty;
                foreach (IField field in _class.Fields.ToEnumerable())
                {
                    if (field.type == FieldType.binary || field.type == FieldType.Shape)
                    {
                        continue;
                    }

                    _filter.AddField(field.name);
                }
            }
        }

        private void FormDataTable_Load(object sender, EventArgs e)
        {
            if (_startOnShown)
            {
                StartWorkerThread();
            }
        }

        //private void FormDataTable_Shown(object sender, EventArgs e)
        //{
        //    if (_startOnShown)
        //        StartWorkerThread();
        //}

        private void OnFeatureSelectionChanged(IFeatureSelection sender)
        {
            switch (_viewMode)
            {
                case ViewMode.all:
                case ViewMode.mapextent:
                    MarkSelectedRows(_selectionColor);
                    SetNavigatorCountItemText();
                    break;
                case ViewMode.selected:
                    StartWorkerThread();
                    break;
            }
        }

        private void OnBeforeClearSelection(IFeatureSelection sender)
        {
            switch (_viewMode)
            {
                case ViewMode.all:
                case ViewMode.mapextent:
                    UnmarkSelection();
                    break;
                case ViewMode.selected:
                    break;
            }
        }

        private void HookAllMaps()
        {
            if (_doc == null)
            {
                return;
            }

            UnhookAllMaps();
            foreach (IMap map in _doc.Maps)
            {
                map.NewExtentRendered += new NewExtentRenderedEvent(OnNewExtentRendered);
            }
        }

        private void UnhookAllMaps()
        {
            if (_doc == null)
            {
                return;
            }

            foreach (IMap map in _doc.Maps)
            {
                map.NewExtentRendered -= new NewExtentRenderedEvent(OnNewExtentRendered);
            }
        }

        private delegate void OnNewExtentRenderedCallback(IMap map, IEnvelope envelope);
        private void OnNewExtentRendered(IMap sender, gView.Framework.Geometry.IEnvelope extent)
        {
            if (gridView.InvokeRequired)
            {
                OnNewExtentRenderedCallback d = new OnNewExtentRenderedCallback(this.OnNewExtentRendered);
                this.Invoke(d, new object[] { sender, extent });
            }
            else
            {
                if (_viewMode == ViewMode.mapextent)
                {
                    if (_filter is ISpatialFilter)
                    {
                        ((ISpatialFilter)_filter).Geometry = extent;
                        if (sender != null && sender.Display != null)
                        {
                            ((ISpatialFilter)_filter).FilterSpatialReference = sender.Display.SpatialReference;
                        }
                    }
                    else
                    {
                        MakeFilter(extent);
                    }

                    StartWorkerThread();
                }
            }
        }

        #region WorkerThread
        internal void StartWorkerThread()
        {
            if (_tabWorker == null || this.Visible == false)
            {
                return;
            }

            gridView.DataSource = null;

            CancelWorkerThread(false);
            ((CancelTracker)_cancelTracker).Reset();

            gridNavigator.BindingSource = null;
            gridSource.DataSource = null;

            _tabWorker.CleanUp();

            EnableCancelThreadButton(true);
            EnableExecuteButton(true, 1);

            _workerThread = new Thread(new ParameterizedThreadStart(_tabWorker.Start));
            _workerThread.Priority = ThreadPriority.Lowest;

            if (_viewMode == ViewMode.selected)
            {
                if (_dsElement is IFeatureSelection &&
                    ((IFeatureSelection)_dsElement).SelectionSet is IIDSelectionSet)
                {
                    IIDSelectionSet selSet = (IIDSelectionSet)((IFeatureSelection)_dsElement).SelectionSet;
                    if (selSet != null && selSet.IDs != null)
                    {
                        _workerThread.Start(selSet.IDs);
                    }
                }
            }
            else
            {
                RefreshFilterFields();
                _workerThread.Start(_filter);
            }
        }

        private void CancelWorkerThread(bool pause)
        {
            if (pause)
            {
                _cancelTracker.Pause();
            }
            else
            {
                _cancelTracker.Cancel();
            }

            if (_workerThread == null)
            {
                return;
            }

            if (_workerThread.IsAlive)
            {
                //
                // Events zurücksetzen,
                // da ansonsten beim Join() ein deadlocking auftritt...
                //
                _tabWorker.ThreadFinished -= new FillTableThread.ThreadFinishedEvent(tabWorker_ThreadFinished);
                _tabWorker.Paused -= new FillTableThread.PausedEvent(tabWorker_Paused);
                _tabWorker.Progress -= new FillTableThread.ProgressEvent(tabWorker_Progress);

                _cancelTracker.Cancel();
                _workerThread.Abort();
                _workerThread.Join(1000);

                _tabWorker.ThreadFinished += new FillTableThread.ThreadFinishedEvent(tabWorker_ThreadFinished);
                _tabWorker.Paused += new FillTableThread.PausedEvent(tabWorker_Paused);
                _tabWorker.Progress += new FillTableThread.ProgressEvent(tabWorker_Progress);

            }
            _workerThread = null;

            if (pause)
            {
                tabWorker_Paused();
            }
            else
            {
                tabWorker_ThreadFinished();
            }

            EnableCancelThreadButton(false);
        }

        void tabWorker_Progress()
        {
            if (_tabWorker.Table != null)
            {
                SetGridTable(_tabWorker.Table, false);
            }
        }

        private delegate void tabworker_PausedCallback();
        void tabWorker_Paused()
        {
            if (gridView.InvokeRequired)
            {
                tabworker_PausedCallback d = new tabworker_PausedCallback(this.tabWorker_Paused);
                this.Invoke(d);
            }
            else
            {
                _workerThread = null;
                EnableCancelThreadButton(true);
                EnableExecuteButton(true, 2);

                if (_tabWorker.Table != null)
                {
                    SetGridTable(_tabWorker.Table, true);
                }

                if (_viewMode != ViewMode.selected)
                {
                    MarkSelection();
                }
            }
        }

        private delegate void tabworker_FinishedCallBack();
        void tabWorker_ThreadFinished()
        {
            if (gridView.InvokeRequired)
            {
                tabworker_FinishedCallBack d = new tabworker_FinishedCallBack(this.tabWorker_ThreadFinished);
                this.Invoke(d);
            }
            else
            {
                _workerThread = null;
                EnableCancelThreadButton(false);
                EnableExecuteButton(true, 0);

                if (_tabWorker.Table != null)
                {
                    SetGridTable(_tabWorker.Table, true);
                }

                if (_viewMode != ViewMode.selected)
                {
                    MarkSelection();
                }
            }
        }
        #endregion

        #region gridView Callbacks
        private delegate void SetGridTableCallback(DataTable tab, bool bindNavigator);
        private void SetGridTable(DataTable tab, bool bindNavigator)
        {
            if (gridView.InvokeRequired)
            {
                SetGridTableCallback d = new SetGridTableCallback(this.SetGridTable);
                this.Invoke(d, new object[] { tab, bindNavigator });
            }
            else
            {
                if (gridSource.DataSource != tab)
                {
                    gridSource.DataSource = tab;
                    //gridView.Columns.Clear();
                    gridView.DataSource = gridSource;
                }
                bindingNavigatorCountItem.Text = "*" + gridView.Rows.Count;

                if (bindNavigator)
                {
                    SetNavigatorCountItemText();

                    gridNavigator.BindingSource = gridSource;
                }
            }
        }

        private void SetNavigatorCountItemText()
        {
            gridNavigator.CountItemFormat =
                (_tabWorker.HasMore) ? "*{0}" : "{0}";
            if (_dsElement is IFeatureSelection)
            {
                if (((IFeatureSelection)_dsElement).SelectionSet == null)
                {
                    gridNavigator.CountItemFormat = "0 of " + gridNavigator.CountItemFormat + " selected";
                }
                else if (((IFeatureSelection)_dsElement).SelectionSet.Count == 0)
                {
                    gridNavigator.CountItemFormat = "0 of " + gridNavigator.CountItemFormat + " selected";
                }
                else
                {
                    gridNavigator.CountItemFormat = ((IFeatureSelection)_dsElement).SelectionSet.Count + " of " + gridNavigator.CountItemFormat + " selected";
                }
            }
        }

        private delegate void SetRowBackColorCallback(int index, Color back);
        private void SetRowBackColor(int index, Color back)
        {
            if (gridView.InvokeRequired)
            {
                SetRowBackColorCallback d = new SetRowBackColorCallback(this.SetRowBackColor);
                this.Invoke(d, new object[] { index, back });
            }
            else
            {
                if (index > gridView.Rows.Count)
                {
                    return;
                }

                gridView.Rows[index].DefaultCellStyle.BackColor = back;
            }
        }
        #endregion

        #region Navigation Callbacks
        private delegate void EnableCancelThreadButtonEvent(bool enable);
        private void EnableCancelThreadButton(bool enable)
        {
            if (gridNavigator.InvokeRequired)
            {
                EnableCancelThreadButtonEvent d = new EnableCancelThreadButtonEvent(this.EnableCancelThreadButton);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                toolStripButtonCancelThread.Enabled = enable;
            }
        }

        private delegate void EnableExecuteButtonEvent(bool enable, int imageIndex);
        private void EnableExecuteButton(bool enable, int imageIndex)
        {
            if (gridNavigator.InvokeRequired)
            {
                EnableExecuteButtonEvent d = new EnableExecuteButtonEvent(this.EnableExecuteButton);
                this.Invoke(d, new object[] { enable, imageIndex });
            }
            else
            {
                toolStripButtonExecute.Image = ilExecute.Images[imageIndex];
                _executeMethod = (ExecuteMethod)imageIndex;
                toolStripButtonExecute.Enabled = enable;
            }
        }
        #endregion

        private void toolStripButtonCancelThread_Click(object sender, EventArgs e)
        {
            CancelWorkerThread(false);
        }

        private void toolStripButtonExecute_Click(object sender, EventArgs e)
        {
            switch (_executeMethod)
            {
                case ExecuteMethod.Refresh:
                    StartWorkerThread();
                    break;
                case ExecuteMethod.Pause:
                    CancelWorkerThread(true);
                    break;
                case ExecuteMethod.Continue:
                    if (_workerThread != null)
                    {
                        break;
                    }

                    gridNavigator.BindingSource = null;

                    ((CancelTracker)_cancelTracker).Reset();
                    _workerThread = new Thread(new ThreadStart(_tabWorker.Continue));
                    _workerThread.Start();

                    EnableCancelThreadButton(true);
                    EnableExecuteButton(true, (int)ExecuteMethod.Pause);
                    break;
            }
        }

        private void gridView_Sorted(object sender, EventArgs e)
        {
            //MessageBox.Show("sorted "+_class.IDFieldName);
            MarkSelection();
        }

        #region MouseSelection
        private bool _selChanged = false;
        private bool _mouseSelect = false, _mouseDown = false;
        private bool _shiftKey = false;
        private bool _ctrlKey = false;
        private void gridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_mouseSelect)
            {
                _selChanged = true;
            }
        }

        private void ChangeSelection()
        {
            if (!(_dsElement is IFeatureSelection) && _class != null)
            {
                return;
            } ((IFeatureSelection)_dsElement).ClearSelection();
            IIDSelectionSet selSet = ((FeatureSelection)_dsElement).SelectionSet as IIDSelectionSet;
            if (selSet == null)
            {
                return;
            }

            foreach (DataGridViewRow row in gridView.SelectedRows)
            {
                try
                {
                    selSet.AddID((int)row.Cells[_class.IDFieldName].Value);
                    row.DefaultCellStyle.BackColor = _selectionColor;
                }
                catch
                {
                }
            }

            SetNavigatorCountItemText();

            if (_doc != null && _doc.Application is IMapApplication)
            {
                ((IMapApplication)_doc.Application).RefreshActiveMap(gView.Framework.Carto.DrawPhase.Selection);
            }
            ((IFeatureSelection)_dsElement).FireSelectionChangedEvent();
        }

        private void MarkSelection()
        {
            MarkSelectedRows(_selectionColor);
        }

        private void UnmarkSelection()
        {
            //MarkSelectedRows(Color.White);
            for (int i = 0; i < gridView.Rows.Count; i++)
            {
                gridView.Rows[i].DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void MarkSelectedRows(Color col)
        {
            if (gridView.DataSource == null)
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            if (_dsElement is IFeatureSelection && gridSource.DataSource is DataTable)
            {
                IIDSelectionSet selSet = ((IFeatureSelection)_dsElement).SelectionSet as IIDSelectionSet;
                if (selSet != null && selSet.IDs != null)
                {
                    //foreach (int id in selSet.IDs)
                    //{
                    //    try
                    //    {
                    //        //int index = gridSource.Find(_class.IDFieldName, id);
                    //        //DataRow row = ((DataTable)gridSource.DataSource).Select(_class.IDFieldName + "=" + id)[0];
                    //        int index = -1;
                    //        for (int i = 0; i < gridSource.Count; i++)
                    //        {
                    //            if ((int)((DataRowView)gridSource[i])[0] == id)
                    //            {
                    //                index = i;
                    //                break;
                    //            }
                    //        }
                    //        if (index != -1) gridView.Rows[index].DefaultCellStyle.BackColor = col;

                    //        //toolStripStatusLabel1.Text = id.ToString();
                    //    }
                    //    catch
                    //    {
                    //        return;
                    //    }
                    //}


                    List<int> ids = ListOperations<int>.Clone(selSet.IDs);
                    ids.Sort();

                    if (ids.Count != 0)
                    {
                        for (int i = 0; i < gridSource.Count; i++)
                        {
                            if (ids.BinarySearch((int)((DataRowView)gridSource[i])[_class.IDFieldName]) >= 0)
                            {
                                gridView.Rows[i].DefaultCellStyle.BackColor = col;
                                ids.Remove((int)((DataRowView)gridSource[i])[_class.IDFieldName]);
                            }
                        }
                    }
                    else
                    {
                        gridView.ClearSelection();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void gridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (_viewMode == ViewMode.selected)
            {
                gridView.DefaultCellStyle.SelectionBackColor = Color.Yellow;
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (e.X >= 0 && e.X <= gridView.RowHeadersWidth /* && e.Y > gridView.ColumnHeadersHeight*/)
            {
                _mouseSelect = _mouseDown = true;
                gridView.DefaultCellStyle.SelectionBackColor = _selectionColor;

                if (!_ctrlKey && !_shiftKey && _dsElement is IFeatureSelection)
                {
                    ((IFeatureSelection)_dsElement).ClearSelection();
                }
            }
            else
            {
                _mouseSelect = false;
                gridView.DefaultCellStyle.SelectionBackColor = Color.Yellow;
            }
        }

        private void gridView_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            if (_shiftKey || _ctrlKey || _viewMode == ViewMode.selected)
            {
                return;
            }

            _mouseSelect = false;
            if (_selChanged)
            {
                _selChanged = false;
                ChangeSelection();
            }
        }

        private void gridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 16)
            {
                _shiftKey = true;
            }

            if (e.KeyValue == 17)
            {
                _ctrlKey = true;
            }
        }
        private void gridView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 16)
            {
                _shiftKey = false;
            }

            if (e.KeyValue == 17)
            {
                _ctrlKey = false;
            }

            if (_mouseDown || _shiftKey || _ctrlKey || _viewMode == ViewMode.selected)
            {
                return;
            }

            if (_selChanged)
            {
                _selChanged = false;
                ChangeSelection();
            }
        }
        #endregion

        private void gridView_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void toolStripMenuItemShowAll_Click(object sender, EventArgs e)
        {
            this.ReleaseCursor();
            toolStripDropDownButtonShow.Image = toolStripMenuItemShowAll.Image;
            _viewMode = ViewMode.all;

            gridView.DefaultCellStyle.BackColor = gridView.DefaultCellStyle.SelectionBackColor = Color.White;
            if (_filter is ISpatialFilter)
            {
                MakeFilter(null);
            }

            StartWorkerThread();
        }

        private void toolStripMenuItemShowSelected_Click(object sender, EventArgs e)
        {
            this.ReleaseCursor();
            toolStripDropDownButtonShow.Image = toolStripMenuItemShowSelected.Image;
            _viewMode = ViewMode.selected;

            gridView.DefaultCellStyle.BackColor = gridView.DefaultCellStyle.SelectionBackColor = _selectionColor;
            if (_filter is ISpatialFilter)
            {
                MakeFilter(null);
            }

            StartWorkerThread();
        }

        private void toolStripMenuItemShowFeaturesInExtent_Click(object sender, EventArgs e)
        {
            this.ReleaseCursor();
            toolStripDropDownButtonShow.Image = toolStripMenuItemShowFeaturesInExtent.Image;
            _viewMode = ViewMode.mapextent;

            gridView.DefaultCellStyle.BackColor = gridView.DefaultCellStyle.SelectionBackColor = Color.White;
            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
            {
                OnNewExtentRendered(_doc.FocusMap, _doc.FocusMap.Display.Envelope);
            }
        }

        async private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_dsElement is IFeatureLayer && _dsElement is IFeatureSelection)
            {
                FormQueryBuilder dlg = await FormQueryBuilder.CreateAsync((IFeatureLayer)_dsElement);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = dlg.whereClause;

                    await ((IFeatureSelection)_dsElement).Select(filter, dlg.combinationMethod);
                    ((IFeatureSelection)_dsElement).FireSelectionChangedEvent();

                    if (_doc != null)
                    {
                        await _doc.FocusMap.RefreshMap(DrawPhase.Selection, null);
                    }
                }
            }
        }

        private void toolStripDropDownButton2_DropDownOpening(object sender, EventArgs e)
        {
            toolStripDropDownButton2.DropDownItems.Clear();

            if (_doc != null && _doc.TableRelations != null)
            {
                foreach (ITableRelation tableRelation in _doc.TableRelations.GetRelations(_dsElement))
                {
                    toolStripDropDownButton2.DropDownItems.Add(new RelationTreeNode(_doc, tableRelation, _dsElement));
                }
            }
        }

        #region Item Classes

        internal class RelationTreeNode : ToolStripMenuItem
        {
            private IMapDocument _mapDocument;

            public RelationTreeNode(IMapDocument mapDocument, ITableRelation tableRelation, IDatasetElement element)
            {
                _mapDocument = mapDocument;
                this.TableRelation = tableRelation;
                this.DatasetElement = element;

                IDatasetElement target = (tableRelation.LeftTable == element) ? tableRelation.RightTable : tableRelation.LeftTable;
                base.Text = tableRelation.RelationName + " (" + target.Title + ")";

                base.Click += new EventHandler(RelationTreeNode_Click);
            }

            #region Events

            async private void RelationTreeNode_Click(object sender, EventArgs e)
            {
                IFeatureSelection target = (this.TableRelation.LeftTable == this.DatasetElement ? this.TableRelation.RightTable : this.TableRelation.LeftTable) as IFeatureSelection;
                if (target == null)
                {
                    return;
                }

                target.ClearSelection();

                if (this.DatasetElement is IFeatureSelection &&
                    ((IFeatureSelection)this.DatasetElement).SelectionSet is IIDSelectionSet)
                {
                    IIDSelectionSet selSet = (IIDSelectionSet)((IFeatureSelection)this.DatasetElement).SelectionSet;
                    if (selSet != null && selSet.IDs != null && selSet.IDs.Count > 0)
                    {
                        string relationField = this.TableRelation.LeftTable == this.DatasetElement ? this.TableRelation.LeftTableField : this.TableRelation.RightTableField;
                        RowIDFilter filter = new RowIDFilter(((ITableClass)this.DatasetElement.Class).IDFieldName);
                        filter.IDs = selSet.IDs;
                        filter.SubFields = relationField;
                        ICursor cursor = await ((ITableClass)this.DatasetElement.Class).Search(filter);

                        IRow row = null;
                        while ((row = await Next(cursor)) != null)
                        {
                            object val = row[relationField];
                            if (val == null)
                            {
                                continue;
                            }

                            IQueryFilter selFilter = this.TableRelation.LeftTable == this.DatasetElement ? this.TableRelation.GetRightFilter("*", val) : this.TableRelation.GetLeftFilter("*", val);
                            await target.Select(selFilter, CombinationMethod.Union);
                        }
                    }
                }

                target.FireSelectionChangedEvent();

                if (_mapDocument != null && _mapDocument.Application is IMapApplication)
                {
                    await ((IMapApplication)_mapDocument.Application).RefreshActiveMap(DrawPhase.Selection);
                }
            }

            #endregion

            #region Helper

            async private Task<IRow> Next(ICursor cursor)
            {
                if (cursor is IFeatureCursor)
                {
                    return await ((IFeatureCursor)cursor).NextFeature();
                }

                if (cursor is IRowCursor)
                {
                    return await ((IRowCursor)cursor).NextRow();
                }

                return null;
            }

            #endregion

            public ITableRelation TableRelation { get; set; }
            public IDatasetElement DatasetElement { get; set; }
        }

        #endregion
    }

    internal class FillTableThread : IDisposable
    {
        public delegate void ProgressEvent();
        public event ProgressEvent Progress = null;
        public delegate void ThreadFinishedEvent();
        public event ThreadFinishedEvent ThreadFinished = null;
        public delegate void PausedEvent();
        public event PausedEvent Paused = null;

        private ITableClass _class;
        private ICancelTracker _cancelTracker;
        private int _firstFillMaximum = -1;
        private FeatureTable _fTab = null;
        private ICursor _cursor = null;

        public FillTableThread(ITableClass Class, ICancelTracker cancelTracker)
        {
            _class = Class;
            _cancelTracker = cancelTracker;
        }
        public int FirstFillMaximum
        {
            get { return _firstFillMaximum; }
            set { _firstFillMaximum = value; }
        }

        public ITableClass TableClass
        {
            get { return _class; }
            set { _class = value; }
        }
        public bool HasMore
        {
            get { return _fTab != null ? _fTab.hasMore : false; }
        }
        public void CleanUp()
        {
            if (_fTab != null)
            {
                _fTab.ReleaseCursor();
                _fTab.Dispose();
                _fTab = null;
            }
        }

        public DataTable Table
        {
            get { return _fTab != null ? _fTab.Table : null; }
        }

        // Thread
        public void Start(object filter)
        {
            CleanUp();

            try
            {
                if (_class is IFeatureClass)
                {
                    if (filter is List<int>)
                    {
                        ProcessFeatureClassByID(_class as IFeatureClass, filter as List<int>).Wait();
                    }
                    else
                    {
                        ProcessFeatureClass(_class as IFeatureClass, filter as IQueryFilter).Wait();
                    }
                }
                else if (_class is ITableClass)
                {
                    ProcessTableClass(_class as ITableClass, filter as IQueryFilter);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.AllMessages(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Continue()
        {
            ((CancelTracker)_cancelTracker).Reset();

            if (_fTab != null)
            {
                try
                {
                    _fTab.Fill(_cancelTracker);


                    if (!_fTab.hasMore ||
                        (_cancelTracker != null && !_cancelTracker.Continue && !_cancelTracker.Paused))
                    {
                        _fTab.ReleaseCursor();
                        _cursor = null;
                        if (ThreadFinished != null)
                        {
                            ThreadFinished();
                        }
                    }
                    else
                    {
                        if (Paused != null)
                        {
                            Paused();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ThreadFinished != null)
                    {
                        ThreadFinished();
                    }

                    MessageBox.Show(ex.AllMessages(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (ThreadFinished != null)
                {
                    ThreadFinished();
                }
            }
        }

        async private Task ProcessFeatureClassByID(IFeatureClass fc, List<int> ids)
        {
            CleanUp();
            if (fc != null)
            {
                _fTab = new FeatureTable(null, fc.Fields, fc);

                //if (Progress != null) Progress();
                await _fTab.Fill(ids, _cancelTracker);

                if (!_fTab.hasMore ||
                    (_cancelTracker != null && !_cancelTracker.Continue && !_cancelTracker.Paused))
                {
                    _fTab.ReleaseCursor();
                    _cursor = null;
                    if (ThreadFinished != null)
                    {
                        ThreadFinished();
                    }
                }
                else
                {
                    if (Paused != null)
                    {
                        Paused();
                    }
                }
            }
            else
            {
                if (ThreadFinished != null)
                {
                    ThreadFinished();
                }
            }
        }

        async private Task ProcessFeatureClass(IFeatureClass fc, IQueryFilter filter)
        {
            CleanUp();
            if (fc != null)
            {
                _cursor = await fc.Search(filter);
                if (_cursor == null)
                {
                    return;
                }

                _fTab = new FeatureTable(_cursor as IFeatureCursor, fc.Fields, fc);
                _fTab.RowsAddedToTable += new RowsAddedToTableEvent(fTab_RowsAddedToTable);

                try
                {
                    await _fTab.Fill(_firstFillMaximum, _cancelTracker);
                }
                catch (System.Threading.ThreadAbortException)
                {
                    _fTab.ReleaseCursor();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!_fTab.hasMore ||
                    (_cancelTracker != null && !_cancelTracker.Continue && !_cancelTracker.Paused))
                {
                    _fTab.ReleaseCursor();
                    _cursor = null;
                    if (ThreadFinished != null)
                    {
                        ThreadFinished();
                    }
                }
                else
                {
                    if (Paused != null)
                    {
                        Paused();
                    }
                }
            }
            else
            {
                if (ThreadFinished != null)
                {
                    ThreadFinished();
                }
            }
        }

        void fTab_RowsAddedToTable(int count)
        {
            if (Progress != null)
            {
                Progress();
            }
        }

        private void ProcessTableClass(ITableClass tc, IQueryFilter filter)
        {
            /*
            if (tc != null)
            {

                IRowCursor cursor = tc.Search(filter) as IRowCursor;
                if (cursor == null) return;

                RowTable ftab = new RowTable(cursor, tc.Fields);
                ftab.Fill(200);
                _table = ftab.Table;

                while (ftab.hasMore)
                {
                    if (_cancelTracker != null && !_cancelTracker.Continue) break;
                    if (Progress != null) Progress();
                    ftab.Fill(200);
                }

                cursor.Release();
            }
            if (ThreadFinished != null) ThreadFinished();
             * */
        }

        public void ReleaseCursor()
        {
            if (_fTab != null)
            {
                _fTab.ReleaseCursor();
            }
        }

        #region IDisposable Member

        public void Dispose()
        {
            CleanUp();
        }

        #endregion
    }
}
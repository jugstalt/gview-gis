using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Cursors;
using gView.Framework.Data.Filters;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    /// <summary>
    /// Zusammenfassung für FormDataTable.
    /// </summary>
    public class FormDataTable2 : System.Windows.Forms.Form, IDockableWindow
    {
        private IContainer components = null;
        private IDatasetElement _dsElement;
        private ITableClass _tableClass;
        private ITable _result;
        //private DataSet _ds;
        private IMap _map = null;
        private ToolStrip toolStrip1;
        private ToolStripButton btnFirst;
        private ToolStripButton btnPrev;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripTextBox txtPos;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnNext;
        private ToolStripButton btnLast;
        private ToolStripLabel toolStripLabel2;
        private ToolStripButton btnMoveToRecord;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripLabel tsLabelCount;
        private ToolStripComboBox cmbShow;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripLabel tsLabelSelected;
        private ToolStripLabel toolStripLabel3;
        private ToolStripLabel toolStripLabel4;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar progress1;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem toolStripMenuItem1;
        private IMapDocument _doc = null;
        private ToolStripMenuItem toolStripMenuItem_LoadAllSelectedRecords;
        private ToolStripSeparator toolStripSeparator6;
        private DataGridView grid;
        private int _maxNewSelectedRows = 200;
        private VScrollBar vScrollBar;
        private ToolStripButton toolStripButton_CloseWindow;
        private ToolStripSeparator toolStripSeparator7;
        private DataGridView dataGridView1;
        private List<int> _selectedRowIndices = new List<int>();

        public FormDataTable2(IDatasetElement dsElement)
        {
            InitializeComponent();

            _tableClass = null;
            if (dsElement is IFeatureLayer)
            {
                _tableClass = ((IFeatureLayer)dsElement).FeatureClass;
            }
            else if (dsElement is ITableLayer)
            {
                _tableClass = ((ITableLayer)dsElement).TableClass;
            }

            _dsElement = dsElement;

            cmbShow.SelectedIndex = 0;

            if (_dsElement is IFeatureSelection)
            {
                ((IFeatureSelection)_dsElement).FeatureSelectionChanged -= new FeatureSelectionChangedEvent(FeatureSelectionChanged);
                ((IFeatureSelection)_dsElement).FeatureSelectionChanged += new FeatureSelectionChangedEvent(FeatureSelectionChanged);
            }
        }

        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public IMapDocument MapDocument
        {
            set
            {
                if (value == null)
                {
                    _doc = null;
                    _map = null;
                    return;
                }
                _map = value.FocusMap;
                _doc = value;
            }
        }
        public ITableClass TableClass
        {
            get { return _tableClass; }
        }

        #region Vom Windows Form-Designer generierter Code
        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDataTable2));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cmbShow = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFirst = new System.Windows.Forms.ToolStripButton();
            this.btnPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.txtPos = new System.Windows.Forms.ToolStripTextBox();
            this.btnMoveToRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnNext = new System.Windows.Forms.ToolStripButton();
            this.btnLast = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsLabelSelected = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.tsLabelCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_LoadAllSelectedRecords = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_CloseWindow = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progress1 = new System.Windows.Forms.ToolStripProgressBar();
            this.grid = new System.Windows.Forms.DataGridView();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.cmbShow,
            this.toolStripSeparator4,
            this.btnFirst,
            this.btnPrev,
            this.toolStripSeparator1,
            this.txtPos,
            this.btnMoveToRecord,
            this.toolStripSeparator2,
            this.btnNext,
            this.btnLast,
            this.toolStripSeparator3,
            this.tsLabelSelected,
            this.toolStripLabel3,
            this.tsLabelCount,
            this.toolStripLabel4,
            this.toolStripSeparator5,
            this.toolStripDropDownButton1,
            this.toolStripSeparator7,
            this.toolStripButton_CloseWindow});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.ShowItemToolTips = false;
            this.toolStrip1.Size = new System.Drawing.Size(699, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(37, 22);
            this.toolStripLabel2.Text = "Show:";
            // 
            // cmbShow
            // 
            this.cmbShow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbShow.Items.AddRange(new object[] {
            "All",
            "Selected"});
            this.cmbShow.Name = "cmbShow";
            this.cmbShow.Size = new System.Drawing.Size(80, 25);
            this.cmbShow.SelectedIndexChanged += new System.EventHandler(this.cmbShow_SelectedIndexChanged);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnFirst
            // 
            this.btnFirst.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFirst.Image = ((System.Drawing.Image)(resources.GetObject("btnFirst.Image")));
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.RightToLeftAutoMirrorImage = true;
            this.btnFirst.Size = new System.Drawing.Size(23, 22);
            this.btnFirst.Text = "Move first";
            this.btnFirst.Click += new System.EventHandler(this.btnMoveFirst_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnPrev.Image")));
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.RightToLeftAutoMirrorImage = true;
            this.btnPrev.Size = new System.Drawing.Size(23, 22);
            this.btnPrev.Text = "Move previous";
            this.btnPrev.Click += new System.EventHandler(this.btnMovePrev_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // txtPos
            // 
            this.txtPos.AccessibleName = "Position";
            this.txtPos.AutoSize = false;
            this.txtPos.Name = "txtPos";
            this.txtPos.Size = new System.Drawing.Size(50, 21);
            this.txtPos.Text = "0";
            this.txtPos.ToolTipText = "Current position";
            // 
            // btnMoveToRecord
            // 
            this.btnMoveToRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveToRecord.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveToRecord.Image")));
            this.btnMoveToRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveToRecord.Name = "btnMoveToRecord";
            this.btnMoveToRecord.Size = new System.Drawing.Size(23, 22);
            this.btnMoveToRecord.Text = "toolStripButton1";
            this.btnMoveToRecord.Click += new System.EventHandler(this.btnMoveTo_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnNext
            // 
            this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNext.Image")));
            this.btnNext.Name = "btnNext";
            this.btnNext.RightToLeftAutoMirrorImage = true;
            this.btnNext.Size = new System.Drawing.Size(23, 22);
            this.btnNext.Text = "Move next";
            this.btnNext.Click += new System.EventHandler(this.btnMoveNext_Click);
            // 
            // btnLast
            // 
            this.btnLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLast.Image = ((System.Drawing.Image)(resources.GetObject("btnLast.Image")));
            this.btnLast.Name = "btnLast";
            this.btnLast.RightToLeftAutoMirrorImage = true;
            this.btnLast.Size = new System.Drawing.Size(23, 22);
            this.btnLast.Text = "Move last";
            this.btnLast.Click += new System.EventHandler(this.btnMoveLast_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tsLabelSelected
            // 
            this.tsLabelSelected.Name = "tsLabelSelected";
            this.tsLabelSelected.Size = new System.Drawing.Size(13, 22);
            this.tsLabelSelected.Text = "0";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(17, 22);
            this.toolStripLabel3.Text = "of";
            // 
            // tsLabelCount
            // 
            this.tsLabelCount.Name = "tsLabelCount";
            this.tsLabelCount.Size = new System.Drawing.Size(13, 22);
            this.tsLabelCount.Text = "0";
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(47, 22);
            this.toolStripLabel4.Text = "selected";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_LoadAllSelectedRecords,
            this.toolStripSeparator6,
            this.toolStripMenuItem1});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(57, 22);
            this.toolStripDropDownButton1.Text = "Options";
            // 
            // toolStripMenuItem_LoadAllSelectedRecords
            // 
            this.toolStripMenuItem_LoadAllSelectedRecords.Name = "toolStripMenuItem_LoadAllSelectedRecords";
            this.toolStripMenuItem_LoadAllSelectedRecords.Size = new System.Drawing.Size(208, 22);
            this.toolStripMenuItem_LoadAllSelectedRecords.Text = "Load All Selected Records";
            this.toolStripMenuItem_LoadAllSelectedRecords.Click += new System.EventHandler(this.toolStripMenuItem_LoadAllSelectedRecords_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(205, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(208, 22);
            this.toolStripMenuItem1.Text = "Select By Attributes";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_CloseWindow
            // 
            this.toolStripButton_CloseWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_CloseWindow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_CloseWindow.Image")));
            this.toolStripButton_CloseWindow.Name = "toolStripButton_CloseWindow";
            this.toolStripButton_CloseWindow.RightToLeftAutoMirrorImage = true;
            this.toolStripButton_CloseWindow.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_CloseWindow.Text = "Delete";
            this.toolStripButton_CloseWindow.Click += new System.EventHandler(this.toolStripButton_CloseWindow_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progress1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 402);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(699, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progress1
            // 
            this.progress1.Name = "progress1";
            this.progress1.Size = new System.Drawing.Size(200, 16);
            // 
            // grid
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Cyan;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(0, 25);
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.grid.ShowCellToolTips = false;
            this.grid.ShowEditingIcon = false;
            this.grid.Size = new System.Drawing.Size(670, 134);
            this.grid.TabIndex = 5;
            this.grid.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.grid_RowHeaderMouseClick);
            this.grid.SizeChanged += new System.EventHandler(this.grid_SizeChanged);
            this.grid.Sorted += new System.EventHandler(this.grid_Sorted);
            this.grid.CurrentCellChanged += new System.EventHandler(this.grid_CurrentCellChanged);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(682, 25);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 377);
            this.vScrollBar.TabIndex = 6;
            this.vScrollBar.ValueChanged += new System.EventHandler(this.vScrollBar_ValueChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 166);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(497, 202);
            this.dataGridView1.TabIndex = 7;
            // 
            // FormDataTable
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(699, 424);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "FormDataTable";
            this.ShowInTaskbar = false;
            this.Text = "FormDataTable";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.FormDataTable_Closing);
            this.Load += new System.EventHandler(this.FormDataTable_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        async private void FormDataTable_Load(object sender, System.EventArgs e)
        {
            var value = _tableClass;

            _maxNewSelectedRows = 200;
            _selectedRowIndices = new List<int>();
            grid.DataSource = null;

            _tableClass = value;
            if (_tableClass == null)
            {
                return;
            }

            grid.DefaultCellStyle.BackColor = Color.White;

            QueryFilter filter = new QueryFilter();
            foreach (IField field in _tableClass.Fields.ToEnumerable())
            {
                if (field.type == FieldType.binary || field.type == FieldType.Shape)
                {
                    continue;
                }

                filter.AddField(field.name);
            }
            //filter.SubFields="*";

            ICursor cursor = await _tableClass.Search(filter);
            if (cursor is IFeatureCursor)
            {
                _result = new FeatureTable((IFeatureCursor)cursor, _tableClass.Fields, _tableClass);
            }
            else if (cursor is IRowCursor)
            {
                _result = new RowTable((IRowCursor)cursor, _tableClass.Fields);
            }
            else
            {
                return;
            }

            _result.RowsAddedToTable -= new RowsAddedToTableEvent(RowsAddedToTable);

            progress1.Minimum = 0;
            progress1.Visible = true;
            progress1.Value = 0;
            await _result.Fill(progress1.Maximum = 200);

            _result.RowsAddedToTable += new RowsAddedToTableEvent(RowsAddedToTable);

            _viewTable = null; // in PrepareDataView neu anlegen...
            PrepareDataView(_result.Table);
            cmbShow.SelectedIndex = 0;  // hier wird auch MarkSelected durchgeführt
            progress1.Visible = false;
        }

        private void FeatureSelectionChanged(IFeatureSelection sender)
        {
            if (_dsElement is IFeatureSelection)
            {
                ISelectionSet selSet = ((IFeatureSelection)_dsElement).SelectionSet;
                if (selSet is IIDSelectionSet)
                {
                    if (_selectedIDs != null)
                    {
                        _selectedIDs.Clear();
                    }

                    _selectedIDs = ((IIDSelectionSet)selSet).IDs;
                    _selectedIDs.Sort();
                }
                else if (selSet == null)
                {
                    if (_selectedIDs != null)
                    {
                        _selectedIDs.Clear();
                    }

                    _selectedIDs = null;
                }
            }
            if (cmbShow.SelectedIndex == 1)
            {
                vScrollBar.Value = 0;
            }
            markSelected(_maxNewSelectedRows);
        }

        private void markSelected(int maxNewSelectedRows)
        {
            if (_result == null)
            {
                return;
            }

            tsLabelCount.Text = _result.Table.Rows.Count.ToString();
            tsLabelCount.Text += (_result.hasMore) ? "*" : "";
            tsLabelSelected.Text = "0";

            int count = 0;
            if (grid.DataSource != null)
            {
                txtPos.Enabled = true;
                txtPos.Text = (grid.CurrentCellAddress.Y + 1).ToString();
                count = ((DataView)grid.DataSource).Count;
                //for (int i = 0; i < count; i++) grid.UnSelect(i);
            }
            else
            {
                txtPos.Enabled = false;
                txtPos.Text = "";
            }
            if (!(_dsElement is IFeatureSelection))
            {
                return;
            }

            IFeatureSelection selection = (IFeatureSelection)_dsElement;

            tsLabelSelected.Text = "0";
            tsLabelCount.Text = _result.Table.Rows.Count.ToString();
            tsLabelCount.Text += (_result.hasMore) ? "*" : "";

            if (!(selection.SelectionSet is IIDSelectionSet))
            {
                return;
            }

            List<int> IDs = ((IIDSelectionSet)selection.SelectionSet).IDs;

            if (maxNewSelectedRows != 0)
            {
                // Noch fehlende IDs suchen
                List<int> ids = ListOperations<int>.Clone(IDs);

                int alreayIncluded = 0;
                ids.Sort();
                foreach (int id in IDs)
                {
                    if (_result.Table.Select(_tableClass.IDFieldName + "=" + id.ToString()).Length > 0)
                    {
                        ids.Remove(id);
                        alreayIncluded++;
                        if (alreayIncluded >= maxNewSelectedRows && maxNewSelectedRows > 0)
                        {
                            ids.Clear();
                        }
                    }
                }
                if (ids.Count > 0)
                {
                    if (maxNewSelectedRows > 0 && ids.Count > maxNewSelectedRows)
                    {
                        ids.RemoveRange(maxNewSelectedRows, ids.Count - maxNewSelectedRows);
                    }
                    grid.DataSource = null;
                    progress1.Value = 0;
                    progress1.Maximum = ids.Count;
                    progress1.Visible = true;
                    _result.FillAtLeast(ids);
                    progress1.Visible = false;
                }
            }

            if (cmbShow.SelectedIndex == 1)
            {
                grid.DefaultCellStyle.BackColor = Color.FromArgb(128, 255, 255);
                grid.DefaultCellStyle.SelectionBackColor = Color.Yellow;
                PrepareSelectionFilter();
            }
            else
            {
                grid.DefaultCellStyle.SelectionBackColor = Color.White;
                grid.DefaultCellStyle.SelectionForeColor = Color.Blue;
                grid.DefaultCellStyle.BackColor = Color.White;
                grid.ForeColor = Color.Black;
                PrepareDataView(_result.Table);
            }

            tsLabelCount.Text = _result.Table.Rows.Count.ToString();
            tsLabelCount.Text += (_result.hasMore) ? "*" : "";
            tsLabelSelected.Text = IDs.Count.ToString();
        }

        private int _viewPos = 0;
        private DataTable _viewTable = null;
        private DataRow[] _sortedViewRows = null;
        private DataRow[] _selectedViewRows = null;
        private List<int> _selectedIDs = null;
        private void PrepareDataView(DataTable table)
        {
            if (table == null)
            {
                return;
            }
            //return table.DefaultView;

            if (_viewTable == null)
            {
                _viewTable = table.Clone();
            }

            int max = (grid.Height - grid.ColumnHeadersHeight) / 22 + 1;
            if (max == 0)
            {
                return;
            }

            _viewPos = vScrollBar.Value;
            int cols = _viewTable.Columns.Count;

            _viewTable.Rows.Clear();

            if (cmbShow.SelectedIndex == 0)
            {
                vScrollBar.Maximum = table.Rows.Count;
                for (int i = _viewPos; i < Math.Min(_viewPos + max, ((_sortedViewRows != null) ? _sortedViewRows.Length : table.Rows.Count)); i++)
                {
                    DataRow row = _viewTable.NewRow();
                    for (int c = 0; c < cols; c++)
                    {
                        row[c] = ((_sortedViewRows != null) ? _sortedViewRows[i][c] : table.Rows[i][c]);
                    }
                    _viewTable.Rows.Add(row);
                }
                grid.DataSource = _viewTable.DefaultView;

                if (cmbShow.SelectedIndex == 0 && _selectedIDs != null)
                {
                    // Selection
                    string idField = _tableClass.IDFieldName;
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (_selectedIDs.IndexOf(Convert.ToInt32(row.Cells[idField].Value)) != -1)
                        {
                            row.DefaultCellStyle.BackColor = row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(128, 255, 255);
                        }
                    }
                }
            }
            else
            {
                if (_selectedViewRows != null)
                {
                    vScrollBar.Maximum = _selectedViewRows.Length;
                    for (int i = _viewPos; i < Math.Min(_viewPos + max, _selectedViewRows.Length); i++)
                    {
                        DataRow row = _viewTable.NewRow();
                        for (int c = 0; c < cols; c++)
                        {
                            row[c] = _selectedViewRows[i][c];
                        }
                        _viewTable.Rows.Add(row);
                    }
                }
                else
                {
                    grid.DataSource = null;
                }
                grid.DataSource = _viewTable.DefaultView;
            }
        }

        private void Select(List<int> IDs)
        {
            /*
            UnselectAll();
            if (_tableClass == null) return;

            string idField = _tableClass.IDFieldName;
            int index = 0;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (IDs.IndexOf(row.Cells[idField].Value) != -1)
                {
                    row.DefaultCellStyle.BackColor = row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(128, 255, 255);
                    _selectedRowIndices.Add(index);
                }
                index++;
            }
            */
        }

        private void UnselectAll()
        {
            int max = grid.Rows.Count;
            foreach (int index in _selectedRowIndices)
            {
                if (index >= max)
                {
                    continue;
                }

                DataGridViewRow row = grid.Rows[index];

                row.DefaultCellStyle.BackColor = row.DefaultCellStyle.SelectionBackColor = Color.White;
            }
            _selectedRowIndices.Clear();
        }

        private void cmbShow_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (_dsElement == null || _result == null || _tableClass == null)
            {
                return;
            }

            //DataView dv=_result.Table.DefaultView;

            switch (cmbShow.SelectedIndex)
            {
                case 0:
                    if (_selectedTable != null)
                    {
                        _selectedTable.Dispose();
                        _selectedTable = null;
                    }
                    PrepareDataView(_result.Table);
                    break;
                case 1:
                    PrepareSelectionFilter();
                    break;
            }
            vScrollBar.Value = 0;
            markSelected(0);
        }

        private DataTable _selectedTable = null;
        private void PrepareSelectionFilter()
        {
            if (_dsElement == null || _result == null || _tableClass == null)
            {
                return;
            }

            if (grid.DataSource != _result.Table.DefaultView && grid.DataSource is DataView)
            {
                ((DataView)grid.DataSource).Table.Dispose();
                grid.DataSource = null;
            }

            if (!(_dsElement is IFeatureSelection))
            {
                return;
            }

            if (!(((IFeatureSelection)_dsElement).SelectionSet is IIDSelectionSet))
            {
                grid.DataSource = null;
                return;
            }
            List<int> IDs = ((IIDSelectionSet)((IFeatureSelection)_dsElement).SelectionSet).IDs;
            if (IDs.Count == 0)
            {
                grid.DataSource = null;
                return;
            }

            /*
            _selectedTable = _result.Table.Clone();
            int cols=_selectedTable.Columns.Count;

            foreach (object id in IDs)
            {
                DataRow[] rows = _result.Table.Select(_tableClass.IDFieldName + "=" + id.ToString());
                if (rows.Length == 0) continue;
                DataRow row = _selectedTable.NewRow();
                for(int i=0;i<cols;i++) row[i]=rows[0][i];
                _selectedTable.Rows.Add(row);
            }

            PrepareDataView(_selectedTable);
            */

            _selectedIDs = IDs;
            DataRow[] selRows = (DataRow[])Array.CreateInstance(typeof(DataRow), _selectedIDs.Count);
            int index = 0;
            if (_sortedViewRows == null)
            {
                foreach (object id in IDs)
                {
                    DataRow[] rows = _result.Table.Select(_tableClass.IDFieldName + "=" + id.ToString());
                    if (rows.Length == 0)
                    {
                        continue;
                    } ((Array)selRows).SetValue(rows[0], index++);
                }
            }
            else
            {
                foreach (DataRow row in _sortedViewRows)
                {
                    if (IDs.IndexOf(Convert.ToInt32(row[_tableClass.IDFieldName])) == -1)
                    {
                        continue;
                    } ((Array)selRows).SetValue(row, index++);
                }
            }
            _selectedViewRows = (DataRow[])Array.CreateInstance(typeof(DataRow), index);
            Array.Copy(selRows, _selectedViewRows, index);

            PrepareDataView(_result.Table);
        }

        private void grid_Scroll(object sender, System.EventArgs e)
        {

        }

        private void grid_CurrentCellChanged(object sender, System.EventArgs e)
        {
            //txtPos.Text=(grid.CurrentCell.RowNumber+1).ToString();
            txtPos.Text = (grid.CurrentCellAddress.Y + 1).ToString();
        }

        private void btnMoveNext_Click(object sender, System.EventArgs e)
        {
            //if(grid.CurrentRowIndex>=_result.Table.Rows.Count-1) return;
            //grid.CurrentRowIndex++;
            if (grid.CurrentCellAddress.Y >= _result.Table.Rows.Count - 1)
            {
                return;
            }

            grid.CurrentCell = grid[grid.CurrentCellAddress.X, grid.CurrentCellAddress.Y + 1];
        }

        private void btnMovePrev_Click(object sender, System.EventArgs e)
        {
            //if(grid.CurrentRowIndex==0) return;
            //grid.CurrentRowIndex--;
            if (grid.CurrentCellAddress.Y == 0)
            {
                return;
            }

            grid.CurrentCell = grid[grid.CurrentCellAddress.X, grid.CurrentCellAddress.Y - 1];
        }

        private void btnMoveFirst_Click(object sender, System.EventArgs e)
        {
            //grid.CurrentRowIndex=0;
            grid.CurrentCell = grid[grid.CurrentCellAddress.X, 0];
        }

        private void btnMoveLast_Click(object sender, System.EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            if (_result.Table == null)
            {
                return;
            }

            if (_result.hasMore)
            {
                grid.DataSource = null;

                progress1.Value = 0;
                progress1.Visible = true;
                _result.Fill(progress1.Maximum = 2000);
                progress1.Visible = false;

                PrepareDataView(_result.Table);
                markSelected(_maxNewSelectedRows);
            }
            //grid.CurrentRowIndex=_result.Table.Rows.Count-1;
            grid.CurrentCell = grid[grid.CurrentCellAddress.X, grid.Rows.Count - 1];
        }

        private void grid_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.UnselectAll();
            grid_Click(sender, e);
        }

        private void grid_Click(object sender, System.EventArgs e)
        {
            if (grid.DataSource == null)
            {
                return;
            }

            if (!(_dsElement is IFeatureSelection) || _tableClass.IDFieldName == String.Empty)
            {
                return;
            }

            if (((IFeatureSelection)_dsElement).SelectionSet == null)
            {
                ((IFeatureSelection)_dsElement).SelectionSet = new IDSelectionSet();
            }
            else
            {
                ((IFeatureSelection)_dsElement).SelectionSet.Clear();
            }
            this.Cursor = Cursors.WaitCursor;
            int count = ((DataView)grid.DataSource).Count;

            if (((IFeatureSelection)_dsElement).SelectionSet is IIDSelectionSet)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Selected)
                    {
                        int id = Convert.ToInt32(row.Cells[_tableClass.IDFieldName].Value);
                        ((IIDSelectionSet)((IFeatureSelection)_dsElement).SelectionSet).AddID(id);
                    }
                }
                Select(((IDSelectionSet)((IFeatureSelection)_dsElement).SelectionSet).IDs);
            }
            ((IFeatureSelection)_dsElement).FeatureSelectionChanged -= new FeatureSelectionChangedEvent(FeatureSelectionChanged);
            ((IFeatureSelection)_dsElement).FireSelectionChangedEvent();
            ((IFeatureSelection)_dsElement).FeatureSelectionChanged += new FeatureSelectionChangedEvent(FeatureSelectionChanged);

            if (_doc != null)
            {
                _doc.FocusMap.RefreshMap(DrawPhase.Selection, null);
            }

            this.Cursor = Cursors.Default;
        }

        private void FormDataTable_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_dsElement is IFeatureSelection)
            {
                ((IFeatureSelection)_dsElement).FeatureSelectionChanged -= new FeatureSelectionChangedEvent(FeatureSelectionChanged);
            }
        }

        private void MoveTo(int pos)
        {
            if (_result == null)
            {
                txtPos.Text = "0";
            }

            if (pos > _result.Table.Rows.Count)
            {
                grid.DataSource = null;

                progress1.Value = 0;
                progress1.Visible = true;
                progress1.Maximum = pos - _result.Table.Rows.Count;
                while (true)
                {
                    int nextN = Math.Min(pos - _result.Table.Rows.Count, 20000);
                    if (nextN <= 0 || !_result.hasMore)
                    {
                        break;
                    }

                    _result.Fill(progress1.Maximum = nextN);
                }
                progress1.Visible = false;

                PrepareDataView(_result.Table);
                markSelected(_maxNewSelectedRows);
            }

            //grid.CurrentRowIndex=Math.Min(_result.Table.Rows.Count-1,pos-1);
            //txtPos.Text=(grid.CurrentRowIndex+1).ToString();
        }

        private void btnMoveTo_Click(object sender, System.EventArgs e)
        {
            try
            {
                int pos = Convert.ToInt32(txtPos.Text);
                MoveTo(pos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                txtPos.Text = (grid.CurrentCellAddress.Y + 1).ToString();
            }
        }

        private void RowsAddedToTable(int count)
        {
            if (progress1.Maximum >= progress1.Value + count)
            {
                progress1.Value += count;
            }
            else
            {
                progress1.Value = 0;
            }
            statusStrip1.Refresh();
        }

        #region resizing
        /*
		int _width,_height,_left,_top;
		bool _isMinimized=false,_resizing=false;
		private void FormDataTable_Resize(object sender, System.EventArgs e)
		{
			if(_resizing) return;
			_resizing=true;
			if(this.WindowState==FormWindowState.Minimized && this.Parent!=null) 
			{
				_isMinimized=true;
				
				this.Width=100;
				this.Height=23;
				this.WindowState=FormWindowState.Normal;
				this.Left=this.Parent.Controls.IndexOf(this)*100;
				this.Top=this.Parent.ClientRectangle.Height-this.Height;
			}
			else if(this.Parent!=null && _isMinimized) 
			{
				_isMinimized=false;
				this.WindowState=FormWindowState.Normal;
				this.Left=_left;
				this.Top=_top;
				this.Width=_width;
				this.Height=_height;
			}
			else 
			{
				_width=this.Width;
				_height=this.Height;
			}
			_resizing=false;
		}

		private void FormDataTable_Move(object sender, System.EventArgs e)
		{
			if(this.WindowState!=FormWindowState.Normal) 
			{
				_left=this.Left;
				_top =this.Top;
			}
		}
		*/
        #endregion

        #region IDockableWindow Members

        private DockWindowState _dockState = DockWindowState.bottom;
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

        private void toolStripMenuItem_LoadAllSelectedRecords_Click(object sender, EventArgs e)
        {
            markSelected(-1);
        }

        private void grid_SizeChanged(object sender, EventArgs e)
        {
            if (!vScrollBar.Visible)
            {
                return;
            }

            if (_result == null)
            {
                return;
            }

            PrepareDataView(_result.Table);
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            PrepareDataView(_result.Table);
        }

        private void grid_Sorted(object sender, EventArgs e)
        {
            if (_result == null || _viewTable == null)
            {
                return;
            }

            if (_sortedViewRows != null)
            {
                // Dispose ?
            }
            _sortedViewRows = _result.Table.Select("", _viewTable.DefaultView.Sort);
            if (cmbShow.SelectedIndex == 0)
            {
                PrepareDataView(_result.Table);
            }
            else
            {
                PrepareSelectionFilter();
            }
            GC.Collect();
        }

        private void toolStripButton_CloseWindow_Click(object sender, EventArgs e)
        {
            if (_result != null)
            {
                _result.Table.Dispose();
            }
            GC.Collect();
            this.Close();
        }
    }
}

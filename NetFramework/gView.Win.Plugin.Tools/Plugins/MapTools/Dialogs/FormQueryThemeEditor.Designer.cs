namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormQueryThemeEditor
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormQueryThemeEditor));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnAdd = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnAddQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddSeperator = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddTable = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnMoveUp = new System.Windows.Forms.ToolStripButton();
            this.btnMoveDown = new System.Windows.Forms.ToolStripButton();
            this.treeQueries = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dgLayers = new System.Windows.Forms.DataGridView();
            this.colField = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colPrompt = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colOperator = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dgQueries = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInputPrompt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObliging = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabControlLayers = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgVisibleFields = new System.Windows.Forms.DataGridView();
            this.fieldCol1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fieldCol2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldCol3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldCol4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chkUseStandard = new System.Windows.Forms.CheckBox();
            this.groupPrimaryField = new System.Windows.Forms.GroupBox();
            this.cmbPrimaryField = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgLayers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgQueries)).BeginInit();
            this.tabControlLayers.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgVisibleFields)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupPrimaryField.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdd,
            this.btnRemove,
            this.toolStripSeparator1,
            this.btnMoveUp,
            this.btnMoveDown});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(584, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnAdd
            // 
            this.btnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAddQuery,
            this.btnAddSeperator,
            this.btnAddTable});
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(29, 22);
            this.btnAdd.Text = "Add";
            // 
            // btnAddQuery
            // 
            this.btnAddQuery.Image = global::gView.Plugins.Tools.Properties.Resources.search;
            this.btnAddQuery.Name = "btnAddQuery";
            this.btnAddQuery.Size = new System.Drawing.Size(133, 22);
            this.btnAddQuery.Text = "Query";
            this.btnAddQuery.Click += new System.EventHandler(this.btnAddQuery_Click);
            // 
            // btnAddSeperator
            // 
            this.btnAddSeperator.Image = global::gView.Plugins.Tools.Properties.Resources.seperator;
            this.btnAddSeperator.Name = "btnAddSeperator";
            this.btnAddSeperator.Size = new System.Drawing.Size(133, 22);
            this.btnAddSeperator.Text = "Seperator";
            this.btnAddSeperator.Click += new System.EventHandler(this.btnAddSeperator_Click);
            // 
            // btnAddTable
            // 
            this.btnAddTable.Enabled = false;
            this.btnAddTable.Image = global::gView.Plugins.Tools.Properties.Resources.tab;
            this.btnAddTable.Name = "btnAddTable";
            this.btnAddTable.Size = new System.Drawing.Size(133, 22);
            this.btnAddTable.Text = "Table";
            this.btnAddTable.Click += new System.EventHandler(this.btnAddTable_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(23, 22);
            this.btnRemove.Text = "btnRemove";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveUp.Image")));
            this.btnMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(23, 22);
            this.btnMoveUp.Text = "Move Up";
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveDown.Image")));
            this.btnMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(23, 22);
            this.btnMoveDown.Text = "Move Down";
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // treeQueries
            // 
            this.treeQueries.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeQueries.FullRowSelect = true;
            this.treeQueries.HideSelection = false;
            this.treeQueries.ImageIndex = 0;
            this.treeQueries.ImageList = this.imageList1;
            this.treeQueries.LabelEdit = true;
            this.treeQueries.Location = new System.Drawing.Point(0, 25);
            this.treeQueries.Name = "treeQueries";
            this.treeQueries.SelectedImageIndex = 0;
            this.treeQueries.Size = new System.Drawing.Size(140, 341);
            this.treeQueries.TabIndex = 1;
            this.treeQueries.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeQueries_MouseDoubleClick);
            this.treeQueries.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeQueries_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "search.png");
            this.imageList1.Images.SetKeyName(1, "seperator.png");
            this.imageList1.Images.SetKeyName(2, "tab.gif");
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(140, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 341);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 366);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 49);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(384, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 49);
            this.panel2.TabIndex = 2;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(113, 14);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(4, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // dgLayers
            // 
            this.dgLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgLayers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colField,
            this.colPrompt,
            this.colOperator});
            this.dgLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgLayers.Location = new System.Drawing.Point(3, 3);
            this.dgLayers.Name = "dgLayers";
            this.dgLayers.Size = new System.Drawing.Size(355, 216);
            this.dgLayers.TabIndex = 4;
            // 
            // colField
            // 
            this.colField.DataPropertyName = "Field";
            this.colField.HeaderText = "Field";
            this.colField.Name = "colField";
            // 
            // colPrompt
            // 
            this.colPrompt.HeaderText = "Prompt";
            this.colPrompt.Name = "colPrompt";
            this.colPrompt.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colPrompt.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colOperator
            // 
            this.colOperator.DataPropertyName = "Operator";
            this.colOperator.HeaderText = "Operator";
            this.colOperator.Items.AddRange(new object[] {
            "=",
            "<>",
            ">",
            ">=",
            "<",
            "<=",
            " like "});
            this.colOperator.Name = "colOperator";
            // 
            // dgQueries
            // 
            this.dgQueries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgQueries.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colInputPrompt,
            this.colObliging});
            this.dgQueries.Location = new System.Drawing.Point(159, 295);
            this.dgQueries.Name = "dgQueries";
            this.dgQueries.Size = new System.Drawing.Size(369, 65);
            this.dgQueries.TabIndex = 5;
            this.dgQueries.Visible = false;
            // 
            // colID
            // 
            this.colID.DataPropertyName = "ID";
            this.colID.HeaderText = "ID";
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            this.colID.Width = 30;
            // 
            // colInputPrompt
            // 
            this.colInputPrompt.DataPropertyName = "Prompt";
            this.colInputPrompt.HeaderText = "Prompt";
            this.colInputPrompt.Name = "colInputPrompt";
            this.colInputPrompt.Width = 150;
            // 
            // colObliging
            // 
            this.colObliging.DataPropertyName = "Obliging";
            this.colObliging.HeaderText = "Obliging";
            this.colObliging.Name = "colObliging";
            this.colObliging.Width = 50;
            // 
            // tabControlLayers
            // 
            this.tabControlLayers.Controls.Add(this.tabPage1);
            this.tabControlLayers.Controls.Add(this.tabPage2);
            this.tabControlLayers.Location = new System.Drawing.Point(159, 41);
            this.tabControlLayers.Name = "tabControlLayers";
            this.tabControlLayers.SelectedIndex = 0;
            this.tabControlLayers.Size = new System.Drawing.Size(369, 248);
            this.tabControlLayers.TabIndex = 6;
            this.tabControlLayers.Visible = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgLayers);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(361, 222);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Queryable Field";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgVisibleFields);
            this.tabPage2.Controls.Add(this.panel4);
            this.tabPage2.Controls.Add(this.panel3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(361, 222);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Visible Fields";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgVisibleFields
            // 
            this.dgVisibleFields.AllowUserToAddRows = false;
            this.dgVisibleFields.AllowUserToDeleteRows = false;
            this.dgVisibleFields.AllowUserToResizeRows = false;
            this.dgVisibleFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgVisibleFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldCol1,
            this.fieldCol2,
            this.fieldCol3,
            this.fieldCol4});
            this.dgVisibleFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgVisibleFields.GridColor = System.Drawing.Color.White;
            this.dgVisibleFields.Location = new System.Drawing.Point(3, 98);
            this.dgVisibleFields.Name = "dgVisibleFields";
            this.dgVisibleFields.RowHeadersVisible = false;
            this.dgVisibleFields.Size = new System.Drawing.Size(355, 82);
            this.dgVisibleFields.TabIndex = 4;
            // 
            // fieldCol1
            // 
            this.fieldCol1.Frozen = true;
            this.fieldCol1.HeaderText = " ";
            this.fieldCol1.Name = "fieldCol1";
            this.fieldCol1.Width = 20;
            // 
            // fieldCol2
            // 
            this.fieldCol2.HeaderText = "Name";
            this.fieldCol2.Name = "fieldCol2";
            this.fieldCol2.ReadOnly = true;
            this.fieldCol2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.fieldCol2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.fieldCol2.Width = 150;
            // 
            // fieldCol3
            // 
            this.fieldCol3.HeaderText = "Aliasname";
            this.fieldCol3.Name = "fieldCol3";
            this.fieldCol3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.fieldCol3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.fieldCol3.Width = 150;
            // 
            // fieldCol4
            // 
            this.fieldCol4.HeaderText = "Type";
            this.fieldCol4.Name = "fieldCol4";
            this.fieldCol4.ReadOnly = true;
            this.fieldCol4.Width = 50;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnClearAll);
            this.panel4.Controls.Add(this.btnSelectAll);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(3, 180);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(355, 39);
            this.panel4.TabIndex = 5;
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(90, 7);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(75, 23);
            this.btnClearAll.TabIndex = 1;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(8, 7);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 0;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.chkUseStandard);
            this.panel3.Controls.Add(this.groupPrimaryField);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(355, 95);
            this.panel3.TabIndex = 3;
            // 
            // chkUseStandard
            // 
            this.chkUseStandard.AutoSize = true;
            this.chkUseStandard.Location = new System.Drawing.Point(13, 11);
            this.chkUseStandard.Name = "chkUseStandard";
            this.chkUseStandard.Size = new System.Drawing.Size(229, 17);
            this.chkUseStandard.TabIndex = 1;
            this.chkUseStandard.Text = "Use layers default settings (layer properties)";
            this.chkUseStandard.UseVisualStyleBackColor = true;
            this.chkUseStandard.CheckedChanged += new System.EventHandler(this.chkUseStandard_CheckedChanged);
            // 
            // groupPrimaryField
            // 
            this.groupPrimaryField.Controls.Add(this.cmbPrimaryField);
            this.groupPrimaryField.Controls.Add(this.label6);
            this.groupPrimaryField.Location = new System.Drawing.Point(8, 36);
            this.groupPrimaryField.Name = "groupPrimaryField";
            this.groupPrimaryField.Size = new System.Drawing.Size(393, 53);
            this.groupPrimaryField.TabIndex = 0;
            this.groupPrimaryField.TabStop = false;
            this.groupPrimaryField.Text = "Primary Display Field";
            // 
            // cmbPrimaryField
            // 
            this.cmbPrimaryField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrimaryField.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbPrimaryField.FormattingEnabled = true;
            this.cmbPrimaryField.Location = new System.Drawing.Point(82, 21);
            this.cmbPrimaryField.Name = "cmbPrimaryField";
            this.cmbPrimaryField.Size = new System.Drawing.Size(292, 21);
            this.cmbPrimaryField.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Select field:";
            // 
            // FormQueryThemeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 415);
            this.Controls.Add(this.tabControlLayers);
            this.Controls.Add(this.dgQueries);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.treeQueries);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FormQueryThemeEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Query Theme Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormQueryThemeEditor_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgLayers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgQueries)).EndInit();
            this.tabControlLayers.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgVisibleFields)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupPrimaryField.ResumeLayout(false);
            this.groupPrimaryField.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnMoveUp;
        private System.Windows.Forms.ToolStripButton btnMoveDown;
        private System.Windows.Forms.ToolStripDropDownButton btnAdd;
        private System.Windows.Forms.ToolStripMenuItem btnAddQuery;
        private System.Windows.Forms.ToolStripMenuItem btnAddSeperator;
        private System.Windows.Forms.TreeView treeQueries;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem btnAddTable;
        private System.Windows.Forms.DataGridView dgLayers;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgQueries;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInputPrompt;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colObliging;
        private System.Windows.Forms.DataGridViewComboBoxColumn colField;
        private System.Windows.Forms.DataGridViewComboBoxColumn colPrompt;
        private System.Windows.Forms.DataGridViewComboBoxColumn colOperator;
        private System.Windows.Forms.TabControl tabControlLayers;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgVisibleFields;
        private System.Windows.Forms.DataGridViewCheckBoxColumn fieldCol1;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldCol2;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldCol3;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldCol4;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupPrimaryField;
        private System.Windows.Forms.ComboBox cmbPrimaryField;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkUseStandard;
    }
}
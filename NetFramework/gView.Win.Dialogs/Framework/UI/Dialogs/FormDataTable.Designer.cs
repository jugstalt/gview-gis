namespace gView.Framework.UI.Dialogs
{
    partial class FormDataTable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDataTable));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gridNavigator = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.toolStripDropDownButtonShow = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItemShowAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemShowSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemShowFeaturesInExtent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonExecute = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCancelThread = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.gridView = new System.Windows.Forms.DataGridView();
            this.ilExecute = new System.Windows.Forms.ImageList(this.components);
            this.gridSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gridNavigator)).BeginInit();
            this.gridNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSource)).BeginInit();
            this.SuspendLayout();
            // 
            // gridNavigator
            // 
            this.gridNavigator.AddNewItem = null;
            this.gridNavigator.CountItem = this.bindingNavigatorCountItem;
            this.gridNavigator.DeleteItem = null;
            this.gridNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButtonShow,
            this.toolStripDropDownButton2,
            this.toolStripSeparator2,
            this.toolStripButtonExecute,
            this.toolStripButtonCancelThread,
            this.toolStripSeparator1,
            this.bindingNavigatorMoveFirstItem,
            this.bindingNavigatorMovePreviousItem,
            this.bindingNavigatorSeparator,
            this.bindingNavigatorPositionItem,
            this.bindingNavigatorMoveNextItem,
            this.bindingNavigatorMoveLastItem,
            this.bindingNavigatorSeparator2,
            this.bindingNavigatorCountItem,
            this.bindingNavigatorSeparator1,
            this.toolStripDropDownButton1,
            this.toolStripButton1});
            resources.ApplyResources(this.gridNavigator, "gridNavigator");
            this.gridNavigator.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.gridNavigator.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.gridNavigator.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.gridNavigator.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.gridNavigator.Name = "gridNavigator";
            this.gridNavigator.PositionItem = this.bindingNavigatorPositionItem;
            this.gridNavigator.ShowItemToolTips = false;
            // 
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            resources.ApplyResources(this.bindingNavigatorCountItem, "bindingNavigatorCountItem");
            // 
            // toolStripDropDownButtonShow
            // 
            this.toolStripDropDownButtonShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButtonShow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemShowAll,
            this.toolStripMenuItemShowSelected,
            this.toolStripMenuItemShowFeaturesInExtent});
            resources.ApplyResources(this.toolStripDropDownButtonShow, "toolStripDropDownButtonShow");
            this.toolStripDropDownButtonShow.Name = "toolStripDropDownButtonShow";
            // 
            // toolStripMenuItemShowAll
            // 
            resources.ApplyResources(this.toolStripMenuItemShowAll, "toolStripMenuItemShowAll");
            this.toolStripMenuItemShowAll.Name = "toolStripMenuItemShowAll";
            this.toolStripMenuItemShowAll.Click += new System.EventHandler(this.toolStripMenuItemShowAll_Click);
            // 
            // toolStripMenuItemShowSelected
            // 
            resources.ApplyResources(this.toolStripMenuItemShowSelected, "toolStripMenuItemShowSelected");
            this.toolStripMenuItemShowSelected.Name = "toolStripMenuItemShowSelected";
            this.toolStripMenuItemShowSelected.Click += new System.EventHandler(this.toolStripMenuItemShowSelected_Click);
            // 
            // toolStripMenuItemShowFeaturesInExtent
            // 
            resources.ApplyResources(this.toolStripMenuItemShowFeaturesInExtent, "toolStripMenuItemShowFeaturesInExtent");
            this.toolStripMenuItemShowFeaturesInExtent.Name = "toolStripMenuItemShowFeaturesInExtent";
            this.toolStripMenuItemShowFeaturesInExtent.Click += new System.EventHandler(this.toolStripMenuItemShowFeaturesInExtent_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripButtonExecute
            // 
            this.toolStripButtonExecute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonExecute, "toolStripButtonExecute");
            this.toolStripButtonExecute.Name = "toolStripButtonExecute";
            this.toolStripButtonExecute.Click += new System.EventHandler(this.toolStripButtonExecute_Click);
            // 
            // toolStripButtonCancelThread
            // 
            this.toolStripButtonCancelThread.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonCancelThread, "toolStripButtonCancelThread");
            this.toolStripButtonCancelThread.Name = "toolStripButtonCancelThread";
            this.toolStripButtonCancelThread.Click += new System.EventHandler(this.toolStripButtonCancelThread_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.bindingNavigatorMoveFirstItem, "bindingNavigatorMoveFirstItem");
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            this.bindingNavigatorMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.bindingNavigatorMovePreviousItem, "bindingNavigatorMovePreviousItem");
            this.bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            // 
            // bindingNavigatorSeparator
            // 
            this.bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            resources.ApplyResources(this.bindingNavigatorSeparator, "bindingNavigatorSeparator");
            // 
            // bindingNavigatorPositionItem
            // 
            resources.ApplyResources(this.bindingNavigatorPositionItem, "bindingNavigatorPositionItem");
            this.bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            // 
            // bindingNavigatorMoveNextItem
            // 
            this.bindingNavigatorMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.bindingNavigatorMoveNextItem, "bindingNavigatorMoveNextItem");
            this.bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            // 
            // bindingNavigatorMoveLastItem
            // 
            this.bindingNavigatorMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.bindingNavigatorMoveLastItem, "bindingNavigatorMoveLastItem");
            this.bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            // 
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            resources.ApplyResources(this.bindingNavigatorSeparator2, "bindingNavigatorSeparator2");
            // 
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            resources.ApplyResources(this.bindingNavigatorSeparator1, "bindingNavigatorSeparator1");
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripDropDownButton2, "toolStripDropDownButton2");
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.DropDownOpening += new System.EventHandler(this.toolStripDropDownButton2_DropDownOpening);
            // 
            // gridView
            // 
            this.gridView.AllowUserToAddRows = false;
            this.gridView.AllowUserToDeleteRows = false;
            this.gridView.AllowUserToResizeRows = false;
            this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ActiveCaption;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridView.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.gridView, "gridView");
            this.gridView.Name = "gridView";
            this.gridView.ReadOnly = true;
            this.gridView.ShowCellToolTips = false;
            this.gridView.ShowEditingIcon = false;
            this.gridView.SelectionChanged += new System.EventHandler(this.gridView_SelectionChanged);
            this.gridView.Sorted += new System.EventHandler(this.gridView_Sorted);
            this.gridView.Paint += new System.Windows.Forms.PaintEventHandler(this.gridView_Paint);
            this.gridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridView_KeyDown);
            this.gridView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gridView_KeyUp);
            this.gridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridView_MouseDown);
            this.gridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gridView_MouseUp);
            // 
            // ilExecute
            // 
            this.ilExecute.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilExecute.ImageStream")));
            this.ilExecute.TransparentColor = System.Drawing.Color.Transparent;
            this.ilExecute.Images.SetKeyName(0, "refresh.png");
            this.ilExecute.Images.SetKeyName(1, "break.png");
            this.ilExecute.Images.SetKeyName(2, "continue.png");
            // 
            // FormDataTable
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridView);
            this.Controls.Add(this.gridNavigator);
            this.Name = "FormDataTable";
            this.Load += new System.EventHandler(this.FormDataTable_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridNavigator)).EndInit();
            this.gridNavigator.ResumeLayout(false);
            this.gridNavigator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingNavigator gridNavigator;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonCancelThread;
        private System.Windows.Forms.BindingSource gridSource;
        private System.Windows.Forms.ToolStripButton toolStripButtonExecute;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.DataGridView gridView;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonShow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowSelected;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowFeaturesInExtent;
        private System.Windows.Forms.ImageList ilExecute;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
    }
}
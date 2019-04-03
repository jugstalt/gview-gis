namespace gView.DataSources.Fdb.UI
{
    partial class FeatureClassExplorerTabPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeatureClassExplorerTabPage));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextItem_AddField = new System.Windows.Forms.ToolStripMenuItem();
            this.contextItem_ImportFields = new System.Windows.Forms.ToolStripMenuItem();
            this.contextItem_Remove = new System.Windows.Forms.ToolStripMenuItem();
            this.contextItem_Properties = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.AccessibleDescription = null;
            this.listView1.AccessibleName = null;
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.BackgroundImage = null;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.Font = null;
            this.listView1.FullRowSelect = true;
            this.listView1.Name = "listView1";
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            this.listView1.Click += new System.EventHandler(this.listView1_Click);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "field.png");
            this.imageList1.Images.SetKeyName(1, "field_geom_point.png");
            this.imageList1.Images.SetKeyName(2, "field_geom_line.png");
            this.imageList1.Images.SetKeyName(3, "field_geom_polygon.png");
            this.imageList1.Images.SetKeyName(4, "field_id.png");
            this.imageList1.Images.SetKeyName(5, "autofield.png");
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.AccessibleDescription = null;
            this.contextMenuStrip.AccessibleName = null;
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.BackgroundImage = null;
            this.contextMenuStrip.Font = null;
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextItem_AddField,
            this.contextItem_ImportFields,
            this.contextItem_Remove,
            this.contextItem_Properties});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            // 
            // contextItem_AddField
            // 
            this.contextItem_AddField.AccessibleDescription = null;
            this.contextItem_AddField.AccessibleName = null;
            resources.ApplyResources(this.contextItem_AddField, "contextItem_AddField");
            this.contextItem_AddField.BackgroundImage = null;
            this.contextItem_AddField.Name = "contextItem_AddField";
            this.contextItem_AddField.ShortcutKeyDisplayString = null;
            this.contextItem_AddField.Click += new System.EventHandler(this.contextItem_AddField_Click);
            // 
            // contextItem_ImportFields
            // 
            this.contextItem_ImportFields.AccessibleDescription = null;
            this.contextItem_ImportFields.AccessibleName = null;
            resources.ApplyResources(this.contextItem_ImportFields, "contextItem_ImportFields");
            this.contextItem_ImportFields.BackgroundImage = null;
            this.contextItem_ImportFields.Name = "contextItem_ImportFields";
            this.contextItem_ImportFields.ShortcutKeyDisplayString = null;
            this.contextItem_ImportFields.Click += new System.EventHandler(this.contextItem_ImportFields_Click);
            // 
            // contextItem_Remove
            // 
            this.contextItem_Remove.AccessibleDescription = null;
            this.contextItem_Remove.AccessibleName = null;
            resources.ApplyResources(this.contextItem_Remove, "contextItem_Remove");
            this.contextItem_Remove.BackgroundImage = null;
            this.contextItem_Remove.Name = "contextItem_Remove";
            this.contextItem_Remove.ShortcutKeyDisplayString = null;
            this.contextItem_Remove.Click += new System.EventHandler(this.contextItem_Remove_Click);
            // 
            // contextItem_Properties
            // 
            this.contextItem_Properties.AccessibleDescription = null;
            this.contextItem_Properties.AccessibleName = null;
            resources.ApplyResources(this.contextItem_Properties, "contextItem_Properties");
            this.contextItem_Properties.BackgroundImage = null;
            this.contextItem_Properties.Name = "contextItem_Properties";
            this.contextItem_Properties.ShortcutKeyDisplayString = null;
            this.contextItem_Properties.Click += new System.EventHandler(this.contextItem_Properties_Click);
            // 
            // FeatureClassExplorerTabPage
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.listView1);
            this.Font = null;
            this.Name = "FeatureClassExplorerTabPage";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem contextItem_AddField;
        private System.Windows.Forms.ToolStripMenuItem contextItem_ImportFields;
        private System.Windows.Forms.ToolStripMenuItem contextItem_Properties;
        private System.Windows.Forms.ToolStripMenuItem contextItem_Remove;
    }
}

namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormIdentify
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormIdentify));
            this.listValues = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.treeObjects = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.panelFeatures = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panelText = new System.Windows.Forms.Panel();
            this.txtIdentify = new System.Windows.Forms.TextBox();
            this.panelHTML = new System.Windows.Forms.Panel();
            this.webBrowserControl = new System.Windows.Forms.WebBrowser();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageFeatures = new System.Windows.Forms.TabPage();
            this.tabPageText = new System.Windows.Forms.TabPage();
            this.tabPageHTML = new System.Windows.Forms.TabPage();
            this.panelFeatures.SuspendLayout();
            this.panelText.SuspendLayout();
            this.panelHTML.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageFeatures.SuspendLayout();
            this.tabPageText.SuspendLayout();
            this.tabPageHTML.SuspendLayout();
            this.SuspendLayout();
            // 
            // listValues
            // 
            resources.ApplyResources(this.listValues, "listValues");
            this.listValues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listValues.FullRowSelect = true;
            this.listValues.GridLines = true;
            this.listValues.Name = "listValues";
            this.listValues.UseCompatibleStateImageBehavior = false;
            this.listValues.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Tag = "Field";
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            this.columnHeader2.Tag = "Value";
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // treeObjects
            // 
            resources.ApplyResources(this.treeObjects, "treeObjects");
            this.treeObjects.HideSelection = false;
            this.treeObjects.ImageList = this.imageList1;
            this.treeObjects.Name = "treeObjects";
            this.treeObjects.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeObjects_BeforeExpand);
            this.treeObjects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeObjects_AfterSelect);
            this.treeObjects.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeObjects_MouseClick);
            this.treeObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeObjects_MouseDown);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tab.gif");
            this.imageList1.Images.SetKeyName(1, "");
            // 
            // txtLocation
            // 
            resources.ApplyResources(this.txtLocation, "txtLocation");
            this.txtLocation.Name = "txtLocation";
            // 
            // panelFeatures
            // 
            resources.ApplyResources(this.panelFeatures, "panelFeatures");
            this.panelFeatures.Controls.Add(this.listValues);
            this.panelFeatures.Controls.Add(this.splitter1);
            this.panelFeatures.Controls.Add(this.treeObjects);
            this.panelFeatures.Name = "panelFeatures";
            // 
            // splitter1
            // 
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // panelText
            // 
            resources.ApplyResources(this.panelText, "panelText");
            this.panelText.Controls.Add(this.txtIdentify);
            this.panelText.Name = "panelText";
            // 
            // txtIdentify
            // 
            resources.ApplyResources(this.txtIdentify, "txtIdentify");
            this.txtIdentify.BackColor = System.Drawing.Color.White;
            this.txtIdentify.Name = "txtIdentify";
            this.txtIdentify.ReadOnly = true;
            // 
            // panelHTML
            // 
            resources.ApplyResources(this.panelHTML, "panelHTML");
            this.panelHTML.Controls.Add(this.webBrowserControl);
            this.panelHTML.Name = "panelHTML";
            // 
            // webBrowserControl
            // 
            resources.ApplyResources(this.webBrowserControl, "webBrowserControl");
            this.webBrowserControl.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserControl.Name = "webBrowserControl";
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPageFeatures);
            this.tabControl1.Controls.Add(this.tabPageText);
            this.tabControl1.Controls.Add(this.tabPageHTML);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPageFeatures
            // 
            resources.ApplyResources(this.tabPageFeatures, "tabPageFeatures");
            this.tabPageFeatures.Controls.Add(this.panelFeatures);
            this.tabPageFeatures.Name = "tabPageFeatures";
            this.tabPageFeatures.UseVisualStyleBackColor = true;
            // 
            // tabPageText
            // 
            resources.ApplyResources(this.tabPageText, "tabPageText");
            this.tabPageText.Controls.Add(this.panelText);
            this.tabPageText.Name = "tabPageText";
            this.tabPageText.UseVisualStyleBackColor = true;
            // 
            // tabPageHTML
            // 
            resources.ApplyResources(this.tabPageHTML, "tabPageHTML");
            this.tabPageHTML.Controls.Add(this.panelHTML);
            this.tabPageHTML.Name = "tabPageHTML";
            this.tabPageHTML.UseVisualStyleBackColor = true;
            // 
            // FormIdentify
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.txtLocation);
            this.Name = "FormIdentify";
            this.panelFeatures.ResumeLayout(false);
            this.panelText.ResumeLayout(false);
            this.panelText.PerformLayout();
            this.panelHTML.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageFeatures.ResumeLayout(false);
            this.tabPageText.ResumeLayout(false);
            this.tabPageHTML.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listValues;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.TreeView treeObjects;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panelFeatures;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelText;
        private System.Windows.Forms.TextBox txtIdentify;
        private System.Windows.Forms.Panel panelHTML;
        private System.Windows.Forms.WebBrowser webBrowserControl;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageFeatures;
        private System.Windows.Forms.TabPage tabPageText;
        private System.Windows.Forms.TabPage tabPageHTML;
    }
}
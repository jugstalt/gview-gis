namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormPerformanceMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPerformanceMonitor));
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkPerform = new System.Windows.Forms.CheckBox();
            this.lstEvents = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkPerform);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(288, 41);
            this.panel1.TabIndex = 0;
            // 
            // chkPerform
            // 
            this.chkPerform.AutoSize = true;
            this.chkPerform.Location = new System.Drawing.Point(13, 13);
            this.chkPerform.Name = "chkPerform";
            this.chkPerform.Size = new System.Drawing.Size(114, 17);
            this.chkPerform.TabIndex = 0;
            this.chkPerform.Text = "Perform Monitoring";
            this.chkPerform.UseVisualStyleBackColor = true;
            this.chkPerform.CheckedChanged += new System.EventHandler(this.chkPerform_CheckedChanged);
            // 
            // lstEvents
            // 
            this.lstEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstEvents.Location = new System.Drawing.Point(0, 41);
            this.lstEvents.Name = "lstEvents";
            this.lstEvents.Size = new System.Drawing.Size(288, 375);
            this.lstEvents.SmallImageList = this.imageList1;
            this.lstEvents.TabIndex = 1;
            this.lstEvents.UseCompatibleStateImageBehavior = false;
            this.lstEvents.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time Event";
            this.columnHeader1.Width = 127;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Timespan";
            this.columnHeader2.Width = 94;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "time.png");
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Features";
            // 
            // FormPerformanceMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 416);
            this.Controls.Add(this.lstEvents);
            this.Controls.Add(this.panel1);
            this.Name = "FormPerformanceMonitor";
            this.Text = "FormPerformanceMonitor";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView lstEvents;
        private System.Windows.Forms.CheckBox chkPerform;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}
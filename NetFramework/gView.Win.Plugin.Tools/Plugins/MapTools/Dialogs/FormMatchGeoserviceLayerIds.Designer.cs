namespace gView.Win.Plugin.Tools.Plugins.MapTools.Dialogs
{
    partial class FormMatchGeoserviceLayerIds
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMapServerUrl = new System.Windows.Forms.TextBox();
            this.btnMatch = new System.Windows.Forms.Button();
            this.lstLayers = new System.Windows.Forms.ListView();
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLayername = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCurrentLayerId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colGeoServiceId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnApply = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(847, 55);
            this.panel1.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(8, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(743, 29);
            this.label6.TabIndex = 1;
            this.label6.Text = "Match Layer Ids from GeoServices MapService for identical Layers (Layer with equa" +
    "l TOC Name, TOC Group, Geometry, ...)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "GeoService MapServer Url: (https://server.com/..../rest/services/..../MapServer)";
            // 
            // txtMapServerUrl
            // 
            this.txtMapServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMapServerUrl.Location = new System.Drawing.Point(14, 79);
            this.txtMapServerUrl.Name = "txtMapServerUrl";
            this.txtMapServerUrl.Size = new System.Drawing.Size(821, 20);
            this.txtMapServerUrl.TabIndex = 15;
            // 
            // btnMatch
            // 
            this.btnMatch.Location = new System.Drawing.Point(14, 106);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnMatch.Size = new System.Drawing.Size(119, 23);
            this.btnMatch.TabIndex = 16;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = true;
            this.btnMatch.Click += new System.EventHandler(this.btnMatch_Click);
            // 
            // lstLayers
            // 
            this.lstLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colStatus,
            this.colLayername,
            this.colCurrentLayerId,
            this.colGeoServiceId});
            this.lstLayers.GridLines = true;
            this.lstLayers.HideSelection = false;
            this.lstLayers.Location = new System.Drawing.Point(13, 142);
            this.lstLayers.Name = "lstLayers";
            this.lstLayers.Size = new System.Drawing.Size(821, 695);
            this.lstLayers.TabIndex = 17;
            this.lstLayers.UseCompatibleStateImageBehavior = false;
            this.lstLayers.View = System.Windows.Forms.View.Details;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 73;
            // 
            // colLayername
            // 
            this.colLayername.Text = "Groupname/Layername";
            this.colLayername.Width = 411;
            // 
            // colCurrentLayerId
            // 
            this.colCurrentLayerId.Text = "Current Id";
            this.colCurrentLayerId.Width = 157;
            // 
            // colGeoServiceId
            // 
            this.colGeoServiceId.Text = "GeoService Id";
            this.colGeoServiceId.Width = 163;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(680, 843);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(154, 41);
            this.btnApply.TabIndex = 18;
            this.btnApply.Text = "Apply Layer Ids";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // FormMatchGeoserviceLayerIds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 896);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lstLayers);
            this.Controls.Add(this.btnMatch);
            this.Controls.Add(this.txtMapServerUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Name = "FormMatchGeoserviceLayerIds";
            this.Text = "Match Geoservice Layer Ids";
            this.Load += new System.EventHandler(this.FormMatchGeoserviceLayerIds_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMapServerUrl;
        private System.Windows.Forms.Button btnMatch;
        private System.Windows.Forms.ListView lstLayers;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLayername;
        private System.Windows.Forms.ColumnHeader colCurrentLayerId;
        private System.Windows.Forms.ColumnHeader colGeoServiceId;
        private System.Windows.Forms.Button btnApply;
    }
}
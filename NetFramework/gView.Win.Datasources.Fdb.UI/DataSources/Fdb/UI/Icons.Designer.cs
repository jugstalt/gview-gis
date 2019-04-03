namespace gView.DataSources.Fdb.UI
{
    partial class Icons
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Icons));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.White;
            this.imageList1.Images.SetKeyName(0, "SqlFDB_Connections_16_16.png");
            this.imageList1.Images.SetKeyName(1, "SqlFDB_newConnection_16_16.png");
            this.imageList1.Images.SetKeyName(2, "sqlFDB_16_16.png");
            this.imageList1.Images.SetKeyName(3, "Dataset.png");
            this.imageList1.Images.SetKeyName(4, "polylinelayer.png");
            this.imageList1.Images.SetKeyName(5, "pointlayer.png");
            this.imageList1.Images.SetKeyName(6, "polygonlayer.png");
            this.imageList1.Images.SetKeyName(7, "rasterlayer.png");
            this.imageList1.Images.SetKeyName(8, "db_connect2.png");
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "");
            this.imageList2.Images.SetKeyName(1, "AccessFDB.png");
            this.imageList2.Images.SetKeyName(2, "Dataset.png");
            this.imageList2.Images.SetKeyName(3, "polylinelayer.png");
            this.imageList2.Images.SetKeyName(4, "pointlayer.png");
            this.imageList2.Images.SetKeyName(5, "polygonlayer.png");
            this.imageList2.Images.SetKeyName(6, "rasterlayer.png");
            // 
            // Icons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 131);
            this.Name = "Icons";
            this.Text = "Icons";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.ImageList imageList2;
    }
}
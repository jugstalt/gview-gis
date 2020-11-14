
namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Dialogs
{
    partial class FormVectorTileCacheConnection
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
            this.groupBoxCapabilites = new System.Windows.Forms.GroupBox();
            this.txtCapabilites = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBoxName = new System.Windows.Forms.GroupBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.groupBoxCapabilites.SuspendLayout();
            this.groupBoxName.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCapabilites
            // 
            this.groupBoxCapabilites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCapabilites.Controls.Add(this.btnConnect);
            this.groupBoxCapabilites.Controls.Add(this.txtCapabilites);
            this.groupBoxCapabilites.Controls.Add(this.label1);
            this.groupBoxCapabilites.Location = new System.Drawing.Point(12, 25);
            this.groupBoxCapabilites.Name = "groupBoxCapabilites";
            this.groupBoxCapabilites.Size = new System.Drawing.Size(651, 153);
            this.groupBoxCapabilites.TabIndex = 0;
            this.groupBoxCapabilites.TabStop = false;
            this.groupBoxCapabilites.Text = "Capabilities";
            // 
            // txtCapabilites
            // 
            this.txtCapabilites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCapabilites.Location = new System.Drawing.Point(24, 63);
            this.txtCapabilites.Name = "txtCapabilites";
            this.txtCapabilites.Size = new System.Drawing.Size(605, 26);
            this.txtCapabilites.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(364, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Url or path to Vector-Tile-Cache capabilities JSON:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(462, 425);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(201, 65);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBoxName
            // 
            this.groupBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxName.Controls.Add(this.txtName);
            this.groupBoxName.Controls.Add(this.label2);
            this.groupBoxName.Location = new System.Drawing.Point(12, 198);
            this.groupBoxName.Name = "groupBoxName";
            this.groupBoxName.Size = new System.Drawing.Size(651, 175);
            this.groupBoxName.TabIndex = 2;
            this.groupBoxName.TabStop = false;
            this.groupBoxName.Text = "Properties";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(24, 72);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(605, 26);
            this.txtName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Name:";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(473, 104);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(156, 37);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // FormVectorTileCacheConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 502);
            this.Controls.Add(this.groupBoxName);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBoxCapabilites);
            this.Name = "FormVectorTileCacheConnection";
            this.Text = "Vector Tile Cache Connection";
            this.groupBoxCapabilites.ResumeLayout(false);
            this.groupBoxCapabilites.PerformLayout();
            this.groupBoxName.ResumeLayout(false);
            this.groupBoxName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxCapabilites;
        private System.Windows.Forms.TextBox txtCapabilites;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox groupBoxName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
    }
}
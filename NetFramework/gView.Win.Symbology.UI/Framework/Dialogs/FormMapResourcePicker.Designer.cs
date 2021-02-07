
namespace gView.Framework.UI.Symbology.Dialogs
{
    partial class FormMapResourcePicker
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.lstResources = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnAbort);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 732);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(666, 60);
            this.panel1.TabIndex = 11;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(504, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(162, 60);
            this.panel2.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Enabled = false;
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(32, 12);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 35);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnAbort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnAbort.Location = new System.Drawing.Point(18, 12);
            this.btnAbort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAbort.Name = "btnCancel";
            this.btnAbort.Size = new System.Drawing.Size(218, 35);
            this.btnAbort.TabIndex = 3;
            this.btnAbort.Text = "Cancel";
            this.btnAbort.UseVisualStyleBackColor = true;
            // 
            // lstResources
            // 
            this.lstResources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResources.FormattingEnabled = true;
            this.lstResources.ItemHeight = 20;
            this.lstResources.Location = new System.Drawing.Point(12, 8);
            this.lstResources.Name = "lstResources";
            this.lstResources.Size = new System.Drawing.Size(642, 704);
            this.lstResources.Sorted = true;
            this.lstResources.TabIndex = 12;
            this.lstResources.SelectedIndexChanged += new System.EventHandler(this.lstResources_SelectedIndexChanged);
            // 
            // FormMapResourcePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 792);
            this.Controls.Add(this.lstResources);
            this.Controls.Add(this.panel1);
            this.Name = "FormMapResourcePicker";
            this.Text = "Map Resource Picker";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.ListBox lstResources;
    }
}
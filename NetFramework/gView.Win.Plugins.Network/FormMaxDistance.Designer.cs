namespace gView.Plugins.Network
{
    partial class FormMaxDistance
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnUseMaxDistance = new System.Windows.Forms.RadioButton();
            this.btnInfinite = new System.Windows.Forms.RadioButton();
            this.numMaxDistance = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDistance)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 88);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(394, 88);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnUseMaxDistance
            // 
            this.btnUseMaxDistance.AutoSize = true;
            this.btnUseMaxDistance.Checked = true;
            this.btnUseMaxDistance.Location = new System.Drawing.Point(24, 13);
            this.btnUseMaxDistance.Name = "btnUseMaxDistance";
            this.btnUseMaxDistance.Size = new System.Drawing.Size(243, 17);
            this.btnUseMaxDistance.TabIndex = 2;
            this.btnUseMaxDistance.TabStop = true;
            this.btnUseMaxDistance.Text = "Perform to Maximum Distance from Start Point:";
            this.btnUseMaxDistance.UseVisualStyleBackColor = true;
            // 
            // btnInfinite
            // 
            this.btnInfinite.AutoSize = true;
            this.btnInfinite.Location = new System.Drawing.Point(24, 45);
            this.btnInfinite.Name = "btnInfinite";
            this.btnInfinite.Size = new System.Drawing.Size(56, 17);
            this.btnInfinite.TabIndex = 3;
            this.btnInfinite.Text = "Infinite";
            this.btnInfinite.UseVisualStyleBackColor = true;
            this.btnInfinite.CheckedChanged += new System.EventHandler(this.btnInfinite_CheckedChanged);
            // 
            // numMaxDistance
            // 
            this.numMaxDistance.DecimalPlaces = 2;
            this.numMaxDistance.Location = new System.Drawing.Point(273, 13);
            this.numMaxDistance.Maximum = new decimal(new int[] {
            276447232,
            23283,
            0,
            0});
            this.numMaxDistance.Name = "numMaxDistance";
            this.numMaxDistance.Size = new System.Drawing.Size(182, 20);
            this.numMaxDistance.TabIndex = 4;
            this.numMaxDistance.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // FormMaxDistance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 123);
            this.Controls.Add(this.numMaxDistance);
            this.Controls.Add(this.btnInfinite);
            this.Controls.Add(this.btnUseMaxDistance);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMaxDistance";
            this.Text = "Maximum Distance";
            ((System.ComponentModel.ISupportInitialize)(this.numMaxDistance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RadioButton btnUseMaxDistance;
        private System.Windows.Forms.RadioButton btnInfinite;
        private System.Windows.Forms.NumericUpDown numMaxDistance;
    }
}
namespace gView.Plugins.Editor.Dialogs
{
    partial class FormDirectionDistance
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
            this.numDirection = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numDistance = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numDirection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDistance)).BeginInit();
            this.SuspendLayout();
            // 
            // numDirection
            // 
            this.numDirection.DecimalPlaces = 5;
            this.numDirection.Location = new System.Drawing.Point(93, 12);
            this.numDirection.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numDirection.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numDirection.Name = "numDirection";
            this.numDirection.Size = new System.Drawing.Size(124, 20);
            this.numDirection.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Direction:";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(142, 103);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(12, 103);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Distance:";
            // 
            // numDistance
            // 
            this.numDistance.DecimalPlaces = 3;
            this.numDistance.Location = new System.Drawing.Point(93, 47);
            this.numDistance.Maximum = new decimal(new int[] {
            -1486618625,
            232830643,
            0,
            0});
            this.numDistance.Minimum = new decimal(new int[] {
            -1486618625,
            232830643,
            0,
            -2147483648});
            this.numDistance.Name = "numDistance";
            this.numDistance.Size = new System.Drawing.Size(124, 20);
            this.numDistance.TabIndex = 4;
            // 
            // FormDirectionDistance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(225, 138);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numDistance);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numDirection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormDirectionDistance";
            this.Text = "Direction, Distance";
            ((System.ComponentModel.ISupportInitialize)(this.numDirection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDistance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numDirection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numDistance;
    }
}
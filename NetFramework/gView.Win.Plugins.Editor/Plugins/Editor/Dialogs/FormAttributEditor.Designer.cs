using gView.Plugins.Editor.Controls;
namespace gView.Plugins.Editor.Dialogs
{
    partial class FormAttributeEditor
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
            this.attributeControl = new AttributeControl();
            this.SuspendLayout();
            // 
            // attributeControl
            // 
            this.attributeControl.Location = new System.Drawing.Point(58, 66);
            this.attributeControl.Module = null;
            this.attributeControl.Name = "attributeControl";
            this.attributeControl.Size = new System.Drawing.Size(155, 241);
            this.attributeControl.TabIndex = 0;
            // 
            // FormAttributeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 349);
            this.Controls.Add(this.attributeControl);
            this.Name = "FormAttributeEditor";
            this.Text = "FormAttributeEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private AttributeControl attributeControl;

    }
}
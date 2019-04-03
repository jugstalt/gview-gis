namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormOverviewMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOverviewMap));
            this.mapView1 = new gView.Framework.UI.Controls.MapView();
            this.SuspendLayout();
            // 
            // mapView1
            // 
            this.mapView1.AccessibleDescription = null;
            this.mapView1.AccessibleName = null;
            this.mapView1.AllowMouseWheel = false;
            resources.ApplyResources(this.mapView1, "mapView1");
            this.mapView1.BackgroundImage = null;
            this.mapView1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.mapView1.Font = null;
            this.mapView1.Limit = null;
            this.mapView1.Map = null;
            this.mapView1.MouseNavigationType = gView.Framework.UI.Controls.NavigationType.Standard;
            this.mapView1.Name = "mapView1";
            this.mapView1.resizeMode = gView.Framework.UI.Controls.MapView.ResizeMode.SameExtent;
            this.mapView1.Tool = null;
            // 
            // FormOverviewMap
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.mapView1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = null;
            this.Name = "FormOverviewMap";
            this.MouseEnter += new System.EventHandler(this.FormOverviewMap_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FormOverviewMap_MouseLeave);
            this.Load += new System.EventHandler(this.FormOverviewMap_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.MapView mapView1;
    }
}
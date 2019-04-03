namespace gView.DataSources.Fdb.UI.MSSql
{
    partial class FormRebuildSpatialIndexDef
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRebuildSpatialIndexDef));
            gView.Framework.Geometry.Envelope envelope1 = new gView.Framework.Geometry.Envelope();
            gView.Framework.Geometry.Point point1 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point2 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point3 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point4 = new gView.Framework.Geometry.Point();
            gView.Framework.Geometry.Point point5 = new gView.Framework.Geometry.Point();
            this.spatialIndexControl1 = new gView.Framework.UI.Controls.SpatialIndexControl();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // spatialIndexControl1
            // 
            this.spatialIndexControl1.AccessibleDescription = null;
            this.spatialIndexControl1.AccessibleName = null;
            resources.ApplyResources(this.spatialIndexControl1, "spatialIndexControl1");
            this.spatialIndexControl1.BackgroundImage = null;
            point1.M = 0;
            point1.X = 0;
            point1.Y = 0;
            point1.Z = 0;
            envelope1.Center = point1;
            point2.M = 0;
            point2.X = 0;
            point2.Y = 0;
            point2.Z = 0;
            envelope1.LowerLeft = point2;
            point3.M = 0;
            point3.X = 0;
            point3.Y = 0;
            point3.Z = 0;
            envelope1.LowerRight = point3;
            envelope1.maxx = 0;
            envelope1.maxy = 0;
            envelope1.minx = 0;
            envelope1.miny = 0;
            point4.M = 0;
            point4.X = 0;
            point4.Y = 0;
            point4.Z = 0;
            envelope1.UpperLeft = point4;
            point5.M = 0;
            point5.X = 0;
            point5.Y = 0;
            point5.Z = 0;
            envelope1.UpperRight = point5;
            this.spatialIndexControl1.Extent = envelope1;
            this.spatialIndexControl1.Font = null;
            this.spatialIndexControl1.Levels = 0;
            this.spatialIndexControl1.Name = "spatialIndexControl1";
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.AccessibleDescription = null;
            this.btnClose.AccessibleName = null;
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.BackgroundImage = null;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = null;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FormRebuildSpatialIndexDef
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.spatialIndexControl1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormRebuildSpatialIndexDef";
            this.ResumeLayout(false);

        }

        #endregion

        private gView.Framework.UI.Controls.SpatialIndexControl spatialIndexControl1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnClose;
    }
}
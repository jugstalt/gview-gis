using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI
{
	/// <summary>
	/// Zusammenfassung für FormColorTransparency.
	/// </summary>
	public class FormColorTransparency : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private Color _color;

		public FormColorTransparency(Color color)
		{
			InitializeComponent();

			_color=color;
			trackBar1.Value=_color.A;
		}

		public Color Color 
		{
			get { return _color; }
		}
		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormColorTransparency));
            this.panelPreview = new System.Windows.Forms.Panel();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPreview
            // 
            this.panelPreview.AccessibleDescription = null;
            this.panelPreview.AccessibleName = null;
            resources.ApplyResources(this.panelPreview, "panelPreview");
            this.panelPreview.BackgroundImage = null;
            this.panelPreview.Font = null;
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // trackBar1
            // 
            this.trackBar1.AccessibleDescription = null;
            this.trackBar1.AccessibleName = null;
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.BackgroundImage = null;
            this.trackBar1.Font = null;
            this.trackBar1.Maximum = 255;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.TickFrequency = 26;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // btnOk
            // 
            this.btnOk.AccessibleDescription = null;
            this.btnOk.AccessibleName = null;
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.BackgroundImage = null;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = null;
            this.btnOk.Name = "btnOk";
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            // 
            // FormColorTransparency
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.panelPreview);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormColorTransparency";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void panelPreview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Rectangle rect=new Rectangle(20,20,panelPreview.Width-40,panelPreview.Height-40);

			using(SolidBrush brush=new SolidBrush(Color.White)) 
			{
				e.Graphics.FillRectangle(brush,0,0,panelPreview.Width,panelPreview.Height);
			}
			using(Pen pen=new Pen(Color.Black,3)) 
			{
				e.Graphics.DrawLine(pen,0,0,panelPreview.Width,panelPreview.Height);
				e.Graphics.DrawLine(pen,panelPreview.Width,0,0,panelPreview.Height);
			}
			using(SolidBrush brush=new SolidBrush(_color)) 
			{
				e.Graphics.FillRectangle(brush,rect);
			}
			using(Pen pen=new Pen(Color.Black)) 
			{
				e.Graphics.DrawRectangle(pen,rect);
			}
		}

		private void trackBar1_Scroll(object sender, System.EventArgs e)
		{
			_color=Color.FromArgb(trackBar1.Value,_color.R,_color.G,_color.B);
			panelPreview.Refresh();
		}
	}
}

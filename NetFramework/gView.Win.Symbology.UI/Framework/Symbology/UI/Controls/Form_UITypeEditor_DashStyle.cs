using gView.Framework.Sys.UI.Extensions;
using gView.GraphicsEngine;
using System;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    /// <summary>
    /// Zusammenfassung für Form_UITypeEditor_DashStyle.
    /// </summary>
    internal class Form_UITypeEditor_DashStyle : System.Windows.Forms.Form
	{
		private enum EditStyle { dash,hatch }

		private System.Windows.Forms.ListBox listBox1;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private IWindowsFormsEditorService _wfes;
		private	object _style;
		private EditStyle editStyle=EditStyle.dash;

		public Form_UITypeEditor_DashStyle(IWindowsFormsEditorService wfes, LineDashStyle style)
		{
			editStyle=EditStyle.dash;
			InitializeComponent();

			foreach(LineDashStyle s in Enum.GetValues(typeof(LineDashStyle))) 
			{
				listBox1.Items.Add(s);
			}
			_wfes=wfes;
			_style=style;
		}

		public Form_UITypeEditor_DashStyle(IWindowsFormsEditorService wfes,HatchStyle style) 
		{
			editStyle=EditStyle.hatch;
			InitializeComponent();

			foreach(HatchStyle h in HatchStyle.GetValues(typeof(HatchStyle))) 
			{
				listBox1.Items.Add(h);
			}
			_wfes=wfes;
			_style=style;
		}

		public LineDashStyle DashStyle 
		{
			get 
			{
				return (LineDashStyle)_style; 
			}
		}
		public HatchStyle HatchStyle 
		{
			get 
			{ 
				return (HatchStyle)_style; 
			}
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
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.listBox1.IntegralHeight = false;
			this.listBox1.ItemHeight = 30;
			this.listBox1.Location = new System.Drawing.Point(0, 0);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(112, 152);
			this.listBox1.TabIndex = 0;
			this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
			this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// Form_UITypeEditor_DashStyle
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(112, 152);
			this.Controls.Add(this.listBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Form_UITypeEditor_DashStyle";
			this.Text = "Form_UITypeEditor_DashStyle";
			this.ResumeLayout(false);

		}
		#endregion

		private void listBox1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			try
			{
				if (editStyle == EditStyle.dash)
				{
					LineDashStyle style = (LineDashStyle)listBox1.Items[e.Index];

					using (var bitmap = Current.Engine.CreateBitmap(e.Bounds.Width, e.Bounds.Height))
					using (var canvas = bitmap.CreateCanvas())
					using (var pen = Current.Engine.CreatePen(ArgbColor.Black, 2))
					{
						if (style == (LineDashStyle)_style)
						{
							using (var brush = Current.Engine.CreateSolidBrush(ArgbColor.Yellow))
							{
								canvas.FillRectangle(brush, new CanvasRectangle(0, 0, e.Bounds.Width, e.Bounds.Height));
							}
						}
						pen.DashStyle = style;
						canvas.DrawLine(pen, 5, bitmap.Height / 2, bitmap.Width - 10, bitmap.Height / 2);

						e.Graphics.DrawImage(bitmap.ToGdiBitmap(), e.Bounds.Left, e.Bounds.Top);
					}
				}
				else if (editStyle == EditStyle.hatch)
				{
					HatchStyle style = (HatchStyle)listBox1.Items[e.Index];

					using (var bitmap = Current.Engine.CreateBitmap(e.Bounds.Width, e.Bounds.Height))
					using (var canvas = bitmap.CreateCanvas())
					using (var hbrush = Current.Engine.CreateHatchBrush(style, ArgbColor.Black, ArgbColor.Transparent))
					{
						if (style == (HatchStyle)_style)
						{
							using (var brush = Current.Engine.CreateSolidBrush(ArgbColor.Yellow))
							{
								canvas.FillRectangle(brush, new CanvasRectangle(0, 0, e.Bounds.Width, e.Bounds.Height));
							}
						}
						CanvasRectangleF rect = new CanvasRectangleF(5, 5, bitmap.Width - 10, bitmap.Height - 10);
						using (var pen = Current.Engine.CreatePen(ArgbColor.Black, 1))
						{
							canvas.DrawRectangle(pen, rect);
						}
						canvas.FillRectangle(hbrush, rect);

						e.Graphics.DrawImage(bitmap.ToGdiBitmap(), e.Bounds.Left, e.Bounds.Top);
					}
				}
			}
			catch
			{
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(listBox1.SelectedItem==null)
            {
                return;
            }

            if (_wfes!=null) 
			{
				_style=(LineDashStyle)listBox1.SelectedItem;
				_wfes.CloseDropDown();
			}
		}
	}
}

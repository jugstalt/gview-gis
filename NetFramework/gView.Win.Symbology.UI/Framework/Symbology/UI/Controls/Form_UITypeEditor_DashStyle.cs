using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
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

		public Form_UITypeEditor_DashStyle(IWindowsFormsEditorService wfes,DashStyle style)
		{
			editStyle=EditStyle.dash;
			InitializeComponent();

			foreach(DashStyle s in DashStyle.GetValues(typeof(DashStyle))) 
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

		public DashStyle DashStyle 
		{
			get 
			{
				return (DashStyle)_style; 
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
				if(editStyle==EditStyle.dash) 
				{
					DashStyle style=(DashStyle)listBox1.Items[e.Index];
					using(Pen pen=new Pen(Color.Black,2)) 
					{
						if(style==(DashStyle)_style) 
						{
							using(SolidBrush brush=new SolidBrush(Color.Aqua)) 
							{
								e.Graphics.FillRectangle(brush,e.Bounds);
							}
						}
						pen.DashStyle=style;
						e.Graphics.DrawLine(pen,5,e.Bounds.Top+listBox1.ItemHeight/2,e.Bounds.Width-10,e.Bounds.Top+listBox1.ItemHeight/2);
					}
				} 
				else if(editStyle==EditStyle.hatch) 
				{
					HatchStyle style=(HatchStyle)listBox1.Items[e.Index];
					using(HatchBrush hbrush=new HatchBrush(style,Color.Black,Color.Transparent)) 
					{
						if(style==(HatchStyle)_style) 
						{
							using(SolidBrush brush=new SolidBrush(Color.Aqua)) 
							{
								e.Graphics.FillRectangle(brush,e.Bounds);
							}
						}
						Rectangle rect=new Rectangle(e.Bounds.X+5,e.Bounds.Y+5,e.Bounds.Width-10,e.Bounds.Height-10);
						using(Pen pen=new Pen(Color.Black)) 
						{
							e.Graphics.DrawRectangle(pen,rect);
						}
						e.Graphics.FillRectangle(hbrush,rect);
					}
				}
			} 
			catch 
			{
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(listBox1.SelectedItem==null) return;

			if(_wfes!=null) 
			{
				_style=(DashStyle)listBox1.SelectedItem;
				_wfes.CloseDropDown();
			}
		}
	}
}

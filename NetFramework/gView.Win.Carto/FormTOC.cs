using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.UI;
using System.Threading.Tasks;

namespace gView.Win.Carto
{
	internal class FormTOC : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;

		private gView.Framework.UI.Controls.TOCControl _toc;
		private gView.Framework.UI.Controls.TOCControl _source;
		private System.Windows.Forms.ImageList imageList1;

        private FormTOC()
        {
            InitializeComponent();
        }

		async static public Task<FormTOC> CreateAsync(gView.Framework.UI.MapDocument doc)
		{
            var toc = new FormTOC();

            toc._toc = new gView.Framework.UI.Controls.TOCControl();
            await toc._toc.SetMapDocumentAsync(doc);
            toc._toc.TocViewMode = gView.Framework.UI.Controls.TOCViewMode.Groups;
            toc._source = new gView.Framework.UI.Controls.TOCControl();
            await toc._source.SetMapDocumentAsync(doc);
            toc._source.TocViewMode = gView.Framework.UI.Controls.TOCViewMode.Datasets;

            return toc;
		}

        public gView.Framework.UI.Controls.TOCControl TOC
        {
            get { return _toc; }
        }
        public gView.Framework.UI.Controls.TOCControl Source
        {
            get { return _source; }
        }

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

        public void AddContainer(Container container)
        {

        }
		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTOC));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            // 
            // FormTOC
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(249, 431);
            this.Name = "FormTOC";
            this.Text = "FormTOC";
            this.ResumeLayout(false);

		}
		#endregion
	}
}

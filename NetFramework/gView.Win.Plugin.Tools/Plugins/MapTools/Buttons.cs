using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
	/// <summary>
	/// Zusammenfassung für Buttons.
	/// </summary>
	internal class Buttons : System.Windows.Forms.Form
	{
		public System.Windows.Forms.PictureBox picZoomin;
		public System.Windows.Forms.PictureBox picZoomout;
		public System.Windows.Forms.PictureBox picPan;
		public System.Windows.Forms.PictureBox picSelect;
		public System.Windows.Forms.PictureBox picAddDataset;
		public System.Windows.Forms.PictureBox picZoom2Select;
		public System.Windows.Forms.PictureBox picClearSelect;
		public System.Windows.Forms.PictureBox picSave;
        public System.Windows.Forms.PictureBox picLoad;
        public ImageList imageListPrint;
        public ImageList imageList2;
        public ImageList imageList1;
        private ContextMenuStrip contextMenuStripMeasure;
        private ToolStripMenuItem cmnStopMeasuring;
        private ToolStripMenuItem cmnClosePolygonAndStop;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem cmnShowArea;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem cmnDistanceUnit;
        private ToolStripMenuItem cmnAreaUnit;
        public ImageList ILGraphics;
        private IContainer components;

		public Buttons()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Buttons));
            this.imageListPrint = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripMeasure = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmnStopMeasuring = new System.Windows.Forms.ToolStripMenuItem();
            this.cmnClosePolygonAndStop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmnShowArea = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmnDistanceUnit = new System.Windows.Forms.ToolStripMenuItem();
            this.cmnAreaUnit = new System.Windows.Forms.ToolStripMenuItem();
            this.ILGraphics = new System.Windows.Forms.ImageList(this.components);
            this.picLoad = new System.Windows.Forms.PictureBox();
            this.picSave = new System.Windows.Forms.PictureBox();
            this.picClearSelect = new System.Windows.Forms.PictureBox();
            this.picZoom2Select = new System.Windows.Forms.PictureBox();
            this.picAddDataset = new System.Windows.Forms.PictureBox();
            this.picSelect = new System.Windows.Forms.PictureBox();
            this.picPan = new System.Windows.Forms.PictureBox();
            this.picZoomout = new System.Windows.Forms.PictureBox();
            this.picZoomin = new System.Windows.Forms.PictureBox();
            this.contextMenuStripMeasure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picClearSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoom2Select)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddDataset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPan)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoomout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoomin)).BeginInit();
            this.SuspendLayout();
            // 
            // imageListPrint
            // 
            this.imageListPrint.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPrint.ImageStream")));
            this.imageListPrint.TransparentColor = System.Drawing.Color.Black;
            this.imageListPrint.Images.SetKeyName(0, "pagesetup.png");
            this.imageListPrint.Images.SetKeyName(1, "printpreview.png");
            this.imageListPrint.Images.SetKeyName(2, "print_24.gif");
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "copy_16.png");
            this.imageList2.Images.SetKeyName(1, "documents_16.png");
            this.imageList2.Images.SetKeyName(2, "layer.png");
            this.imageList2.Images.SetKeyName(3, "layergroup.png");
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder-open_16.png");
            this.imageList1.Images.SetKeyName(1, "save_16.png");
            this.imageList1.Images.SetKeyName(2, "add_16.png");
            this.imageList1.Images.SetKeyName(3, "documents_16.png");
            this.imageList1.Images.SetKeyName(4, "zoom_in.png");
            this.imageList1.Images.SetKeyName(5, "zoom_out.png");
            this.imageList1.Images.SetKeyName(6, "pan.png");
            this.imageList1.Images.SetKeyName(7, "selection.png");
            this.imageList1.Images.SetKeyName(8, "zoom2selection.png");
            this.imageList1.Images.SetKeyName(9, "clearselection.png");
            this.imageList1.Images.SetKeyName(10, "identify.png");
            this.imageList1.Images.SetKeyName(11, "documents_16.png");
            this.imageList1.Images.SetKeyName(12, "arrow-back_16.png");
            this.imageList1.Images.SetKeyName(13, "arrow-forward_16.png");
            this.imageList1.Images.SetKeyName(14, "delete_16.png");
            this.imageList1.Images.SetKeyName(15, "zoom_out_fix.png");
            this.imageList1.Images.SetKeyName(16, "zoom_in_fix.png");
            this.imageList1.Images.SetKeyName(17, "print_16.png");
            this.imageList1.Images.SetKeyName(18, "zoom_dyn.png");
            this.imageList1.Images.SetKeyName(19, "measure.png");
            this.imageList1.Images.SetKeyName(20, "search.png");
            this.imageList1.Images.SetKeyName(21, "toc.png");
            this.imageList1.Images.SetKeyName(22, "copy_16.gif");
            this.imageList1.Images.SetKeyName(23, "documents_16.gif");
            this.imageList1.Images.SetKeyName(24, "layer.png");
            this.imageList1.Images.SetKeyName(25, "layergroup.png");
            this.imageList1.Images.SetKeyName(26, "pointer.png");
            this.imageList1.Images.SetKeyName(27, "editVertex.png");
            // 
            // contextMenuStripMeasure
            // 
            this.contextMenuStripMeasure.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmnStopMeasuring,
            this.cmnClosePolygonAndStop,
            this.toolStripSeparator1,
            this.cmnShowArea,
            this.toolStripSeparator2,
            this.cmnDistanceUnit,
            this.cmnAreaUnit});
            this.contextMenuStripMeasure.Name = "contextMenuStripMeasure";
            this.contextMenuStripMeasure.Size = new System.Drawing.Size(248, 126);
            // 
            // cmnStopMeasuring
            // 
            this.cmnStopMeasuring.Name = "cmnStopMeasuring";
            this.cmnStopMeasuring.Size = new System.Drawing.Size(247, 22);
            this.cmnStopMeasuring.Text = "Stop Measuring";
            // 
            // cmnClosePolygonAndStop
            // 
            this.cmnClosePolygonAndStop.Name = "cmnClosePolygonAndStop";
            this.cmnClosePolygonAndStop.Size = new System.Drawing.Size(247, 22);
            this.cmnClosePolygonAndStop.Text = "Close Polygonand Stop Measuring";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnShowArea
            // 
            this.cmnShowArea.Name = "cmnShowArea";
            this.cmnShowArea.Size = new System.Drawing.Size(247, 22);
            this.cmnShowArea.Text = "Show Area";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnDistanceUnit
            // 
            this.cmnDistanceUnit.Name = "cmnDistanceUnit";
            this.cmnDistanceUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnDistanceUnit.Text = "Distance Unit";
            // 
            // cmnAreaUnit
            // 
            this.cmnAreaUnit.Name = "cmnAreaUnit";
            this.cmnAreaUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnAreaUnit.Text = "Area Unit";
            // 
            // ILGraphics
            // 
            this.ILGraphics.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ILGraphics.ImageStream")));
            this.ILGraphics.TransparentColor = System.Drawing.Color.Transparent;
            this.ILGraphics.Images.SetKeyName(0, "rectangle.png");
            this.ILGraphics.Images.SetKeyName(1, "ellipse.png");
            this.ILGraphics.Images.SetKeyName(2, "arrow.png");
            this.ILGraphics.Images.SetKeyName(3, "line.png");
            this.ILGraphics.Images.SetKeyName(4, "text.png");
            this.ILGraphics.Images.SetKeyName(5, "freehand.png");
            this.ILGraphics.Images.SetKeyName(6, "polyline.png");
            this.ILGraphics.Images.SetKeyName(7, "polygon.png");
            this.ILGraphics.Images.SetKeyName(8, "point.png");
            // 
            // picLoad
            // 
            this.picLoad.Image = ((System.Drawing.Image)(resources.GetObject("picLoad.Image")));
            this.picLoad.Location = new System.Drawing.Point(312, 8);
            this.picLoad.Name = "picLoad";
            this.picLoad.Size = new System.Drawing.Size(24, 24);
            this.picLoad.TabIndex = 8;
            this.picLoad.TabStop = false;
            // 
            // picSave
            // 
            this.picSave.Image = ((System.Drawing.Image)(resources.GetObject("picSave.Image")));
            this.picSave.Location = new System.Drawing.Point(280, 8);
            this.picSave.Name = "picSave";
            this.picSave.Size = new System.Drawing.Size(24, 24);
            this.picSave.TabIndex = 7;
            this.picSave.TabStop = false;
            // 
            // picClearSelect
            // 
            this.picClearSelect.Image = ((System.Drawing.Image)(resources.GetObject("picClearSelect.Image")));
            this.picClearSelect.Location = new System.Drawing.Point(248, 8);
            this.picClearSelect.Name = "picClearSelect";
            this.picClearSelect.Size = new System.Drawing.Size(24, 24);
            this.picClearSelect.TabIndex = 6;
            this.picClearSelect.TabStop = false;
            // 
            // picZoom2Select
            // 
            this.picZoom2Select.Image = ((System.Drawing.Image)(resources.GetObject("picZoom2Select.Image")));
            this.picZoom2Select.Location = new System.Drawing.Point(208, 8);
            this.picZoom2Select.Name = "picZoom2Select";
            this.picZoom2Select.Size = new System.Drawing.Size(24, 24);
            this.picZoom2Select.TabIndex = 5;
            this.picZoom2Select.TabStop = false;
            // 
            // picAddDataset
            // 
            this.picAddDataset.Image = ((System.Drawing.Image)(resources.GetObject("picAddDataset.Image")));
            this.picAddDataset.Location = new System.Drawing.Point(168, 8);
            this.picAddDataset.Name = "picAddDataset";
            this.picAddDataset.Size = new System.Drawing.Size(24, 24);
            this.picAddDataset.TabIndex = 4;
            this.picAddDataset.TabStop = false;
            // 
            // picSelect
            // 
            this.picSelect.Image = ((System.Drawing.Image)(resources.GetObject("picSelect.Image")));
            this.picSelect.Location = new System.Drawing.Point(128, 8);
            this.picSelect.Name = "picSelect";
            this.picSelect.Size = new System.Drawing.Size(24, 24);
            this.picSelect.TabIndex = 3;
            this.picSelect.TabStop = false;
            // 
            // picPan
            // 
            this.picPan.Image = ((System.Drawing.Image)(resources.GetObject("picPan.Image")));
            this.picPan.Location = new System.Drawing.Point(88, 8);
            this.picPan.Name = "picPan";
            this.picPan.Size = new System.Drawing.Size(24, 24);
            this.picPan.TabIndex = 2;
            this.picPan.TabStop = false;
            // 
            // picZoomout
            // 
            this.picZoomout.Image = ((System.Drawing.Image)(resources.GetObject("picZoomout.Image")));
            this.picZoomout.Location = new System.Drawing.Point(48, 8);
            this.picZoomout.Name = "picZoomout";
            this.picZoomout.Size = new System.Drawing.Size(24, 24);
            this.picZoomout.TabIndex = 1;
            this.picZoomout.TabStop = false;
            // 
            // picZoomin
            // 
            this.picZoomin.Image = ((System.Drawing.Image)(resources.GetObject("picZoomin.Image")));
            this.picZoomin.Location = new System.Drawing.Point(8, 8);
            this.picZoomin.Name = "picZoomin";
            this.picZoomin.Size = new System.Drawing.Size(24, 24);
            this.picZoomin.TabIndex = 0;
            this.picZoomin.TabStop = false;
            // 
            // Buttons
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(568, 45);
            this.Controls.Add(this.picLoad);
            this.Controls.Add(this.picSave);
            this.Controls.Add(this.picClearSelect);
            this.Controls.Add(this.picZoom2Select);
            this.Controls.Add(this.picAddDataset);
            this.Controls.Add(this.picSelect);
            this.Controls.Add(this.picPan);
            this.Controls.Add(this.picZoomout);
            this.Controls.Add(this.picZoomin);
            this.Name = "Buttons";
            this.Text = "Buttons";
            this.contextMenuStripMeasure.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLoad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picClearSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoom2Select)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddDataset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPan)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoomout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoomin)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion
	}
}

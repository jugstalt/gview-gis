using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    /// <summary>
    /// Zusammenfassung für FormAddDataset.
    /// </summary>
    public class FormAddDataset : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ImageList imageList1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelContents;
        private IMap _map;

        public FormAddDataset(IMap map)
        {
            _map = map;
            //
            // Erforderlich für die Windows Form-Designerunterstützung
            //
            InitializeComponent();
        }

        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormAddDataset));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panelContents = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageSize = new System.Drawing.Size(16, 15);
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(160, 493);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 2;
            this.listView1.View = System.Windows.Forms.View.List;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(160, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 493);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // panelContents
            // 
            this.panelContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContents.Location = new System.Drawing.Point(163, 0);
            this.panelContents.Name = "panelContents";
            this.panelContents.Size = new System.Drawing.Size(541, 493);
            this.panelContents.TabIndex = 4;
            // 
            // FormAddDataset
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(704, 493);
            this.Controls.Add(this.panelContents);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.listView1);
            this.Name = "FormAddDataset";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Dataset";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.FormAddDataset_Closing);
            this.Load += new System.EventHandler(this.FormAddDataset_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void FormAddDataset_Load(object sender, System.EventArgs e)
        {
            //if(_map==null) this.Close();

            listView1.Items.Clear();
            PlugInManager compManager = new PlugInManager();
            foreach (var type in compManager.GetPlugins(Plugins.Type.IDataset))
            {
                IDataset dataset = compManager.CreateInstance<IDataset>(type);
                if (dataset == null)
                {
                    continue;
                }

                listView1.Items.Add(new DatasetProviderItem(dataset));
            }
        }

        async private void ButtonDatasetOK_Click(object sender, System.EventArgs e)
        {
            _datasets.Reset();
            IDataset dataset;

            bool first = true;
            double minx = 0, miny = 0, maxx = 0, maxy = 0;

            bool firstDataset = (_map[0] == null);

            while ((dataset = _datasets.Next) != null)
            {
                FormDatasetProperties datasetProps = await FormDatasetProperties.CreateAsync(_map, dataset);
                if (datasetProps.ShowDialog() != DialogResult.OK)
                {
                    continue;
                }

                _map.AddDataset(dataset, 0);
                if (dataset is IFeatureDataset && _map is Map && _map is IDisplay)
                {
                    IFeatureDataset fDS = (IFeatureDataset)dataset;
                    IEnvelope mapLimit = ((IDisplay)_map).Limit;

                    var fDsEnvelope = await fDS.Envelope();

                    mapLimit.minx = Math.Min(mapLimit.minx, fDsEnvelope.minx);
                    mapLimit.miny = Math.Min(mapLimit.miny, fDsEnvelope.miny);
                    mapLimit.maxx = Math.Max(mapLimit.maxx, fDsEnvelope.maxx);
                    mapLimit.maxy = Math.Max(mapLimit.maxy, fDsEnvelope.maxy);
                    ((IDisplay)_map).Limit = mapLimit;

                    if (first)
                    {
                        minx = fDsEnvelope.minx;
                        miny = fDsEnvelope.miny;
                        maxx = fDsEnvelope.maxx;
                        maxy = fDsEnvelope.maxy;
                        first = false;
                    }
                    else
                    {
                        minx = Math.Min(fDsEnvelope.minx, minx);
                        miny = Math.Min(fDsEnvelope.miny, miny);
                        maxx = Math.Max(fDsEnvelope.maxx, maxx);
                        maxy = Math.Max(fDsEnvelope.maxy, maxy);
                    }
                }
                else if (dataset is IRasterDataset && _map is Map && _map is IDisplay)
                {
                    IRasterDataset fDS = (IRasterDataset)dataset;
                    IEnvelope mapLimit = ((IDisplay)_map).Limit;

                    var fDsEnvelope = await fDS.Envelope();
                    if (fDsEnvelope == null)
                    {
                        continue;
                    }

                    mapLimit.minx = Math.Min(mapLimit.minx, fDsEnvelope.minx);
                    mapLimit.miny = Math.Min(mapLimit.miny, fDsEnvelope.miny);
                    mapLimit.maxx = Math.Max(mapLimit.maxx, fDsEnvelope.maxx);
                    mapLimit.maxy = Math.Max(mapLimit.maxy, fDsEnvelope.maxy);
                    ((IDisplay)_map).Limit = mapLimit;

                    if (first)
                    {
                        minx = fDsEnvelope.minx;
                        miny = fDsEnvelope.miny;
                        maxx = fDsEnvelope.maxx;
                        maxy = fDsEnvelope.maxy;
                        first = false;
                    }
                    else
                    {
                        minx = Math.Min(fDsEnvelope.minx, minx);
                        miny = Math.Min(fDsEnvelope.miny, miny);
                        maxx = Math.Max(fDsEnvelope.maxx, maxx);
                        maxy = Math.Max(fDsEnvelope.maxy, maxy);
                    }
                }
            }
            if (!first)
            {
                if (_map is Map && firstDataset)
                {
                    ((Map)_map).ZoomTo(minx, miny, maxx, maxy);
                }
            }
        }

        private IDatasetEnum _datasets = null;
        private void ShowDatasetDialog()
        {
            /*
			if(listView1.SelectedItems.Count!=1) return;
			DatasetProviderItem item=(DatasetProviderItem)listView1.SelectedItems[0];

			_datasets=item.iDataset.DatasetEnum;
			if(_datasets is Form) 
			{
				panelContents.Controls.Clear();
				
				Form dlg=(Form)_datasets;
				while(dlg.Controls.Count>0) 
				{
					panelContents.Controls.Add(dlg.Controls[0]);
				}
				Button ok=OKButton(panelContents);
				if(ok!=null) 
				{
					ok.Click+=new EventHandler(ButtonDatasetOK_Click);
				}
			}
             * */
        }

        private Button OKButton(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button)
                {
                    if (((Button)control).DialogResult == DialogResult.OK)
                    {
                        return (Button)control;
                    }
                }
                Button ok = OKButton(control);
                if (ok != null)
                {
                    return ok;
                }
            }
            return null;
        }

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ShowDatasetDialog();
        }

        private void FormAddDataset_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }

    internal class DatasetProviderItem : ListViewItem
    {
        IDataset m_dataset;

        public DatasetProviderItem(IDataset dataset)
        {
            m_dataset = dataset;
            if (m_dataset == null)
            {
                return;
            }

            base.Text = m_dataset.ProviderName;
            base.ImageIndex = 0;
        }
        public IDataset iDataset
        {
            get { return m_dataset; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI.Dialogs;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.FDB;
using System.Xml;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormNewTileGridClass : Form
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private IRasterDataset _ds = null;
        private List<int> _createLevels = new List<int>();

        public FormNewTileGridClass()
        {
            InitializeComponent();

            cmbTileType.SelectedIndex = cmbLevelType.SelectedIndex = 0;
            _createLevels.Add(0);
        }

        async private void btnGetGridDataset_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenRasterDatasetFiler());

            ExplorerDialog dlg = new ExplorerDialog("Image/Grid Dataset", filters, true);
            dlg.MulitSelection = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (IExplorerObject exObject in dlg.ExplorerObjects)
                {
                    var instance = await exObject?.GetInstanceAsync();
                    if (instance == null) continue;

                    _ds = instance as IRasterDataset;
                    if (_ds != null)
                    {
                        txtGridDataset.Text = _ds.DatasetName;
                        IEnvelope env = null;
                        foreach (IDatasetElement element in await _ds.Elements())
                        {
                            if (element != null && element.Class is IFeatureClass)
                            {
                                if (env == null)
                                    env = new Envelope(((IFeatureClass)element.Class).Envelope);
                                else
                                    env.Union(((IFeatureClass)element.Class).Envelope);
                            }
                        }
                        spatialIndexControl1.Extent = env;
                    }
                    else
                        txtGridDataset.Text = String.Empty;
                }
            }
        }

        #region Properties
        public string GridName
        {
            get { return txtName.Text; }
        }
        public IRasterDataset RasterDataset
        {
            get { return _ds; }
        }
        public ISpatialIndexDef SpatialIndexDefinition
        {
            get
            {
                return spatialIndexControl1.SpatialIndexDef;
            }
        }
        public double TileSizeX
        {
            get { return (double)numTileSizeX.Value; }
        }
        public double TileSizeY
        {
            get { return (double)numTileSizeY.Value; }
        }
        public double ResolutionX
        {
            get { return (double)numResolutionX.Value; }
        }
        public double ResolutionY
        {
            get { return (double)numResolutionY.Value; }
        }
        public int Levels
        {
            get { return (int)numLevels.Value; }
        }
        public string TileCacheDirectory
        {
            get { return txtCacheDirectory.Text; }
        }
        public bool GenerateTileCache
        {
            get { return chkCreateTiles.Checked; }
        }
        public TileGridType TileGridType
        {
            get
            {
                return (TileGridType)cmbTileType.SelectedIndex;
            }
        }
        public TileLevelType TileLevelType
        {
            get { return (TileLevelType)cmbLevelType.SelectedIndex; }
        }
        public List<int> CreateLevels
        {
            get { return _createLevels; }
        }
        #endregion

        private void btnGetCacheDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtCacheDirectory.Text;

            if (dlg.ShowDialog() == DialogResult.OK)
                txtCacheDirectory.Text = dlg.SelectedPath;
        }

        private void btnImportFromGridDef_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Grid (tilecache.xml)|tilecache.xml";

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dlg.FileName);

                    XmlNode envelopeNode = doc.SelectSingleNode("TileCacheDefinition/Envelope[@minx and @miny and  @maxx and @maxy]");
                    if (envelopeNode == null)
                        throw new Exception("Corrupt Grid file: No Envelope Node defined");

                    spatialIndexControl1.Extent = new gView.Framework.Geometry.Envelope(
                        double.Parse(envelopeNode.Attributes["minx"].Value, _nhi),
                        double.Parse(envelopeNode.Attributes["miny"].Value, _nhi),
                        double.Parse(envelopeNode.Attributes["maxx"].Value, _nhi),
                        double.Parse(envelopeNode.Attributes["maxy"].Value, _nhi));

                    XmlNode tileSizeNode = doc.SelectSingleNode("TileCacheDefinition/TileSize[@x and @y]");
                    if (tileSizeNode == null)
                        throw new Exception("Corrupt Grid file: No TileSize Node defined");
                    numTileSizeX.Value = (decimal)double.Parse(tileSizeNode.Attributes["x"].Value, _nhi);
                    numTileSizeY.Value = (decimal)double.Parse(tileSizeNode.Attributes["y"].Value, _nhi);

                    XmlNode resolutionNode = doc.SelectSingleNode("TileCacheDefinition/TileResolution[@x and @y]");
                    if (resolutionNode == null)
                        throw new Exception("Corrupt Grid file: No TileResolution Node defined");
                    numResolutionX.Value = (decimal)double.Parse(resolutionNode.Attributes["x"].Value, _nhi);
                    numResolutionY.Value = (decimal)double.Parse(resolutionNode.Attributes["y"].Value, _nhi);

                    XmlNode generalNode = doc.SelectSingleNode("TileCacheDefinition/General[@levels]");
                    if (generalNode == null)
                        throw new Exception("Corrupt Grid file: No General Node defined");
                    numLevels.Value = (decimal)int.Parse(generalNode.Attributes["levels"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLevelProperties_Click(object sender, EventArgs e)
        {
            FormTileGridLevels dlg = new FormTileGridLevels(
                (int)numLevels.Value,
                _createLevels, TileSizeX, TileSizeY, ResolutionX, ResolutionY, TileLevelType);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _createLevels = dlg.SelectedLevels;
            }
        }

        private void numLevels_ValueChanged(object sender, EventArgs e)
        {
            int levels = (int)numLevels.Value;
            if (!_createLevels.Contains(levels))
                _createLevels.Add(levels - 1);

            List<int> l = new List<int>();
            for (int i = 0; i < levels; i++)
            {
                if (_createLevels.Contains(i))
                    l.Add(i);
            }
            _createLevels = l;
        }
    }
}

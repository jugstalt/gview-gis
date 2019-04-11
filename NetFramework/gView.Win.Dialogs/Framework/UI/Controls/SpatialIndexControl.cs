using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.FDB;
using gView.Framework.UI.Dialogs;

namespace gView.Framework.UI.Controls
{
    public partial class SpatialIndexControl : UserControl
    {
        public enum IndexType { gView = 0, GEOMETRY = 1, GEOGRAPHY = 2 }

        private ISpatialReference _sRef = null;
        private IndexType _type = IndexType.gView;

        public SpatialIndexControl()
        {
            InitializeComponent();

            cmbIndexType.SelectedIndex = 0;
        }

        public ISpatialReference SpatialReference
        {
            set { _sRef = value; }
        }

        public IndexType Type
        {
            get { return _type; }
            set
            {
                cmbIndexType.SelectedIndex = (int)value;
            }
        }

        #region Extent
        public IEnvelope Extent
        {
            get
            {
                return new Envelope(
                    (double)numLeft.Value, (double)numBottom.Value,
                    (double)numRight.Value, (double)numTop.Value);
            }
            set
            {
                if (value == null) return;
                numLeft.Value = (decimal)value.minx;
                numBottom.Value = (decimal)value.miny;
                numRight.Value = (decimal)value.maxx;
                numTop.Value = (decimal)value.maxy;

                CalcCellSize();
            }
        } 
        #endregion
        
        #region Levels
        public int Levels
        {
            get { return (int)numLevels.Value; }
            set
            {
                try
                {
                    numLevels.Value = value;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        
        private void numLevels_ValueChanged(object sender, EventArgs e)
        {
            CalcCellSize();
        }

        private void CalcCellSize()
        {
            BinaryTree2 tree = new BinaryTree2(Extent, (int)numLevels.Value, 100);

            IEnvelope cell = tree[(long)numLevels.Value];
            numCellSizeX.Value = (decimal)Math.Sqrt(cell.Width * cell.Width + cell.Height * cell.Height);
        }
        #endregion

        #region MSSpatial
        public MSSpatialIndex MSIndex
        {
            get
            {
                if (_type == IndexType.GEOMETRY)
                {
                    MSSpatialIndex index = new MSSpatialIndex();
                    index.GeometryType = GeometryFieldType.MsGeometry;
                    index.SpatialIndexBounds = this.Extent;
                    index.CellsPerObject = (int)numCellsPerObject.Value;
                    index.Level1 = (MSSpatialIndexLevelSize)cmbLevel1.SelectedIndex;
                    index.Level2 = (MSSpatialIndexLevelSize)cmbLevel2.SelectedIndex;
                    index.Level3 = (MSSpatialIndexLevelSize)cmbLevel3.SelectedIndex;
                    index.Level4 = (MSSpatialIndexLevelSize)cmbLevel4.SelectedIndex;
                    return index;
                }
                else if (_type == IndexType.GEOGRAPHY)
                {
                    MSSpatialIndex index = new MSSpatialIndex();
                    index.GeometryType = GeometryFieldType.MsGeography;
                    index.CellsPerObject = (int)numCellsPerObject.Value;
                    index.Level1 = (MSSpatialIndexLevelSize)cmbLevel1.SelectedIndex;
                    index.Level2 = (MSSpatialIndexLevelSize)cmbLevel2.SelectedIndex;
                    index.Level3 = (MSSpatialIndexLevelSize)cmbLevel3.SelectedIndex;
                    index.Level4 = (MSSpatialIndexLevelSize)cmbLevel4.SelectedIndex;
                    return index;
                }
                return null;
            }
            set
            {
                // TODO
            }
        }
        #endregion

        #region Properties
        public ISpatialIndexDef SpatialIndexDef
        {
            get
            {
                switch (_type)
                {
                    case IndexType.gView:
                        return new gViewSpatialIndexDef(
                            this.Extent,
                            this.Levels);
                    case IndexType.GEOMETRY:
                    case IndexType.GEOGRAPHY:
                        return MSIndex;
                }
                return null;
            }
            set
            {
                if (value is gViewSpatialIndexDef)
                {
                    gViewSpatialIndexDef gvIndex = (gViewSpatialIndexDef)value;
                    this.Extent = gvIndex.SpatialIndexBounds;
                    this.Levels = gvIndex.Levels;
                    cmbIndexType.SelectedIndex = 0;
                }
                else if (value is MSSpatialIndex)
                {
                    MSSpatialIndex msIndex = (MSSpatialIndex)value;
                    this.Extent = new Envelope(msIndex.SpatialIndexBounds);
                    numCellsPerObject.Value = msIndex.CellsPerObject;
                    cmbLevel1.SelectedIndex = (int)msIndex.Level1;
                    cmbLevel2.SelectedIndex = (int)msIndex.Level2;
                    cmbLevel3.SelectedIndex = (int)msIndex.Level3;
                    cmbLevel4.SelectedIndex = (int)msIndex.Level4;

                    if (msIndex.GeometryType == GeometryFieldType.MsGeometry)
                    {
                        cmbIndexType.SelectedIndex = 1;
                    }
                    else
                    {
                        cmbIndexType.SelectedIndex = 2;
                    }
                }
            }
        }
        public bool IndexTypeIsEditable
        {
            get { return cmbIndexType.Enabled; }
            set { cmbIndexType.Enabled = value; }
        }
        #endregion

        async private void btnImport_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenDataFilter());

            ExplorerDialog dlg = new ExplorerDialog("Import Extent", filters, true);
            dlg.MulitSelection = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IEnvelope bounds = null;
                foreach (IExplorerObject exObject in dlg.ExplorerObjects)
                {
                    var instance = await exObject?.GetInstanceAsync();
                    if (instance == null) continue;

                    IEnvelope objEnvelope = null;

                    if (instance is IDataset)
                    {
                        foreach (IDatasetElement element in await ((IDataset)instance).Elements())
                        {
                            if (element == null) continue;
                            objEnvelope = ClassEnvelope(element.Class);
                        }
                    }
                    else
                    {
                        objEnvelope = ClassEnvelope(instance as IClass);
                    }

                    if (objEnvelope != null)
                    {
                        if (bounds == null)
                            bounds = new Envelope(objEnvelope);
                        else
                            bounds.Union(objEnvelope);
                    }
                }

                if (bounds != null)
                {
                    this.Extent = bounds;
                }
            }
        }

        async private void btnImportDef_Click(object sender, EventArgs e)
        {
            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFDBFeatureclassFilter());

            ExplorerDialog dlg = new ExplorerDialog("Import Spatial Index", filters, true);
            dlg.MulitSelection = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IEnvelope bounds = null;
                int levels = 0;

                foreach (IExplorerObject exObject in dlg.ExplorerObjects)
                {
                    var instance = await exObject?.GetInstanceAsync();
                    if (instance == null) continue;

                    if (instance is IFeatureClass &&
                        ((IFeatureClass)instance).Dataset != null &&
                        ((IFeatureClass)instance).Dataset.Database is IImplementsBinarayTreeDef)
                    {
                        IFeatureClass fc = (IFeatureClass)instance;
                        IImplementsBinarayTreeDef fdb = (IImplementsBinarayTreeDef)fc.Dataset.Database;

                        BinaryTreeDef def = await fdb.BinaryTreeDef(fc.Name);
                        if (def != null)
                        {
                            if (bounds == null)
                                bounds = new Envelope(def.Bounds);
                            else
                                bounds.Union(def.Bounds);
                            levels = Math.Max(levels, def.MaxLevel);
                        }
                    }
                }

                if (bounds != null)
                {
                    this.Extent = bounds;
                    this.Levels = levels;
                }
            }
        }

        private IEnvelope ClassEnvelope(IClass Class)
        {
            if (Class is IFeatureClass)
            {
                return ProjectEnvelope(
                    ((IFeatureClass)Class).Envelope,
                    ((IFeatureClass)Class).SpatialReference);
            }
            else if (Class is IRasterClass && ((IRasterClass)Class).Polygon != null)
            {
                return ProjectEnvelope(
                    ((IRasterClass)Class).Polygon.Envelope,
                    ((IRasterClass)Class).SpatialReference);
            }
            return null;
        }

        private IEnvelope ProjectEnvelope(IEnvelope env, ISpatialReference sRef)
        {
            if (sRef == null || env == null || _sRef == null) return env;

            IGeometry geom = GeometricTransformer.Transform2D(env, sRef, _sRef);
            if (geom != null) return geom.Envelope;

            return null;
        }

        private void cmbIndexType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIndexType.SelectedIndex == 0)
            {
                panelLevels.Visible = true;
                panelRaster.Visible = false;
            }
            else
            {
                panelLevels.Visible = false;
                panelRaster.Visible = true;

                if (cmbLevel1.SelectedIndex == -1)
                    cmbLevel1.SelectedIndex = (cmbIndexType.SelectedIndex == 1 ? 1 : 3);
                if (cmbLevel2.SelectedIndex == -1)
                    cmbLevel2.SelectedIndex = (cmbIndexType.SelectedIndex == 1 ? 1 : 3);
                if (cmbLevel3.SelectedIndex == -1)
                    cmbLevel3.SelectedIndex = (cmbIndexType.SelectedIndex == 1 ? 1 : 3);
                if (cmbLevel4.SelectedIndex == -1)
                    cmbLevel4.SelectedIndex = (cmbIndexType.SelectedIndex == 1 ? 1 : 3);
            }
            gpExtent.Enabled = cmbIndexType.SelectedIndex != 2;

            _type = (IndexType)cmbIndexType.SelectedIndex;
        }
    }
}

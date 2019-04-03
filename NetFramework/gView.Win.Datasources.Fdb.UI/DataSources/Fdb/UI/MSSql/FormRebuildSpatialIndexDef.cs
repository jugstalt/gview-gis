using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.DataSources.Fdb.MSAccess;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormRebuildSpatialIndexDef : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        
        public FormRebuildSpatialIndexDef(AccessFDB fdb, IFeatureClass fc)
        {
            InitializeComponent();

            _fdb = fdb;
            _fc = fc;

            if (_fdb != null && _fc != null)
            {
                BinaryTreeDef def = _fdb.BinaryTreeDef(_fc.Name);

                if (def != null)
                {
                    spatialIndexControl1.Extent = def.Bounds;
                    spatialIndexControl1.Levels = def.MaxLevel;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_fdb == null || _fc == null)
            {
                return;
            }

            BinaryTreeDef def = new BinaryTreeDef(
                spatialIndexControl1.Extent,
                spatialIndexControl1.Levels);

            FormRebuildSpatialIndexProgess dlg = new FormRebuildSpatialIndexProgess(_fdb, _fc, def);
            dlg.ShowDialog();
        }
    }
}
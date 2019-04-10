using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.DataSources.Fdb.MSAccess;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormRebuildSpatialIndexDef : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        
        private FormRebuildSpatialIndexDef()
        {
            InitializeComponent();
        }

        async static public Task<FormRebuildSpatialIndexDef> Create(AccessFDB fdb, IFeatureClass fc)
        {
            var dlg = new FormRebuildSpatialIndexDef();

            dlg._fdb = fdb;
            dlg._fc = fc;

            if (dlg._fdb != null && dlg._fc != null)
            {
                BinaryTreeDef def = await dlg._fdb.BinaryTreeDef(dlg._fc.Name);

                if (def != null)
                {
                    dlg.spatialIndexControl1.Extent = def.Bounds;
                    dlg.spatialIndexControl1.Levels = def.MaxLevel;
                }
            }

            return dlg;
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
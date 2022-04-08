using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.Fdb.UI
{
    public partial class FormRegisterGeographicView : Form
    {
        private IFeatureDataset _dataset;

        private FormRegisterGeographicView()
        {
            InitializeComponent();
        }

        async static public Task<FormRegisterGeographicView> CreateAsync(IFeatureDataset dataset)
        {
            var dlg = new FormRegisterGeographicView();

            dlg._dataset = dataset;
            await dlg.MakeGui();

            return dlg;
        }

        #region Gui

        async private Task MakeGui()
        {
            cmbDatabaseViews.Items.Clear();
            cmbRefFeatureClass.Items.Clear();

            if (_dataset == null)
            {
                return;
            }

            foreach (IDatasetElement element in await _dataset.Elements())
            {
                if (element.Class is IFeatureClass)
                {
                    if (element.Class.Name.Contains("@"))
                    {
                        continue;
                    }

                    cmbRefFeatureClass.Items.Add(new FeatureClassItem((IFeatureClass)element.Class));
                }
            }

            AccessFDB fdb = _dataset.Database as AccessFDB;
            if (fdb != null)
            {
                foreach (string view in await fdb.DatabaseViews())
                {
                    IFields fields = fdb.TableFields(view);
                    if (fields == null ||
                        fields.FindField("FDB_OID") == null ||
                        fields.FindField("FDB_SHAPE") == null ||
                        fields.FindField("FDB_NID") == null)
                    {
                        continue;
                    }

                    cmbDatabaseViews.Items.Add(view);
                }
            }
        }

        #region Item Classes
        private class FeatureClassItem
        {
            private IFeatureClass _fc;

            public FeatureClassItem(IFeatureClass fc)
            {
                _fc = fc;
            }

            public IFeatureClass Class
            {
                get { return _fc; }
            }

            public override string ToString()
            {
                return _fc.Name;
            }
        }
        #endregion

        #endregion

        #region Properties

        public string SpatialViewAlias
        {
            get
            {
                return cmbRefFeatureClass.SelectedItem.ToString() + "@" + cmbDatabaseViews.SelectedItem.ToString();
            }
        }

        #endregion

        #region Events

        private void cmbRefFeatureClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = cmbDatabaseViews.SelectedItem != null && cmbRefFeatureClass.SelectedItem != null;
        }

        private void cmbDatabaseViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = cmbDatabaseViews.SelectedItem != null && cmbRefFeatureClass.SelectedItem != null;
        }

        #endregion
    }
}

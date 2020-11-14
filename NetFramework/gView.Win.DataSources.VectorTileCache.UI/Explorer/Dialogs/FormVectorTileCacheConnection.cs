using gView.DataSources.VectorTileCache;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Win.DataSources.VectorTileCache.UI.Explorer.Dialogs
{
    public partial class FormVectorTileCacheConnection : Form
    {
        public FormVectorTileCacheConnection()
        {
            InitializeComponent();
        }


        #region Properties

        public string ConnectionString
        {
            get
            {
                return $"name={ txtName.Text };source={ txtCapabilites.Text }";
            }
            set
            {
                txtCapabilites.Text = ConfigTextStream.ExtractValue(value, "source");
                txtName.Text= ConfigTextStream.ExtractValue(value, "name");
            }
        }

        public string VectorTileCacheName
        {
            get
            {
                return txtName.Text;
            }
        }

        #endregion

        async private void btnConnect_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(txtCapabilites.Text))
            {
                Dataset dataset = new Dataset();
                await dataset.SetConnectionString(this.ConnectionString);
                if(await dataset.Open())
                {
                    if (String.IsNullOrWhiteSpace(txtName.Text))
                    {
                        txtName.Text = dataset.DatasetName;
                    }
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

        }
    }
}

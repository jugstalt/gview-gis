using gView.Framework.system;
using System;
using System.Text;
using System.Windows.Forms;

namespace gView.Win.DataSources.GeoJson.UI
{
    public partial class FormGeoJsonConnection : Form
    {
        public FormGeoJsonConnection()
        {
            InitializeComponent();

        }

        public string ConnectionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AddConnectionStringParameter("name", txtName.Text.OrTake("geo-json-service"));
                sb.AddConnectionStringParameter("target", txtTarget.Text);

                var waConnectionString = webAuthControl.ConnectionString;
                if (!String.IsNullOrEmpty(waConnectionString))
                {
                    sb.Append($";{waConnectionString}");
                }

                return sb.ToString();
            }
            set
            {
                value = value ?? String.Empty;

                txtName.Text = value.ExtractConnectionStringParameter("name");
                txtTarget.Text = value.ExtractConnectionStringParameter("target");

                webAuthControl.ConnectionString = value;
            }
        }
    }
}

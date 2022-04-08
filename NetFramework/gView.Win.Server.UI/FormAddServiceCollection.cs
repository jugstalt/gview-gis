using gView.Framework.IO;
using gView.Framework.system;
using gView.Server.Connector;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace gView.MapServer.Lib.UI
{
    public partial class FormAddServiceCollection : Form
    {
        private string _connectionString;
        public FormAddServiceCollection(string connectionString)
        {
            InitializeComponent();

            _connectionString = connectionString;
        }

        private void FormAddServiceCollection_Load(object sender, EventArgs e)
        {
            ServerConnection service = new ServerConnection(ConfigTextStream.ExtractValue(_connectionString, "server"));
            string axl = service.Send("catalog", "<GETCLIENTSERVICES/>", "BB294D9C-A184-4129-9555-398AA70284BC",
                ConfigTextStream.ExtractValue(_connectionString, "user"),
                Identity.HashPassword(ConfigTextStream.ExtractValue(_connectionString, "pwd")));

            if (axl == "")
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(axl);
            foreach (XmlNode mapService in doc.SelectNodes("//SERVICE[@name]"))
            {
                MapServiceType type = MapServiceType.MXL;
                if (mapService.Attributes["servicetype"] != null)
                {
                    switch (mapService.Attributes["servicetype"].Value.ToLower())
                    {
                        case "mxl":
                            type = MapServiceType.MXL;
                            break;
                        case "svc":
                            type = MapServiceType.SVC;
                            break;
                        default:
                            continue;
                    }
                }

                lstAvailServices.Items.Add(
                    new ListViewItem(mapService.Attributes["name"].Value,
                    (type == MapServiceType.MXL) ? 0 : 1));
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lstAvailServices.SelectedItems == null)
            {
                return;
            }

            foreach (ListViewItem item in lstAvailServices.SelectedItems)
            {
                bool found = false;
                foreach (ListViewItem i in lstServices.Items)
                {
                    if (item.Text == i.Text)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    lstServices.Items.Add(new ListViewItem(item.Text, item.ImageIndex));
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstServices.SelectedItems == null)
            {
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in lstServices.SelectedItems)
            {
                items.Add(item);
            }

            foreach (ListViewItem item in items)
            {
                lstServices.Items.Remove(item);
            }
        }

        public string CollectionName
        {
            get { return txtCollectionName.Text; }
        }

        public string[] Services
        {
            get
            {
                List<string> services = new List<string>();
                foreach (ListViewItem item in lstServices.Items)
                {
                    services.Add(item.Text);
                }
                if (services.Count == 0)
                {
                    return null;
                }

                return services.ToArray();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace gView.MapServer.Lib.UI
{
    public partial class FormGdiServers : Form
    {
        public FormGdiServers(string xml)
        {
            InitializeComponent();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode servers = doc.SelectSingleNode("servers[@max]");
            if (servers == null) return;

            int max = int.Parse(servers.Attributes["max"].Value);
            foreach (XmlNode server in servers.SelectNodes("server[@uri]"))
            {
                lstServers.Items.Add(
                    new ListViewItem(server.Attributes["uri"].Value,
                    lstServers.Items.Count < max ? 0 : 1));
                if (server.Attributes["port"] != null)
                    lstServers.Items[lstServers.Items.Count - 1].SubItems.Add(server.Attributes["port"].Value);
            }
            txtMax.Text = max.ToString();
        }
    }
}
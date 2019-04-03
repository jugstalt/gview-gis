using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.Globalisation;

namespace gView.Framework.UI.Dialogs
{
    [gView.Framework.system.RegisterPlugIn("09918356-AB35-4ed1-8C5E-034B79857931")]
    public partial class FormLayerSource : Form, ILayerPropertyPage
    {
        public FormLayerSource()
        {
            InitializeComponent();
        }

        #region ILayerPropertyPage Member

        public Panel PropertyPage(IDataset dataset, ILayer layer)
        {
            if (!ShowWith(dataset, layer)) 
                return null;

            if (dataset.ConnectionString.Trim().StartsWith("<"))
            {
                // Connectionstring ist XML, zB beim EventTable
                // TODO: noch nicht implementert, darum geht nicht mit diese Dialog
                return null;
            }
            txtDatasetType.Text = dataset.DatasetGroupName + " (" + dataset.ProviderName + ")";
            txtDatasetname.Text = dataset.DatasetName;
            txtConnectionString.Text = PrepareConnectionString(dataset.ConnectionString);

            txtLayerSID.Text = layer.SID;

            return panel1;
        }

        public bool ShowWith(IDataset dataset, ILayer layer)
        {
            return (dataset != null);
        }

        public string Title
        {
            get { return LocalizedResources.GetResString("String.Source", "Source"); }
        }

        public void Commit()
        {
            
        }

        #endregion

        private string PrepareConnectionString(string str)
        {
            Dictionary<string, string> dic = ConfigTextStream.Extract(str);

            StringBuilder sb = new StringBuilder();
            foreach (string key in dic.Keys)
            {
                if (String.IsNullOrEmpty(dic[key]))
                    sb.Append(key);
                else
                {
                    switch (key.ToLower())
                    {
                        case "password":
                        case "pwd":
                            sb.Append(key + "=*****");
                            break;
                        default:
                            sb.Append(key + "=" + dic[key]);
                            break;
                    }
                }
                sb.Append("\r\n");
            }

            return sb.ToString();
        }
    }
}
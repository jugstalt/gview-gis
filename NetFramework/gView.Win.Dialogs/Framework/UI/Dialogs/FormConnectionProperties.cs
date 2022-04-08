using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormConnectionProperties : Form
    {
        private IDataset _dataset;
        private string _dsConnectionString = String.Empty;
        private IMap _map;

        public FormConnectionProperties(IMap map, IDataset dataset)
        {
            InitializeComponent();

            _map = map;
            _dataset = dataset;
            btnChange.Enabled = _dataset is IConnectionStringDialog;
            if (_dataset != null)
            {
                _dsConnectionString = _dataset.ConnectionString;
            }
        }

        #region Events
        private void FormConnectionProperties_Load(object sender, EventArgs e)
        {
            txtConnectionString.Text = PrepareConnectionString(_dataset.ConnectionString);
        }
        async private void btnOK_Click(object sender, EventArgs e)
        {
            if (_dataset != null &&
                _dataset.ConnectionString != _dsConnectionString)
            {
                await _dataset.SetConnectionString(_dsConnectionString);
                await _dataset.Open();

                int dsIndex = 0;
                IDataset ds;
                while ((ds = _map[dsIndex]) != null)
                {
                    if (ds == _dataset)
                    {
                        break;
                    }

                    dsIndex++;
                }
                if (ds == null)
                {
                    return;
                }

                foreach (IDatasetElement element in _map.MapElements)
                {
                    if (!(element is DatasetElement))
                    {
                        continue;
                    }

                    if (element.DatasetID == dsIndex)
                    {
                        IDatasetElement el = await _dataset.Element(element.Title);
                        if (el == null)
                        {
                            ((DatasetElement)element).Class2 = null;
                        }
                        else
                        {
                            ((DatasetElement)element).Class2 = el.Class;
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper
        private string PrepareConnectionString(string str)
        {
            if (str.Trim().StartsWith("<"))
            {
                // Connectionstring ist XML, zB beim EventTable
                // TODO: noch nicht implementert, darum geht nicht mit diese Dialog
                return String.Empty;
            }

            Dictionary<string, string> dic = ConfigTextStream.Extract(str);

            StringBuilder sb = new StringBuilder();
            foreach (string key in dic.Keys)
            {
                if (String.IsNullOrEmpty(dic[key]))
                {
                    sb.Append(key);
                }
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
        #endregion

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (_dataset is IConnectionStringDialog)
            {
                string connStr = ((IConnectionStringDialog)_dataset).ShowConnectionStringDialog(_dataset.ConnectionString);
                if (!String.IsNullOrEmpty(connStr))
                {
                    _dsConnectionString = connStr;
                    txtConnectionString.Text = PrepareConnectionString(_dsConnectionString);
                }
            }
        }
    }
}
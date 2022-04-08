using gView.Framework.Carto;
using System;
using System.Windows.Forms;

namespace gView.Framework.UI.Symbology.Dialogs
{
    public partial class FormMapResourcePicker : Form
    {
        private readonly IMap _currentMap;

        public FormMapResourcePicker(IMap currentMap)
        {
            _currentMap = currentMap;

            InitializeComponent();

            BuildList();
        }

        void BuildList()
        {
            lstResources.Items.Clear();

            if (_currentMap?.ResourceContainer != null)
            {
                foreach (var resourceName in _currentMap.ResourceContainer.Names)
                {
                    lstResources.Items.Add(resourceName);
                }
            }
        }

        private void lstResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (lstResources.SelectedItem != null);
        }

        public void SetAbortButtonText(string text)
        {
            btnAbort.Text = text;
        }

        public string SelectedResourceName
        {
            get
            {
                return lstResources.SelectedItem?.ToString();
            }
        }
    }
}

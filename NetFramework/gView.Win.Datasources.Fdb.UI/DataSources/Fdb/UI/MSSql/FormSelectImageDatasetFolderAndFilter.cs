using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.system;
using System.Xml;
using gView.Framework.Data;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormSelectImageDatasetFolderAndFilter : Form
    {
        public FormSelectImageDatasetFolderAndFilter()
        {
            InitializeComponent();

            FillFormatList();
        }

        #region Properties
        public string SelectedFolder
        {
            get { return txtFolder.Text; }
            set { txtFolder.Text = value; }
        }
        public string[] FormatFilters
        {
            get
            {
                string filter = txtFilter.Text.Trim();
                if (filter.EndsWith("*")) filter = filter.Substring(0, filter.Length - 1);

                string[] filters = new string[lstFormats.CheckedItems.Count];

                for(int i=0;i<lstFormats.CheckedItems.Count;i++)
                {
                    filters[i] = filter + lstFormats.CheckedItems[i].Text;
                }
                return filters;
            }
        }

        public Dictionary<string, Guid> ProviderGuids
        {
            get
            {
                Dictionary<string, Guid> ret = new Dictionary<string, Guid>();
                for (int i = 0; i < lstFormats.CheckedItems.Count; i++)
                {
                    FormatListItem item = (FormatListItem)lstFormats.CheckedItems[i];
                    string extension = item.Text.ToLower();

                    int pos = extension.LastIndexOf(".");
                    if (pos > 0)
                        extension = extension.Substring(pos, extension.Length - pos);

                    if(ret.ContainsKey(extension))
                        throw new Exception("You can't select a second provider for '"+extension+"'");
                    ret.Add(extension, item.ProviderGuid);
                }

                return (ret.Count > 0 ? ret : null);
            }
        }
        #endregion

        #region Events
        private void btnGetFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dlg.SelectedPath;
            }
        }

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (txtFolder.Text != String.Empty && lstFormats.CheckedItems.Count > 0);
        }

        private void lstFormats_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (txtFolder.Text == String.Empty)
            {
                btnOK.Enabled = false;
            }
            else
            {
                if (e.NewValue == CheckState.Checked)
                    btnOK.Enabled = true;
                else
                    btnOK.Enabled = lstFormats.CheckedItems.Count > 1;
            }
        }
        #endregion

        #region Helper
        private void FillFormatList()
        {
            PlugInManager compMan = new PlugInManager();
            foreach (XmlNode ds in compMan.GetPluginNodes(Plugins.Type.IDataset))
            {
                IRasterFileDataset rds = compMan.CreateInstance(ds) as IRasterFileDataset;
                if (rds == null) continue;

                foreach (string format in rds.SupportedFileFilter.Split('|'))
                {
                    if (format == String.Empty) continue;

                    int priority = rds.SupportsFormat(format.Replace("*", ""));
                    //FormatListItem item = FindFormatItem(format);
                    //if (item != null)
                    //{
                    //    if (item.Priority < priority)
                    //    {
                    //        item.Provider = rds.ToString();
                    //        item.Priority = priority;
                    //    }
                    //}
                    //else
                    //{
                    //    lstFormats.Items.Add(new FormatListItem(format, rds.ToString(), priority));
                    //}
                    lstFormats.Items.Add(new FormatListItem(format, rds.ToString(), priority, PlugInManager.PlugInID(rds)));
                }
            }
        }

        private FormatListItem FindFormatItem(string format)
        {
            foreach (FormatListItem item in lstFormats.Items)
            {
                if (format.ToLower() == item.Format.ToLower()) return item;
            }
            return null;
        }

        private class FormatListItem : ListViewItem
        {
            private string _format, _provider;
            private int _priority = 0;

            public FormatListItem(string format, string provider, int priority, Guid providerGuid)
            {
                _format = format;
                _provider = provider;
                _priority = priority;

                ProviderGuid = providerGuid;

                base.Checked = false;
                base.Text = format;
                base.SubItems.Add(provider);
            }

            public string Format
            {
                get { return _format; }
            }
            public string Provider
            {
                get { return _provider; }
                set
                {
                    _provider = value;
                    base.SubItems[1].Text = _provider;
                }
            }
            public Guid ProviderGuid
            {
                get;
                set;
            }
            public int Priority
            {
                get { return _priority; }
                set { _priority = value; }
            }
        }


        #endregion
    }
}
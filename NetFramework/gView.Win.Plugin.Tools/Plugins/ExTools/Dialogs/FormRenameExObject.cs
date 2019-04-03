using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;

namespace gView.Plugins.ExTools.Dialogs
{
    public partial class FormRenameExObject : Form
    {
        private IExplorerObjectRenamable _exObject = null;
        private string _oldName;

        public FormRenameExObject(IExplorerObject exObject)
        {
            InitializeComponent();

            if (exObject is IExplorerObjectRenamable)
            {
                if (exObject.Icon != null && exObject.Icon.Image != null)
                {
                    IL.Images.Add(exObject.Icon.Image);
                    lstObjects.Items.Add(new ExplorerObjectListViewItem(exObject, IL.Images.Count - 1));
                }
                else
                {
                    lstObjects.Items.Add(new ExplorerObjectListViewItem(exObject, 0));
                }
                txtName.Text = _oldName = exObject.Name;
                _exObject = (IExplorerObjectRenamable)exObject;

                txtName.Select();
                txtName.Focus();
                
            }
        }

        #region Item Classes
        private class ExplorerObjectListViewItem : ListViewItem
        {
            IExplorerObject _exObject = null;
            public ExplorerObjectListViewItem(IExplorerObject exObject, int imageIndex)
            {
                if (exObject == null) return;
                base.Text = exObject.FullName;
                base.ImageIndex = imageIndex;
                base.Checked = true;

                _exObject = exObject;
            }

            public IExplorerObject ExplorerObject
            {
                get { return _exObject; }
            }
        }
        #endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_exObject != null && _oldName != txtName.Text)
            {
                if (!_exObject.RenameExplorerObject(txtName.Text))
                {
                    MessageBox.Show("Can't rename object", "ERROR");
                }
            }
        }
    }
}
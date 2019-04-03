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
    public partial class FormDeleteExObjects : Form
    {
        public FormDeleteExObjects(List<IExplorerObject> exObjects)
        {
            InitializeComponent();

            if (exObjects == null) return;
            foreach (IExplorerObject exObject in exObjects)
            {
                if (exObject is IExplorerObjectDeletable)
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
                }
            }
        }

        public List<IExplorerObject> Selected
        {
            get
            {
                List<IExplorerObject> list=new List<IExplorerObject>();
                foreach (ExplorerObjectListViewItem item in lstObjects.Items)
                {
                    if (item.Checked)
                        list.Add(item.ExplorerObject);
                }
                return list;
            }
        }
    }

    internal class ExplorerObjectListViewItem : ListViewItem
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
}
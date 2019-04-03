using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Management;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Data;
using gView.Framework.UI.Controls;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormCatalogTree : UserControl, IDockableWindow
    {
        public delegate void NodeClickedEvent(TreeNode node);
        public event NodeClickedEvent NodeSelected = null;
        public delegate void NodeDeletedEvent(IExplorerObject exObject);
        public event NodeDeletedEvent NodeDeleted = null;
        public delegate void NodeRenamedEvent(IExplorerObject exObject);
        public event NodeRenamedEvent NodeRenamed = null;

        public FormCatalogTree(IExplorerApplication application)
            :this(application,true)
        {
        }
        public FormCatalogTree(IExplorerApplication application, bool init)
        {
            InitializeComponent();

            this.catalogTreeControl1.ExplorerApplication = application;
            if (init)
                InitTree(true);
        }

        public void InitTree(bool expand)
        {
            catalogTreeControl1.InitTreeView(expand);
        }

        public void ExpandRoot()
        {
            catalogTreeControl1.ExpandRoot();
        }

        public void CollapseRoot()
        {
            catalogTreeControl1.CollapseRoot();
        }

        #region IDockableWindow Members

        public string Name
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
        private DockWindowState _dockState = DockWindowState.left;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _dockState;
            }
            set
            {
                _dockState = value;
            }
        }

        public Image Image
        {
            get { return null; }
        }

        #endregion

        public CatalogTreeControl TreeControl
        {
            get { return catalogTreeControl1; }
        }

        public void RefreshSelectedNode()
        {
            TreeControl.RefreshSelectedNode();
        }

        private void FormCatalogTree_Load(object sender, EventArgs e)
        {
            //catalogTreeControl1.InitTreeView();
        }

        private void catalogTreeControl1_NodeSelected(TreeNode node)
        {
            if (NodeSelected != null) NodeSelected(node);
        }

        private void catalogTreeControl1_NodeRenamed(IExplorerObject exObject)
        {
            if (NodeRenamed != null) NodeRenamed(exObject);
        }

        private void catalogTreeControl1_NodeDeleted(IExplorerObject exObject)
        {
            if (NodeDeleted != null) NodeDeleted(exObject);
        }

        public bool MoveToNode(string path)
        {
            return catalogTreeControl1.MoveToNode(path);
        }

        public IExplorerObject SelectedExplorerObject
        {
            get { return catalogTreeControl1.SelectedExplorerObject; }
        }

        public void SelectRootNode()
        {
            catalogTreeControl1.SelectRootNode();
        }
    }

    /*
    internal class DirectoryNode : TreeNode
    {
        public DirectoryNode(string text)
        {
            base.Text = text;
            base.ImageIndex = 1;
            base.SelectedImageIndex = 2;
            base.Nodes.Add(new DummyNode());
        }

        public DirectoryNode(string text,int imageIndex)
        {
            base.Text = text;
            base.ImageIndex = base.SelectedImageIndex = imageIndex;
            base.Nodes.Add(new DummyNode());
        }

        public string FullName
        {
            get
            {
                return getFullName(this,base.Text);
            }
        }

        private string getFullName(DirectoryNode node,string text)
        {
            if (node.Parent is DirectoryNode)
            {
                return getFullName((DirectoryNode)node.Parent, node.Parent.Text + @"\" + text);
            }
            else
            {
                return text;
            }
        }
    }
    */

    
}
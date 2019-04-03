using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.IO;

namespace gView.Framework.UI.Controls
{
    public partial class PersistStreamGrid : UserControl
    {
        public PersistStreamGrid()
        {
            InitializeComponent();
        }

        public IPersistStream PersistStream
        {
            get
            {
                if (treeView1.Nodes.Count != 1) return null;
                PersistableClassTreeNode node = treeView1.Nodes[0] as PersistableClassTreeNode;
                if (node == null || node.PersistableClass == null) return null;

                return node.PersistableClass.ToXmlStream();
            }

            set
            {
                treeView1.Nodes.Clear();
                propertyGrid1.SelectedObject = null;
                
                if (value == null)
                {
                    return;
                }

                PersistableClass cls = new PersistableClass(value as XmlStream);
                AddPersisableClass(cls,null);

                if (treeView1.Nodes.Count > 0)
                {
                    treeView1.Nodes[0].Expand();
                    if (treeView1.Nodes[0].Nodes.Count > 0)
                    {
                        treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0];
                    }
                }
            }
        }

        private void AddPersisableClass(PersistableClass cls, TreeNode parent)
        {
            if (cls == null) return;
            PersistableClassTreeNode node = new PersistableClassTreeNode(cls);

            if (parent == null)
                treeView1.Nodes.Add(node);
            else
                parent.Nodes.Add(node);

            foreach (PersistableClass child in cls.ChildClasses)
                AddPersisableClass(child, node);
        }

        #region ItemClasses
        private class PersistableClassTreeNode : TreeNode
        {
            private PersistableClass _class;

            public PersistableClassTreeNode(PersistableClass cls)
            {
                _class = cls;
                if (_class == null) return;
                base.Text = _class.Name;
            }

            public PersistableClass PersistableClass
            {
                get { return _class; }
            }
        }
        #endregion

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            PersistableClassTreeNode node=treeView1.SelectedNode as PersistableClassTreeNode;
            if (node == null)
            {
                propertyGrid1.SelectedObject = null;
            }
            else
            {
                propertyGrid1.SelectedObject = node.PersistableClass;
            }

            txtPath.Text = treeView1.SelectedNode.FullPath;
        }
    }
}

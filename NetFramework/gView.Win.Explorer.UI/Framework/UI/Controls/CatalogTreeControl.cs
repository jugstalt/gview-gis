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
using gView.Explorer.UI;
using gView.Framework.system.UI;
using gView.Framework.IO;
using gView.Framework.UI.Dialogs;
using gView.system.UI.Framework.system.UI;

namespace gView.Framework.UI.Controls
{
    public partial class CatalogTreeControl : UserControl
    {
        public delegate void NodeClickedEvent(TreeNode node);
        public event NodeClickedEvent NodeSelected = null;
        public delegate void NodeDeletedEvent(IExplorerObject exObject);
        public event NodeDeletedEvent NodeDeleted = null;
        public delegate void NodeRenamedEvent(IExplorerObject exObject);
        public event NodeRenamedEvent NodeRenamed = null;

        private ToolStripMenuItem _renameMenuItem, _deleteMenuItem;
        private Filter.ExplorerOpenDialogFilter _filter = null;
        private IExplorerApplication _app = null;

        private ContentsList _contentsList = null;

        public CatalogTreeControl()
        {
            InitializeComponent();

            //ExplorerImageList.List.AddImages(treeView.ImageList);
            treeView.UIImageList = ExplorerImageList.List;

            _renameMenuItem = new ToolStripMenuItem("Rename...");
            _renameMenuItem.Click += new EventHandler(_renameMenuItem_Click);
            _deleteMenuItem = new ToolStripMenuItem("Delete");
            _deleteMenuItem.Click += new EventHandler(_deleteMenuItem_Click);
        }

        public IExplorerApplication ExplorerApplication
        {
            get { return _app; }
            set { _app = value; }
        }

        internal ContentsList ContentsListView
        {
            get { return _contentsList; }
            set { _contentsList = value; }
        }

        void _deleteMenuItem_Click(object sender, EventArgs e)
        {
            if (_contextNode is ExplorerObjectNode)
            {
                TreeNode parent = ((TreeNode)_contextNode).Parent;

                IExplorerObject exObject = _contextNode.ExplorerObject;
                if (exObject is IExplorerObjectDeletable)
                {
                    ExplorerObjectEventArgs args = new ExplorerObjectEventArgs();
                    //args.Node = _contextNode;
                    if (((IExplorerObjectDeletable)exObject).DeleteExplorerObject(args))
                    {
                        int pos = -1;
                        if (parent != null)
                        {
                            pos = ((TreeNode)_contextNode).Parent.Nodes.IndexOf(_contextNode);
                            parent.Nodes.Remove(_contextNode);
                        }
                        else
                        {
                            pos = treeView.Nodes.IndexOf(_contextNode);
                            treeView.Nodes.Remove(_contextNode);
                        }
                        if (pos != -1 && NodeDeleted != null) NodeDeleted(_contextNode.ExplorerObject);
                    }
                }

                if (parent != null && parent.Nodes.Count == 0)
                {
                    parent.Nodes.Add(new DummyNode());
                    parent.Collapse();
                }
                _contextNode = null;
            }
        }

        void _renameMenuItem_Click(object sender, EventArgs e)
        {

        }

        public Filter.ExplorerOpenDialogFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        private delegate void InitTreeViewCallback(bool expand);
        public void InitTreeView(bool expand)
        {
            if (this.InvokeRequired)
            {
                InitTreeViewCallback d = new InitTreeViewCallback(InitTreeView);
                this.Invoke(d, new object[] { expand });
            }
            else
            {
                treeView.Nodes.Clear();

                ComputerObject computer = new ComputerObject(_app);
                treeView.Nodes.Add(new ExplorerObjectNode(computer, 0));

                if (expand)
                    treeView.Nodes[0].Expand();
                treeView.SelectedNode = treeView.Nodes[0];
            }
        }

        public void ExpandRoot()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { ExpandRoot(); }));
            }
            else
            {
                if (treeView.Nodes.Count > 0)
                    treeView.Nodes[0].Expand();
            }
        }

        public void CollapseRoot()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { CollapseRoot(); }));
            }
            else
            {
                if (treeView.Nodes.Count > 0)
                    treeView.Nodes[0].Collapse();
            }
        }

        public void SelectRootNode()
        {
            if (treeView.Nodes.Count > 0)
                treeView.SelectedNode = treeView.Nodes[0];
        }

        internal void InsertComputerChildNodes(IExplorerObject computerObject, TreeNode computer)
        {
            //DateTime t1 = DateTime.Now;
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT Name,DriveType From Win32_LogicalDisk ");
            ManagementObjectCollection collection = query.Get();

            foreach (ManagementObject manObject in collection)
            {
                DriveObject exObject = new DriveObject(computerObject, manObject["Name"].ToString(), (uint)manObject["DriveType"]);
                ExplorerObjectNode d = new ExplorerObjectNode(exObject, exObject.ImageIndex);
                computer.Nodes.Add(d);
            }

            //TimeSpan ts = DateTime.Now-t1;
            //MessageBox.Show(ts.TotalMilliseconds.ToString());
        }

        internal void SelectChildNode(IExplorerObject exObject)
        {
            if (exObject == null) return;
            TreeNode node = treeView.SelectedNode;
            if (node == null) return;
            node.Expand();

            foreach (TreeNode n in node.Nodes)
            {
                if (!(n is ExplorerObjectNode)) continue;
                IExplorerObject ex = ((ExplorerObjectNode)n).ExplorerObject;
                if (ex == null) continue;

                if (ExObjectComparer.Equals(ex, exObject))
                {
                    treeView.SelectedNode = n;
                    return;
                }
            }
        }
        internal void RemoveChildNode(IExplorerObject exObject)
        {
            if (exObject == null) return;
            TreeNode node = treeView.SelectedNode;
            if (node == null) return;

            bool removed = false;
            if (node.IsExpanded)
            {
                int index = -1;
                foreach (TreeNode n in node.Nodes)
                {
                    if (!(n is ExplorerObjectNode)) continue;
                    IExplorerObject ex = ((ExplorerObjectNode)n).ExplorerObject;
                    if (ex == null) continue;

                    if (ExObjectComparer.Equals(ex, exObject))
                    {
                        index = node.Nodes.IndexOf(n);
                        break;
                    }
                }
                if (index != -1)
                {
                    node.Nodes.RemoveAt(index);
                    removed = true;
                }
            }
            if (removed)
            {
                if (node.Nodes.Count == 0)
                {
                    node.Nodes.Add(new DummyNode());
                    node.Collapse();
                }
            }
            else
            {
                ExplorerObjectNode exNode = FindNode(exObject);

                if (exNode != null)
                {
                    if (exNode == treeView.SelectedNode)
                    {
                        treeView.SelectedNode = treeView.SelectedNode.Parent;
                        treeView.Nodes.Remove(exNode);

                        if (treeView.SelectedNode.Nodes.Count == 0)
                        {
                            treeView.SelectedNode.Nodes.Add(new DummyNode());
                            treeView.SelectedNode.Collapse();
                        }
                    }
                    else
                    {
                        treeView.Nodes.Remove(exNode);
                    }
                }
            }
        }

        internal ExplorerObjectNode FindNode(IExplorerObject exObject)
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                ExplorerObjectNode n = FindNode(node, exObject);
                if (n != null) return n;
            }
            return null;
        }
        internal ExplorerObjectNode FindNode(TreeNode parent, IExplorerObject exObject)
        {
            if (parent == null) return null;

            if (parent is ExplorerObjectNode &&
                ExObjectComparer.Equals(((ExplorerObjectNode)parent).ExplorerObject, exObject))
            {
                return parent as ExplorerObjectNode;
            }

            foreach (TreeNode node in parent.Nodes)
            {
                ExplorerObjectNode n = FindNode(node, exObject);
                if (n != null) return n;
            }
            return null;
        }
        internal IExplorerObject AddChildNode(IExplorerObject exObject)
        {
            if (!(treeView.SelectedNode is ExplorerObjectNode) || exObject == null)
                return exObject;

            ExplorerObjectNode node = treeView.SelectedNode as ExplorerObjectNode;
            if (node.ExplorerObject is IExplorerParentObject)
            {
                ((IExplorerParentObject)node.ExplorerObject).Refresh();
                foreach (IExplorerObject child in ((IExplorerParentObject)node.ExplorerObject).ChildObjects)
                {
                    if (ExObjectComparer.Equals(child, exObject))
                    {
                        exObject = child;
                        break;
                    }
                }
            }

            if (exObject is IExplorerObjectDeletable)
                ((IExplorerObjectDeletable)exObject).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(CatalogTreeControl_ExplorerObjectDeleted);
            if (exObject is IExplorerObjectRenamable)
                ((IExplorerObjectRenamable)exObject).ExplorerObjectRenamed += new ExplorerObjectRenamedEvent(CatalogTreeControl_ExplorerObjectRenamed);

            if (!node.IsExpanded) return exObject;

            int imageIndex = gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObject);
            node.Nodes.Add(new ExplorerObjectNode(exObject, imageIndex));

            return exObject;
        }
        internal bool HasChildNode(IExplorerObject exObject)
        {
            if (exObject == null) return true;
            TreeNode node = treeView.SelectedNode;
            if (node == null) return true;
            node.Expand();

            foreach (TreeNode n in node.Nodes)
            {
                if (!(n is ExplorerObjectNode)) continue;
                IExplorerObject ex = ((ExplorerObjectNode)n).ExplorerObject;
                if (ex == null) continue;

                if (ExObjectComparer.Equals(ex, exObject))
                {
                    return true;
                }
            }
            return false;
        }
        private void FormCatalogTree_Load(object sender, EventArgs e)
        {

        }

        public void RefreshSelectedNode()
        {
            if (treeView.SelectedNode == null || !treeView.SelectedNode.IsExpanded) return;
            InsertNodeElements(treeView.SelectedNode);
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = !InsertNodeElements(e.Node);
        }

        /*
        private void InsertNodeElementsTask(object parameters)
        {
            TreeNode treenode = (TreeNode)((object[])parameters)[0];
            IExplorerObject exObject = (IExplorerObject)((object[])parameters)[1];

            IStatusBar statusbar = (_app != null) ? _app.StatusBar : null;
            List<ExplorerObjectNode> treeNodes = new List<ExplorerObjectNode>();

            List<IExplorerObject> childObjects = ((IExplorerParentObject)exObject).ChildObjects;
            if (childObjects == null) return;
            int pos = 0, count = childObjects.Count;
            if (statusbar != null) statusbar.ProgressVisible = true;

            foreach (IExplorerObject exObj in childObjects)
            {
                if (exObj == null ||
                    exObj is IExplorerObjectDoubleClick) continue;

                if (statusbar != null)
                {
                    statusbar.ProgressValue = (int)(((double)pos++ / (double)count) * 100.0);
                    statusbar.Text = exObj.Name;
                    statusbar.Refresh();
                }

                if (_filter != null)
                {
                    if (_filter.Match(exObj))
                        continue;
                    else if (!(exObj is IExplorerParentObject)) continue;
                }
                int imageIndex = FormExplorer.ImageIndex(exObj);
                treeNodes.Add(new ExplorerObjectNode(exObj, imageIndex));

                if (exObj is IExplorerObjectDeletable)
                    ((IExplorerObjectDeletable)exObj).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(CatalogTreeControl_ExplorerObjectDeleted);
                if (exObj is IExplorerObjectRenamable)
                    ((IExplorerObjectRenamable)exObj).ExplorerObjectRenamed += new ExplorerObjectRenamedEvent(CatalogTreeControl_ExplorerObjectRenamed);
                if (exObj is IRefreshedEventHandler)
                    ((IRefreshedEventHandler)exObj).Refreshed += new RefreshedEventHandler(CatalogTreeControl_Refreshed);
            }

            if (treenode.Nodes.Count == 0)
            {
                treenode.Nodes.Add(new DummyNode());
                return;
            }
        }

        private delegate void InsertNodeElementsCallback(TreeNode treeNode, List<ExplorerObjectNode> childNodes);
        private void InsertNodeElements(TreeNode treeNode, List<ExplorerObjectNode> childNodes)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new InsertNodeElementsCallback(InsertNodeElements), new object[] { treeNode, childNodes });
            }
            else
            {
                foreach (ExplorerObjectNode childNode in childNodes)
                {
                    treeNode.Nodes.Add(childNode);
                }
            }
        }
*/

        private bool InsertNodeElements(TreeNode treenode)
        {
            if (!(treenode is ExplorerObjectNode))
            {
                return true;
            }
            ExplorerObjectNode node = (ExplorerObjectNode)treenode;
            IExplorerObject exObject = node.ExplorerObject;
            if (exObject == null) return true;

            try
            {
                Cursor = Cursors.WaitCursor;

                if (exObject is IExplorerParentObject)
                {
                    //System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(InsertNodeElementsTask));
                    //thread.Start(new object[] { treenode, exObject });

                    treenode.Nodes.Clear();
                    IStatusBar statusbar = (_app != null) ? _app.StatusBar : null;
                    List<IExplorerObject> childObjects = ((IExplorerParentObject)exObject).ChildObjects;
                    if (childObjects == null) return false;
                    int pos = 0, count = childObjects.Count;
                    if (statusbar != null) statusbar.ProgressVisible = true;

                    foreach (IExplorerObject exObj in childObjects)
                    {
                        if (exObj == null ||
                            exObj is IExplorerObjectDoubleClick) continue;

                        if (statusbar != null)
                        {
                            statusbar.ProgressValue = (int)(((double)pos++ / (double)count) * 100.0);
                            statusbar.Text = exObj.Name;
                            statusbar.Refresh();
                        }

                        if (_filter != null)
                        {
                            if (_filter.Match(exObj))
                                continue;
                            else if (!(exObj is IExplorerParentObject)) continue;
                        }
                        int imageIndex = gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObj);
                        treenode.Nodes.Add(new ExplorerObjectNode(exObj, imageIndex));

                        if (exObj is IExplorerObjectDeletable)
                            ((IExplorerObjectDeletable)exObj).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(CatalogTreeControl_ExplorerObjectDeleted);
                        if (exObj is IExplorerObjectRenamable)
                            ((IExplorerObjectRenamable)exObj).ExplorerObjectRenamed += new ExplorerObjectRenamedEvent(CatalogTreeControl_ExplorerObjectRenamed);
                        if (exObj is IRefreshedEventHandler)
                            ((IRefreshedEventHandler)exObj).Refreshed += new RefreshedEventHandler(CatalogTreeControl_Refreshed);
                    }

                    if (statusbar != null)
                    {
                        statusbar.ProgressVisible = false;
                        statusbar.Text = String.Empty;
                        statusbar.Refresh();
                    }
                }

                if (treenode.Nodes.Count == 0)
                {
                    treenode.Nodes.Add(new DummyNode());
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                MessageBox.Show(ex.Message);
                treenode.Nodes.Add(new DummyNode());
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void CatalogTreeControl_Refreshed(object sender)
        {
            if (sender is IExplorerObject &&
                treeView.SelectedNode is ExplorerObjectNode &&
                ((ExplorerObjectNode)treeView.SelectedNode).ExplorerObject == (IExplorerObject)sender)
            {
                RefreshSelectedNode();
            }
        }

        void CatalogTreeControl_ExplorerObjectRenamed(IExplorerObject exObject)
        {
            if (exObject == null) return;
            ExplorerObjectNode node = FindNode(exObject);
            if (node != null)
            {
                node.Text = exObject.Name;
            }
        }

        void CatalogTreeControl_ExplorerObjectDeleted(IExplorerObject exObject)
        {

            this.RemoveChildNode(exObject);
        }

        ExplorerObjectNode _contextNode = null;
        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView.GetNodeAt(e.X, e.Y);
            if (!(node is ExplorerObjectNode)) return;
            IExplorerObject exObject = ((ExplorerObjectNode)node).ExplorerObject;
            if (exObject == null) return;

            Cursor = Cursors.WaitCursor;

            if (e.Button == MouseButtons.Right)
            {
                //ContextMenuStrip strip = new ContextMenuStrip();
                //CommandInterpreter.AppendMenuItems(strip, exObject);

                //if (_app != null)
                //{
                //    List<IExplorerObject> exObjects = new List<IExplorerObject>();
                //    exObjects.Add(exObject);
                //    _app.AppendContextMenuItems(strip, exObjects);
                //}
                //if (exObject is IExplorerObjectRenamable)
                //{
                //    if (strip.Items.Count > 0) strip.Items.Add(new ToolStripSeparator());
                //    strip.Items.Add(_renameMenuItem);
                //}

                if (_contentsList != null)
                {
                    ContextMenuStrip strip = _contentsList.BuildContextMenu(exObject);

                    if (strip != null && strip.Items.Count > 0)
                    {
                        //_contextNode = (ExplorerObjectNode)node;
                        strip.Show(treeView, new Point(e.X, e.Y));
                    }
                }
            }
            else
            {
                //if (NodeClicked != null) NodeClicked(node);
            }

            Cursor = Cursors.Default;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node is ExplorerObjectNode)) return;

            if (NodeSelected != null) NodeSelected(e.Node);
        }
        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            ExplorerObjectNode node = treeView.SelectedNode as ExplorerObjectNode;
            if (node == null) return;

            if (node.ExplorerObject is IExplorerParentObject && !node.IsExpanded)
            {
                ((IExplorerParentObject)node.ExplorerObject).DiposeChildObjects();
            }
        }
        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node is ComputerNode) return;

            ExplorerObjectNode node = e.Node as ExplorerObjectNode;
            if (node == null || node == treeView.SelectedNode) return;

            if (node.ExplorerObject is IExplorerParentObject)
            {
                ((IExplorerParentObject)node.ExplorerObject).DiposeChildObjects();
            }
            //Cursor = Cursors.WaitCursor;
            //RemoveAndDisposeChildNodes(e.Node);
            //e.Node.Nodes.Add(new DummyNode());
            //Cursor = Cursors.Default;
        }


        private void RemoveAndDisposeChildNodes(TreeNode node)
        {
            if (node == null) return;

            foreach (TreeNode child in node.Nodes)
            {
                if (child is ExplorerObjectNode && ((ExplorerObjectNode)child).ExplorerObject != null)
                {
                    ((ExplorerObjectNode)child).ExplorerObject.Dispose();
                }
                RemoveAndDisposeChildNodes(child);
            }
            node.Nodes.Clear();
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            TreeNode node = treeView.GetNodeAt(mouseX, mouseY);
            if (node is ExplorerObjectNode)
            {
                if (((ExplorerObjectNode)node).ExplorerObject is IExplorerObjectDoubleClick)
                {
                    ExplorerObjectEventArgs args = new ExplorerObjectEventArgs();
                    //args.Node = (ExplorerObjectNode)node;
                    ((IExplorerObjectDoubleClick)((ExplorerObjectNode)node).ExplorerObject).ExplorerObjectDoubleClick(args);
                }
            }
        }

        private int mouseX = 0, mouseY = 0;
        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = (TreeNode)e.Item;

            if (node is ExplorerObjectNode && ((ExplorerObjectNode)node).ExplorerObject != null)
            {
                List<IExplorerObjectSerialization> exObjects = new List<IExplorerObjectSerialization>();
                IExplorerObjectSerialization ser = ExplorerObjectManager.SerializeExplorerObject(((ExplorerObjectNode)node).ExplorerObject);
                if (ser != null)
                {
                    exObjects.Add(ser);
                    this.DoDragDrop(exObjects, DragDropEffects.Copy);
                }
            }
        }

        public void MoveUp()
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Parent == null) return;

            treeView.SelectedNode = treeView.SelectedNode.Parent;
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode node = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));

            if (!(node is ExplorerObjectNode))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            IExplorerObject exObject = (IExplorerObject)((ExplorerObjectNode)node).ExplorerObject;
            if (!(exObject is IExplorerObjectContentDragDropEvents))
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            ((IExplorerObjectContentDragDropEvents)exObject).Content_DragDrop(e);

            InsertNodeElements(node);
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            TreeNode node = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));

            if (!(node is ExplorerObjectNode))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            IExplorerObject exObject = (IExplorerObject)((ExplorerObjectNode)node).ExplorerObject;
            if (!(exObject is IExplorerObjectContentDragDropEvents))
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            ((IExplorerObjectContentDragDropEvents)exObject).Content_DragEnter(e);
        }

        public bool MoveToNode(string path)
        {
            if (treeView.Nodes.Count < 1) return false;

            TreeNode parent = treeView.Nodes[0];
            parent.Expand();

            StringBuilder fullpath = new StringBuilder();

            foreach (TreeNode node in parent.Nodes)
            {
                IExplorerObject exObject = ((ExplorerObjectNode)node).ExplorerObject;
                if (exObject is DriveObject && exObject.FullName.Contains(@"\"))
                {
                    if ((path + @"\").ToLower().IndexOf(exObject.FullName.ToLower()) == 0)
                    {
                        treeView.SelectedNode = node;
                        parent = node;
                        node.Expand();
                        fullpath.Append(exObject.FullName);
                        path = path.Substring(exObject.FullName.Length, path.Length - exObject.FullName.Length);
                        break;
                    }
                }
            }

            foreach (string subPath in path.Split('\\'))
            {
                if (String.IsNullOrEmpty(subPath))
                    continue;

                if (fullpath.Length != 0 && !fullpath.ToString().EndsWith(@"\")) fullpath.Append(@"\");
                fullpath.Append(subPath);

                bool found = false;
                foreach (TreeNode node in parent.Nodes)
                {
                    if (!(node is ExplorerObjectNode)) continue;

                    IExplorerObject exObject = ((ExplorerObjectNode)node).ExplorerObject;
                    if (exObject == null) continue;

                    if (exObject.FullName == fullpath.ToString() ||
                        exObject.FullName == fullpath.ToString() + @"\")
                    {
                        found = true;
                        treeView.SelectedNode = node;
                        parent = node;
                        node.Expand();
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        public IExplorerObject SelectedExplorerObject
        {
            get
            {
                if (treeView.SelectedNode is ExplorerObjectNode)
                {
                    return ((ExplorerObjectNode)treeView.SelectedNode).ExplorerObject;
                }
                return null;
            }
        }

    }

    public class ComputerNode : TreeNode
    {
        public ComputerNode(string name, int imageIndex)
            : base(name, imageIndex, imageIndex)
        {
        }
    }

    public class ExplorerObjectNode : TreeNode, IExplorerObjectTreeNode
    {
        private IExplorerObject _exObject;

        public ExplorerObjectNode(IExplorerObject exObject, int imageIndex)
        {
            if (exObject == null) return;

            _exObject = exObject;
            base.Text = exObject.Name;
            base.ImageIndex = base.SelectedImageIndex = imageIndex;
            if (exObject is IExplorerParentObject || exObject is DriveObject || exObject is DirectoryObject)
                base.Nodes.Add(new DummyNode());
        }

        #region IExplorerObjectTreeNode Members

        public IExplorerObject ExplorerObject
        {
            get { return _exObject; }
        }

        //public new List<IExplorerObjectTreeNode> Nodes
        //{
        //    get
        //    {
        //        List<IExplorerObjectTreeNode> nodes = new List<IExplorerObjectTreeNode>();

        //        foreach (TreeNode node in base.Nodes)
        //        {
        //            if (!(node is IExplorerObjectTreeNode)) continue;
        //            nodes.Add((IExplorerObjectTreeNode)node);
        //        }
        //        return nodes;
        //    }
        //}

        //public void AddNode(TreeNode node)
        //{
        //    base.Nodes.Add(node);
        //}

        public new IExplorerObjectTreeNode Parent
        {
            get
            {
                if (base.Parent is IExplorerObjectTreeNode) return (IExplorerObjectTreeNode)base.Parent;
                return null;
            }
        }

        #endregion
    }

    public class DummyNode : TreeNode
    {
        public DummyNode()
        {
            base.Text = "";
        }
    }
}

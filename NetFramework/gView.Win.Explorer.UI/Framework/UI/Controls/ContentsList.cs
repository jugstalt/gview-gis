using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Globalisation;
using gView.Framework.IO;
using gView.Framework.Offline;
using gView.Framework.Sys.UI;
using gView.Framework.system;
using gView.Framework.system.UI;
using gView.Framework.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI.Controls
{
    [gView.Framework.system.RegisterPlugIn("47328671-87F7-4cf7-B59D-2BC111A29BA3")]
    public partial class ContentsList : UserControl, IExplorerTabPage
    {
        public delegate void ItemClickedEvent(List<IExplorerObject> node);
        public event ItemClickedEvent ItemSelected = null;
        public delegate void ItemDoubleClickedEvent(ListViewItem node);
        public event ItemDoubleClickedEvent ItemDoubleClicked = null;

        private IExplorerObject _exObject = null;
        private ToolStripMenuItem _renameMenuItem, _deleteMenuItem, _metadataMenuItem, _replicationMenuItem, _appendReplicationIDMenuItem, _checkoutMenuItem, _checkinMenuItem;
        private CatalogTreeControl _tree = null;
        private Filter.ExplorerDialogFilter _filter = null;
        private int _contextItemCount = 0;
        private IExplorerApplication _app = null;
        private bool _allowContextMenu = true, _open = true;
        private Thread _worker = null;
        private CancelTracker _cancelTracker = new CancelTracker();

        public ContentsList()
        {
            InitializeComponent();

            listView.SmallImageList = ExplorerImageList.List.ImageList;
            _renameMenuItem = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.Rename", "Rename"));
            _renameMenuItem.Click += new EventHandler(_renameMenuItem_Click);
            _deleteMenuItem = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.Delete", "Delete"));
            _deleteMenuItem.Click += new EventHandler(_deleteMenuItem_Click);
            _metadataMenuItem = new ToolStripMenuItem(LocalizedResources.GetResString("Menu.Metadata", "Metadata..."));
            _metadataMenuItem.Click += new EventHandler(_metadataMenuItem_Click);

            _appendReplicationIDMenuItem = new ToolStripMenuItem("Append ReplicationID");
            _appendReplicationIDMenuItem.Click += new EventHandler(_appendReplicationIDMenuItem_Click);
            _checkoutMenuItem = new ToolStripMenuItem("Checkout...");
            _checkoutMenuItem.Click += new EventHandler(_checkoutMenuItem_Click);
            _checkinMenuItem = new ToolStripMenuItem("Checkin...");

            _checkinMenuItem.Click += new EventHandler(_checkinMenuItem_Click);
            _replicationMenuItem = new ToolStripMenuItem("Replication");
            _replicationMenuItem.DropDownItems.Add(_appendReplicationIDMenuItem);
            _replicationMenuItem.DropDownItems.Add(_checkoutMenuItem);
            _replicationMenuItem.DropDownItems.Add(_checkinMenuItem);

            _contextItemCount = contextStrip.Items.Count;

            LocalizedResources.GlobalizeMenuItem(contextStrip);
        }

        internal CatalogTreeControl TreeControl
        {
            get { return _tree; }
            set
            {
                _tree = value;

                if (_tree != null)
                {
                    _tree.ContentsListView = this;
                    _tree.NodeDeleted += new CatalogTreeControl.NodeDeletedEvent(tree_NodeDeleted);
                    _tree.NodeRenamed += new CatalogTreeControl.NodeRenamedEvent(tree_NodeRenamed);
                }

            }
        }

        #region TreeEvents
        void tree_NodeDeleted(IExplorerObject exObject)
        {
            DeleteItem(exObject);
        }

        void tree_NodeRenamed(IExplorerObject exObject)
        {
            RenameItem(exObject);
        }
        #endregion

        internal ImageList SmallImageList
        {
            get { return listView.SmallImageList; }
            set { listView.SmallImageList = listView.LargeImageList = value; }
        }

        public List<IExplorerObject> SelectedExplorerObjects
        {
            get
            {
                List<IExplorerObject> selected = new List<IExplorerObject>();
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    if (item is ExplorerObjectListViewItem && ((ExplorerObjectListViewItem)item).ExplorerObject != null)
                    {
                        selected.Add(((ExplorerObjectListViewItem)item).ExplorerObject);
                    }
                }
                return selected;
            }
        }
        public View View
        {
            get { return listView.View; }
            set { listView.View = value; }
        }
        public bool IsOpenDialog
        {
            get { return _open; }
            set { _open = value; }
        }

        public Filter.ExplorerDialogFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        #region IExplorerTabPage Members

        public new Control Control
        {
            get { return this; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication && ((IExplorerApplication)hook).ApplicationWindow is gView.Explorer.UI.Framework.UI.IFormExplorer)
            {
                _app = hook as IExplorerApplication;
                this.TreeControl = ((gView.Explorer.UI.Framework.UI.IFormExplorer)((IExplorerApplication)hook).ApplicationWindow).CatalogTree.TreeControl;
            }
        }

        async public Task<bool> OnShow()
        {
            await BuildList();
            return true;
        }
        public void OnHide()
        {
        }

        public Task<bool> ShowWith(IExplorerObject exObject)
        {
            return Task.FromResult(exObject is IExplorerParentObject);
        }

        public string Title { get { return "Contents"; } }

        async public Task<bool> RefreshContents()
        {
            //if (this.InvokeRequired)
            //{
            //    RefreshContextDelegate d = new RefreshContextDelegate(RefreshContents);
            //    this.Invoke(d);
            //}
            //else
            {

                if (!(_exObject is IExplorerParentObject))
                {
                    return false;
                }

                Cursor = Cursors.WaitCursor;
                await ((IExplorerParentObject)_exObject).Refresh();
                await BuildList();

                base.Refresh();
                Cursor = Cursors.Default;
            }

            return true;
        }

        public IExplorerObject GetExplorerObject()
        {
            return _exObject;
        }
        async public Task SetExplorerObjectAsync(IExplorerObject value)
        {
            if (_exObject == value)
            {
                return;
            }

            listView.Items.Clear();
            _exObject = value;
            if (_exObject == null)
            {
                return;
            }

            await BuildList();
        }

        #endregion

        async private Task BuildList()
        {
            await BuildList(false);
        }
        async private Task BuildList(bool checkTreeNodes)
        {
            if (_worker != null)
            {
                _cancelTracker.Cancel();
                //while (_worker != null) ;
            }

            //_worker=new Thread(new ParameterizedThreadStart(BuildListThread));
            //_worker.Start(checkTreeNodes);

            await BuildListThread(checkTreeNodes);
        }

        #region BuildListThread

        async private Task BuildListThread(object chkTreeNodes)
        {
            bool checkTreeNodes = (bool)chkTreeNodes;
            ClearList();

            if (_exObject is IExplorerParentObject)
            {
                List<IExplorerObject> childs = await ((IExplorerParentObject)_exObject).ChildObjects();
                if (childs != null)
                {
                    IStatusBar statusbar = (_app != null) ? _app.StatusBar : null;
                    List<IExplorerObject> childObjects = await ((IExplorerParentObject)_exObject).ChildObjects();
                    if (childObjects == null)
                    {
                        return;
                    }

                    int pos = 0, count = childObjects.Count;
                    if (statusbar != null)
                    {
                        statusbar.ProgressVisible = true;
                    }

                    foreach (IExplorerObject exObject in childObjects)
                    {
                        if (exObject == null)
                        {
                            continue;
                        }

                        if (statusbar != null)
                        {
                            statusbar.ProgressValue = (int)(((double)pos++ / (double)count) * 100.0);
                            statusbar.Text = exObject.Name;
                            statusbar.Refresh();
                        }

                        if (!_cancelTracker.Continue)
                        {
                            break;
                        }

                        if (_filter != null &&
                            !(exObject is IExplorerParentObject) &&
                            !(exObject is IExplorerObjectDoubleClick) &&
                            _open)
                        {
                            if (!await _filter.Match(exObject))
                            {
                                continue;
                            }
                        }
                        int imageIndex = gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(exObject);
                        if (imageIndex == -1)
                        {
                            continue;
                        }

                        string[] texts = null;
                        if (exObject is IExporerOjectSchema)
                        {
                            texts = new string[] { exObject.Name, exObject.Type, ((IExporerOjectSchema)exObject).Schema };
                        }
                        else
                        {
                            texts = new string[] { exObject.Name, exObject.Type };
                        }

                        ListViewItem item = new ExplorerObjectListViewItem(texts, exObject);
                        item.ImageIndex = imageIndex;

                        AddListItem(item);
                        if (checkTreeNodes && _tree != null)
                        {
                            await CheckTree(exObject);
                        }

                        if (exObject is IExplorerObjectDeletable)
                        {
                            ((IExplorerObjectDeletable)exObject).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(ContentsList_ExplorerObjectDeleted);
                        }

                        if (exObject is IExplorerObjectRenamable)
                        {
                            ((IExplorerObjectRenamable)exObject).ExplorerObjectRenamed += new ExplorerObjectRenamedEvent(ContentsList_ExplorerObjectRenamed);
                        }
                    }

                    if (statusbar != null)
                    {
                        statusbar.ProgressVisible = false;
                        statusbar.Text = pos.ToString() + " Objects...";
                        statusbar.Refresh();
                    }
                }
            }

            _worker = null;
        }

        private delegate void ClearListCallback();
        private void ClearList()
        {
            if (this.InvokeRequired)
            {
                ClearListCallback d = new ClearListCallback(ClearList);
                this.Invoke(d);
            }
            else
            {
                listView.Items.Clear();
            }
        }

        private delegate void AddListItemCallback(ListViewItem item);
        private void AddListItem(ListViewItem item)
        {
            if (this.InvokeRequired)
            {
                AddListItemCallback d = new AddListItemCallback(AddListItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                //listView.Items.Add(item);
                int index = listView.Items.Count, count = listView.Items.Count;
                if (item is ExplorerObjectListViewItem)
                {
                    var exObject = ((ExplorerObjectListViewItem)item).ExplorerObject;
                    if (exObject != null)
                    {
                        for (index = 0; index < count; index++)
                        {
                            ListViewItem listItem = listView.Items[index];
                            if (listItem is ExplorerObjectListViewItem)
                            {
                                var listExObject = ((ExplorerObjectListViewItem)listItem).ExplorerObject;
                                if (listExObject != null)
                                {
                                    if (exObject.Priority < listExObject.Priority)
                                    {
                                        break;
                                    }

                                    if (exObject.Priority == listExObject.Priority &&
                                        exObject.Name.ToLower().CompareTo(listExObject.Name.ToLower()) < 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                listView.Items.Insert(Math.Min(count, index), item);
            }
        }

        private delegate void CheckTreeCallback(IExplorerObject exObject);
        async private Task CheckTree(IExplorerObject exObject)
        {
            //if (this.InvokeRequired)
            //{
            //    CheckTreeCallback d = new CheckTreeCallback(CheckTree);
            //    this.Invoke(d, new object[] { exObject });
            //}
            //else
            {
                if (!_tree.HasChildNode(exObject))
                {
                    await _tree.AddChildNode(exObject);
                }
            }
        }
        #endregion

        private void ContentsList_ExplorerObjectRenamed(IExplorerObject exObject)
        {
            ListViewItem item = FindItem(exObject);
            if (item != null)
            {
                item.Text = exObject.Name;
            }
        }

        private void ContentsList_ExplorerObjectDeleted(IExplorerObject exObject)
        {
            ListViewItem item = FindItem(exObject);
            if (item != null)
            {
                listView.Items.Remove(item);
            }
            //_tree.RemoveChildNode(exObject);
        }

        private ListViewItem FindItem(IExplorerObject exObject)
        {
            if (exObject == null)
            {
                return null;
            }

            foreach (ListViewItem item in listView.Items)
            {
                if (item is ExplorerObjectListViewItem && ((ExplorerObjectListViewItem)item).ExplorerObject == exObject)
                {
                    return item;
                }
            }
            return null;
        }

        public bool AllowContextMenu
        {
            get { return _allowContextMenu; }
            set { _allowContextMenu = value; }
        }

        #region DragDrop
        async private void listView_DragDrop(object sender, DragEventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                Cursor = Cursors.WaitCursor;
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_DragDrop(e);
                Cursor = Cursors.Default;

                await BuildList(true);
            }
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_DragEnter(e);
            }
        }

        private void listView_DragLeave(object sender, EventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_DragLeave(e);
            }
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_DragOver(e);
            }
        }

        private void listView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_GiveFeedback(e);
            }
        }

        private void listView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (_exObject is IExplorerObjectContentDragDropEvents)
            {
                ((IExplorerObjectContentDragDropEvents)_exObject).Content_QueryContinueDrag(e);
            }
        }
        #endregion

        #region IOrder Members

        public int SortOrder
        {
            get { return 0; }
        }

        #endregion

        #region ContextMenu

        async private void listView_Click(object sender, EventArgs e)
        {
            await ShowContextMenu();
        }

        async private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            _button = e.Button;
            if (listView.GetItemAt(_mX, _mY) != null)
            {
                return;
            }

            await ShowContextMenu();
        }

        //private ExplorerObjectListViewItem _contextItem = null;
        private IExplorerObject _contextObject = null;
        ContextMenuStrip _strip = null;
        async private Task ShowContextMenu()
        {
            if (!_allowContextMenu)
            {
                return;
            }

            ListViewItem item = listView.GetItemAt(_mX, _mY);
            IExplorerObject exObject;
            if (item is ExplorerObjectListViewItem)
            {
                exObject = ((ExplorerObjectListViewItem)item).ExplorerObject;
            }
            else
            {
                exObject = _exObject;
            }
            if (exObject == null)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;

            if (_button == MouseButtons.Right)
            {
                ContextMenuStrip strip = await BuildContextMenu(exObject, this.SelectedExplorerObjects, item == null);

                if (strip != null && strip.Items.Count > 0)
                {
                    if (item == null)
                    {
                        _contextObject = null;
                    }
                    else
                    {
                        _contextObject = ((ExplorerObjectListViewItem)item).ExplorerObject;
                    }

                    _strip.Show(listView, new Point(_mX, _mY));
                }
            }

            Cursor = Cursors.Default;
        }

        async internal Task<ContextMenuStrip> BuildContextMenu(IExplorerObject exObject)
        {
            List<IExplorerObject> context = new List<IExplorerObject>();
            context.Add(exObject);
            return await BuildContextMenu(exObject, context, false);
        }
        async private Task<ContextMenuStrip> BuildContextMenu(IExplorerObject exObject, List<IExplorerObject> context, bool emptyContentsClick)
        {
            if (_strip != null && _strip.Visible == true)
            {
                _strip.Close();
            }

            _strip = contextStrip;
            for (int i = _strip.Items.Count - 1; i >= _contextItemCount; i--)
            {
                _strip.Items.RemoveAt(i);
            }

            toolStripMenuItemNew.DropDownItems.Clear();
            PlugInManager compMan = new PlugInManager();
            foreach (var compType in compMan.GetPlugins(Plugins.Type.IExplorerObject))
            {
                IExplorerObject ex = compMan.CreateInstance<IExplorerObject>(compType);
                if (ex is IExplorerObjectCreatable)
                {
                    if (!((IExplorerObjectCreatable)ex).CanCreate(_exObject))
                    {
                        continue;
                    }

                    ToolStripItem createNewItem = new CreateNewToolStripItem(ex);
                    createNewItem.Click += new EventHandler(createNewItem_Click);
                    toolStripMenuItemNew.DropDownItems.Add(createNewItem);
                }
            }
            toolStripMenuItemNew.Enabled = (toolStripMenuItemNew.DropDownItems.Count != 0);

            if (_app != null)
            {
                _app.AppendContextMenuItems(_strip, emptyContentsClick ? null : context/*this.SelectedExplorerObjects*/);
            }
            CommandInterpreter.AppendMenuItems(_strip, exObject);

            if (!emptyContentsClick)
            {
                //if (exObject is IExplorerObjectRenamable)
                //{
                //    if (_strip.Items.Count > 0) _strip.Items.Add(new ToolStripSeparator());
                //    _strip.Items.Add(_renameMenuItem);
                //}
                //if (exObject is IExplorerObjectDeletable)
                //{
                //    _strip.Items.Add(_deleteMenuItem);
                //}
            }

            if (exObject is IExplorerObjectContextMenu)
            {
                ToolStripItem[] contextItems = ((IExplorerObjectContextMenu)exObject).ContextMenuItems;
                if (contextItems != null && contextItems.Length > 0)
                {
                    _strip.Items.Add(new ToolStripSeparator());
                    foreach (ToolStripItem contextItem in contextItems)
                    {
                        _strip.Items.Add(contextItem);
                    }
                }
            }
            if (exObject is IExplorerObjectContextMenu2)
            {
                ToolStripItem[] contextItems = ((IExplorerObjectContextMenu2)exObject).ContextMenuItems(RefreshContents);
                if (contextItems != null && contextItems.Length > 0)
                {
                    _strip.Items.Add(new ToolStripSeparator());
                    foreach (ToolStripItem contextItem in contextItems)
                    {
                        _strip.Items.Add(contextItem);
                    }
                }
            }

            var exObjectInstance = await exObject.GetInstanceAsync();
            if (exObject != null && exObjectInstance is IFeatureClass)
            {
                IFeatureClass fc = (IFeatureClass)exObjectInstance;
                if (fc.Dataset != null && fc.Dataset.Database is IFeatureDatabaseReplication)
                {
                    _strip.Items.Add(new ToolStripSeparator());
                    _strip.Items.Add(_replicationMenuItem);

                    _appendReplicationIDMenuItem.Enabled = !await Replication.FeatureClassHasRelicationID(fc);
                    _checkoutMenuItem.Enabled = await Replication.FeatureClassCanReplicate(fc);
                    _checkinMenuItem.Enabled = (await Replication.FeatureClassGeneration(fc) > 0);
                }
            }

            if (exObject is IMetadata)
            {
                _strip.Items.Add(new ToolStripSeparator());
                _strip.Items.Add(_metadataMenuItem);
            }

            _contextObject = exObject;
            return _strip;
        }

        private int _mX = 0, _mY = 0;
        private MouseButtons _button = MouseButtons.None;
        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            _mX = e.X;
            _mY = e.Y;
        }

        #endregion

        async public void createNewItem_Click(object sender, EventArgs e)
        {
            if (!(sender is CreateNewToolStripItem))
            {
                return;
            }

            IExplorerObject exObject = ((CreateNewToolStripItem)sender).ExplorerObject;
            await CreateNewItem(exObject);
        }

        async public Task CreateNewItem(IExplorerObject exObject)
        {
            if (!(exObject is IExplorerObjectCreatable))
            {
                return;
            }

            IExplorerObject newExObj = await ((IExplorerObjectCreatable)exObject).CreateExplorerObject(_exObject);
            if (newExObj == null)
            {
                return;
            }

            if (_tree != null)
            {
                newExObj = await _tree.AddChildNode(newExObj);
            }

            int imageIndex = gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(newExObj);
            string[] texts = { newExObj.Name, newExObj.Type };
            ListViewItem item = new ExplorerObjectListViewItem(texts, newExObj);
            item.ImageIndex = imageIndex;

            if (newExObj is IExplorerObjectDeletable)
            {
                ((IExplorerObjectDeletable)newExObj).ExplorerObjectDeleted += new ExplorerObjectDeletedEvent(ContentsList_ExplorerObjectDeleted);
            }

            if (newExObj is IExplorerObjectRenamable)
            {
                ((IExplorerObjectRenamable)newExObj).ExplorerObjectRenamed += new ExplorerObjectRenamedEvent(ContentsList_ExplorerObjectRenamed);
            }

            listView.Items.Add(item);
        }


        async private void listView_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = listView.GetItemAt(_mX, _mY);

            if (item is ExplorerObjectListViewItem &&
                ((ExplorerObjectListViewItem)item).ExplorerObject is IExplorerObjectDoubleClick)
            {
                ExplorerObjectEventArgs args = new ExplorerObjectEventArgs();
                ((IExplorerObjectDoubleClick)((ExplorerObjectListViewItem)item).ExplorerObject).ExplorerObjectDoubleClick(args);
                await CheckExplorerObjectEventArgs(args);
                return;
            }

            if (_tree != null)
            {
                if (!(item is ExplorerObjectListViewItem))
                {
                    return;
                }

                _tree.SelectChildNode(((ExplorerObjectListViewItem)item).ExplorerObject);
            }
            if (ItemDoubleClicked != null)
            {
                ItemDoubleClicked(item);
            }
        }

        async private Task CheckExplorerObjectEventArgs(ExplorerObjectEventArgs args)
        {
            if (args.NewExplorerObject != null)
            {
                int imageIndex = gView.Explorer.UI.Framework.UI.ExplorerIcons.ImageIndex(args.NewExplorerObject);
                string[] texts = { args.NewExplorerObject.Name, args.NewExplorerObject.Type };
                ListViewItem item = new ExplorerObjectListViewItem(texts, args.NewExplorerObject);
                item.ImageIndex = imageIndex;

                listView.Items.Add(item);
                if (_tree != null)
                {
                    await _tree.AddChildNode(args.NewExplorerObject);
                }
            }
        }

        void _deleteMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextItem is ExplorerObjectListViewItem && listView.SelectedItems.Count == 1 && _contextItem == listView.SelectedItems[0])
            if (_contextObject != null)
            {
                IExplorerObject exObject = _contextObject; //_contextItem.ExplorerObject;
                if (MessageBox.Show("Do you realy want to delete the selected item (" + _contextObject.Name + ") ?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (exObject is IExplorerObjectDeletable)
                {
                    ExplorerObjectEventArgs args = new ExplorerObjectEventArgs();
                    //args.Node = _contextItem;
                    ((IExplorerObjectDeletable)exObject).DeleteExplorerObject(args);

                    //if (_tree != null) _tree.RemoveChildNode(exObject);
                    //listView.Items.Remove(_contextItem);
                }
                _contextObject = null;
            }
            //else if (listView.SelectedItems.Count > 1 && listView.SelectedItems.Contains(_contextItem))
            else if (listView.SelectedItems.Count > 1)
            {
                bool found = false;
                foreach (ExplorerObjectListViewItem item in listView.SelectedItems)
                {
                    if (item.ExplorerObject == _contextObject)
                    {
                        found = true;
                        break;
                    }
                }
                _contextObject = null;
                if (!found)
                {
                    return;
                }

                if (MessageBox.Show("Do you realy want to delete the " + listView.SelectedItems.Count + " selected items?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                foreach (ListViewItem item in listView.SelectedItems)
                {
                    if (!(item is ExplorerObjectListViewItem))
                    {
                        continue;
                    }

                    IExplorerObject exObject = ((ExplorerObjectListViewItem)item).ExplorerObject;
                    if (!(exObject is IExplorerObjectDeletable))
                    {
                        continue;
                    }

                    ExplorerObjectEventArgs args = new ExplorerObjectEventArgs();
                    ((IExplorerObjectDeletable)exObject).DeleteExplorerObject(args);

                    //if (_tree != null) _tree.RemoveChildNode(exObject);
                    //listView.Items.Remove(item);
                }
            }
        }

        void _renameMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextObject != null)
            //{
            //    FormRenameExplorerObject dlg = new FormRenameExplorerObject(_contextObject);
            //    if (dlg.ShowDialog() == DialogResult.OK)
            //    {
            //        this.RefreshContents();
            //    }
            //}
            //BeginRename();
        }

        async void _metadataMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextItem is ExplorerObjectListViewItem)
            if (_contextObject != null)
            {
                IExplorerObject exObject = _contextObject; // ((ExplorerObjectListViewItem)_contextItem).ExplorerObject;
                if (exObject is IMetadata)
                {
                    XmlStream xmlStream = new XmlStream(String.Empty);
                    ((IMetadata)exObject).ReadMetadata(xmlStream);

                    var exObjectInstance = await exObject.GetInstanceAsync();
                    FormMetadata dlg = new FormMetadata(xmlStream, exObjectInstance);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        await ((IMetadata)exObject).WriteMetadata(await dlg.GetStream());
                    }
                }
                _contextObject = null;
            }
        }

        async void _appendReplicationIDMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextItem is ExplorerObjectListViewItem && listView.SelectedItems.Count == 1 && _contextItem == listView.SelectedItems[0])
            if (_contextObject != null)
            {
                IExplorerObject exObject = _contextObject; //_contextItem.ExplorerObject;

                var exObjectInstance = await exObject?.GetInstanceAsync();

                if (exObjectInstance is IFeatureClass &&
                    ((IFeatureClass)exObjectInstance).Dataset != null)
                {
                    IFeatureDatabase fdb = ((IFeatureClass)exObjectInstance).Dataset.Database as IFeatureDatabase;
                    if (fdb is IFeatureDatabaseReplication)
                    {
                        ReplicationUI.ShowAddReplicationIDDialog((IFeatureClass)exObjectInstance);
                    }
                }
                _contextObject = null;
            }
        }

        async void _checkoutMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextItem is ExplorerObjectListViewItem && listView.SelectedItems.Count == 1 && _contextItem == listView.SelectedItems[0])
            if (_contextObject != null)
            {
                IExplorerObject exObject = _contextObject; // _contextItem.ExplorerObject;

                var exObjectInstance = await exObject?.GetInstanceAsync();

                if (exObjectInstance is IFeatureClass &&
                    ((IFeatureClass)exObjectInstance).Dataset != null)
                {
                    IFeatureDatabase fdb = ((IFeatureClass)exObjectInstance).Dataset.Database as IFeatureDatabase;
                    if (fdb is IFeatureDatabaseReplication)
                    {
                        ReplicationUI.ShowCheckoutDialog((IFeatureClass)exObjectInstance);
                    }
                }
                _contextObject = null;
            }
        }

        async void _checkinMenuItem_Click(object sender, EventArgs e)
        {
            //if (_contextItem is ExplorerObjectListViewItem && listView.SelectedItems.Count == 1 && _contextItem == listView.SelectedItems[0])
            if (_contextObject != null)
            {
                IExplorerObject exObject = _contextObject; // _contextItem.ExplorerObject;

                var exObjectInstance = await exObject?.GetInstanceAsync();

                if (exObjectInstance is IFeatureClass &&
                    ((IFeatureClass)exObjectInstance).Dataset != null)
                {
                    IFeatureDatabase fdb = ((IFeatureClass)exObjectInstance).Dataset.Database as IFeatureDatabase;
                    if (fdb is IFeatureDatabaseReplication)
                    {
                        ReplicationUI.ShowCheckinDialog((IFeatureClass)exObjectInstance);
                    }
                }
                _contextObject = null;
            }
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem item = (ListViewItem)e.Item;

            if (listView.SelectedItems.Count > 1 && listView.SelectedItems.Contains(item))
            {
                List<IExplorerObjectSerialization> exObjects = new List<IExplorerObjectSerialization>();
                foreach (ListViewItem i in listView.SelectedItems)
                {
                    if (i is ExplorerObjectListViewItem && ((ExplorerObjectListViewItem)i).ExplorerObject != null)
                    {
                        IExplorerObjectSerialization ser = ExplorerObjectManager.SerializeExplorerObject(((ExplorerObjectListViewItem)i).ExplorerObject);
                        if (ser == null)
                        {
                            continue;
                        }

                        exObjects.Add(ser);
                    }
                }
                this.DoDragDrop(exObjects, DragDropEffects.Copy);
            }
            else
            {
                if (item is ExplorerObjectListViewItem && ((ExplorerObjectListViewItem)item).ExplorerObject != null)
                {
                    List<IExplorerObjectSerialization> exObjects = new List<IExplorerObjectSerialization>();
                    IExplorerObjectSerialization ser = ExplorerObjectManager.SerializeExplorerObject(((ExplorerObjectListViewItem)item).ExplorerObject);
                    if (ser != null)
                    {
                        exObjects.Add(ser);
                        this.DoDragDrop(exObjects, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this.SelectedExplorerObjects);
            }
            /*
if (listView.SelectedItems.Count == 0)
{

}
if (ItemSelected!=null) ItemSelected(
   listView.SelectedItemslistView.SelectedItems[0]
   );
* */
        }

        public bool MultiSelection
        {
            get { return listView.MultiSelect; }
            set { listView.MultiSelect = value; }
        }

        public void DeleteItem(IExplorerObject exObject)
        {
            foreach (ExplorerObjectListViewItem item in listView.Items)
            {
                if (ExObjectComparer.Equals(exObject, item.ExplorerObject))
                {
                    listView.Items.Remove(item);
                }
            }
        }
        public void RenameItem(IExplorerObject exObject)
        {
            /*
            if (pos >= 0 && pos < listView.Items.Count)
            {
                listView.Items[pos].Text = name;
            }
             * */
        }

        async private void toolStripMenuItemRefresh_Click(object sender, EventArgs e)
        {
            await this.RefreshContents();
        }

        #region RenameNode
        private class RenameTextBox : TextBox
        {
            private IExplorerObjectRenamable _exObject;

            public RenameTextBox(IExplorerObjectRenamable exObject)
            {
                _exObject = exObject;

                base.LostFocus += new EventHandler(RenameTextBox_LostFocus);
                base.KeyDown += new KeyEventHandler(RenameTextBox_KeyDown);
            }

            void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (!(sender is RenameTextBox))
                {
                    return;
                }

                if (e.KeyCode == Keys.Enter)
                {
                    this.LostFocus -= new EventHandler(RenameTextBox_LostFocus);

                    if (this.Parent != null)
                    {
                        this.Parent.Controls.Remove(sender as RenameTextBox);
                    }

                    RenameObject();
                }
            }

            void RenameTextBox_LostFocus(object sender, EventArgs e)
            {
                if (!(sender is RenameTextBox))
                {
                    return;
                }

                if (this.Parent != null)
                {
                    this.Parent.Controls.Remove(sender as RenameTextBox);
                }

                RenameObject();
            }

            public void RenameObject()
            {
                if (_exObject != null)
                {
                    _exObject.RenameExplorerObject(this.Text);
                }
            }
        }
        private void BeginRename()
        {
            if (listView.SelectedItems.Count == 1 &&
                listView.SelectedItems[0] is ExplorerObjectListViewItem)
            {
                ExplorerObjectListViewItem item = listView.SelectedItems[0] as ExplorerObjectListViewItem;
                if (item.ExplorerObject is IExplorerObjectRenamable)
                {
                    BeginRename(item);
                }
            }
        }
        private void BeginRename(ExplorerObjectListViewItem item)
        {
            if (item == null || !(item.ExplorerObject is IExplorerObjectRenamable))
            {
                return;
            }

            RenameTextBox box = new RenameTextBox(item.ExplorerObject as IExplorerObjectRenamable);
            box.Bounds = new Rectangle(16, item.Bounds.Y, item.Bounds.Width - 16, item.Bounds.Height);
            box.Text = item.Text;

            listView.Controls.Add(box);
            box.Visible = true;

            box.Focus();
            box.Select(0, item.Text.Length);
        }
        #endregion

        internal ListViewItem GetItemPerPath(string path)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (!(item is ExplorerObjectListViewItem))
                {
                    continue;
                }

                IExplorerObject exObject = ((ExplorerObjectListViewItem)item).ExplorerObject;
                if (exObject == null)
                {
                    continue;
                }

                if (exObject.FullName == path ||
                    exObject.FullName == path + @"\")
                {
                    return item;
                }
            }

            return null;
        }
    }

    internal class ExplorerObjectListViewItem : ListViewItem
    {
        private IExplorerObject _exObject = null;

        public ExplorerObjectListViewItem(string[] items, IExplorerObject exObject)
            : base(items)
        {
            _exObject = exObject;
        }

        public IExplorerObject ExplorerObject
        {
            get { return _exObject; }
        }
    }

    internal class CreateNewToolStripItem : ToolStripMenuItem
    {
        IExplorerObject _exObject;

        public CreateNewToolStripItem(IExplorerObject exObject)
        {
            _exObject = exObject;
            string name = String.IsNullOrEmpty(_exObject.Name) ? _exObject.Type : _exObject.Name;

            if (!name.Contains("..."))
            {
                base.Text = LocalizedResources.GetResString("ArgString.Create", "Create " + name, name);
            }
            else
            {
                base.Text = name;
            }

            if (_exObject.Icon != null && _exObject.Icon.Image != null)
            {
                base.Image = _exObject.Icon.Image;
            }
        }

        public IExplorerObject ExplorerObject
        {
            get { return _exObject; }
        }
    }
}

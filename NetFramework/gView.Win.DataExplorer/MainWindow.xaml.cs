using gView.Desktop.Wpf;
using gView.Desktop.Wpf.Controls;
using gView.Desktop.Wpf.Items;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Controls;
using gView.Win.DataExplorer.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Xceed.Wpf.AvalonDock.Layout;

namespace gView.Win.DataExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow, gView.Explorer.UI.Framework.UI.IFormExplorer
    {
        private gView.Framework.UI.IExplorerApplication _application;
        private gView.Framework.UI.Dialogs.FormCatalogTree _tree;
        private gView.Framework.UI.Controls.ContentsList _content;
        private IExplorerObject _exObject = null;
        private global::System.Windows.Forms.ToolStrip _toolStripAddress = null;
        private TabPages _tabPages = new TabPages();
        private IExplorerTabPage _activeTabPage = null;
        private List<IExplorerObject> _selected = new List<IExplorerObject>();
        private Fluent.RibbonGroupBox _createNewRibbonGroupBox = null;
        private Fluent.DropDownButton _favDropDownButton = null;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                _application = new ExplorerApplication(this);
                _application.DockWindowAdded += new DockWindowAddedEvent(_application_DockWindowAdded);
                _application.OnShowDockableWindow += new OnShowDockableWindowEvent(_application_OnShowDockableWindow);

                #region Windows Forms Control Disign

                _toolStripAddress = new System.Windows.Forms.ToolStrip();
                _toolStripAddress.Stretch = true;
                _toolStripAddress.GripMargin = new System.Windows.Forms.Padding(1);
                _toolStripAddress.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
                _toolStripAddress.BackColor = System.Drawing.Color.White;
                winFormsHostStripAddress.Child = _toolStripAddress;
                #endregion

                _tree = new gView.Framework.UI.Dialogs.FormCatalogTree(_application, false);
                _tree.NodeSelected += new gView.Framework.UI.Dialogs.FormCatalogTree.NodeClickedEvent(tree_NodeSelected);
                _tree.NodeRenamed += new gView.Framework.UI.Dialogs.FormCatalogTree.NodeRenamedEvent(tree_NodeRenamed);
                _tree.NodeDeleted += new gView.Framework.UI.Dialogs.FormCatalogTree.NodeDeletedEvent(tree_NodeDeleted);
                //winFormsHostExplorerTree.Child = _tree;

                PlugInManager compMan = new PlugInManager();
                foreach (var tabType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IExplorerTabPage))
                {
                    IExplorerTabPage page = compMan.CreateInstance<IExplorerTabPage>(tabType);
                    if (page == null || page.Control == null)
                    {
                        continue;
                    }

                    page.OnCreate(_application);

                    LayoutDocument layoutDoc = new LayoutDocument();
                    layoutDoc.Title = layoutDoc.ContentId = page.Title;
                    layoutDoc.CanClose = false;
                    layoutDoc.CanFloat = false;
                    layoutDoc.Content = new WindowsFormsHost();
                    ((WindowsFormsHost)layoutDoc.Content).Child = page.Control;
                    _tabPages.Add(new TabPage(page, layoutDoc));
                    if (page is gView.Framework.UI.Controls.ContentsList)
                    {
                        ((gView.Framework.UI.Controls.ContentsList)page).ItemSelected += new gView.Framework.UI.Controls.ContentsList.ItemClickedEvent(ContentsList_ItemSelected);
                        _content = (gView.Framework.UI.Controls.ContentsList)page;
                    }
                }
                explorerDocPane.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(explorerDocPane_PropertyChanged);

                anchorPaneRight.Children[0].Hide();
                _application.AddDockableWindow(_tree, DockWindowState.left);

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load("menu.explorer.xml");

                    MakeMainMenuBar(doc.SelectSingleNode("//Menubar"));
                }
                catch { }

                MakeRibbon();
                ValidateButtons();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        #region Loaded

        private void dockManager_Loaded(object sender, RoutedEventArgs e)
        {
            _tree.InitTree(true);

            foreach (var item in ribbon.QuickAccessItems)
            {
                ribbon.AddToQuickAccessToolBar(item.Target);
            }
        }

        private void toolBarNav_Loaded(object sender, RoutedEventArgs e)
        {
            var overflowGrid = toolBarNav.Template.FindName("OverflowGrid", toolBarNav) as FrameworkElement;

            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region DockWindow Events
        private Dictionary<IDockableWindow, LayoutAnchorable> _anchorables = new Dictionary<IDockableWindow, LayoutAnchorable>();
        void _application_OnShowDockableWindow(IDockableWindow window)
        {
            if (!_anchorables.ContainsKey(window))
            {
                return;
            }

            LayoutAnchorable anchorable = _anchorables[window];
            if (anchorable != null)
            {
                if (anchorable.IsAutoHidden)
                {
                    anchorable.ToggleAutoHide();
                }
                else
                {
                    LayoutContent content = anchorable;
                    while (content != null && content.Parent is ILayoutContentSelector)
                    {
                        ILayoutContentSelector selector = (ILayoutContentSelector)content.Parent;
                        int index = selector.IndexOf(content);
                        if (index != selector.SelectedContentIndex)
                        {
                            selector.SelectedContentIndex = index;
                        }

                        content = content.Parent as LayoutContent;
                    }

                    anchorable.Show();
                    if (anchorable.IsVisible == false)
                    {
                        //if (anchorable.CanFloat)
                        //    anchorable.Float();
                        //else
                        anchorable.Dock();
                    }
                }
            }
        }

        void _application_DockWindowAdded(IDockableWindow window, string parentDockableWindowName)
        {
            LayoutAnchorable anchorable = new LayoutAnchorable();
            anchorable.Title = String.IsNullOrEmpty(parentDockableWindowName) ? window.Name : parentDockableWindowName;

            if (window is global::System.Windows.Forms.Control)
            {
                WindowsFormsHost winHost = new WindowsFormsHost();
                winHost.Child = window as global::System.Windows.Forms.Control;
                anchorable.Content = winHost;
            }
            else
            {
                anchorable.Content = window;
            }
            _anchorables.Add(window, anchorable);

            switch (window.DockableWindowState)
            {
                case DockWindowState.child:
                case DockWindowState.right:
                    anchorPaneRight.Children.Add(anchorable);
                    break;
                default:
                    anchorPaneLeft.Children.Add(anchorable);
                    break;
            }


            WpfViewToolStripItem winItem = new WpfViewToolStripItem(window.Name, window.Image, window);
            winItem.Click += new RoutedEventHandler(ViewToolStripItem_Click);
        }
        #endregion

        #region TreeEvents

        async private Task tree_NodeSelected(global::System.Windows.Forms.TreeNode node)
        {
            if (_toolStripAddress == null)
            {
                return;
            }

            if (node is ExplorerObjectNode && ((ExplorerObjectNode)node).ExplorerObject != null)
            {
                RemovePathButtons();

                IExplorerObject pathObject = ((ExplorerObjectNode)node).ExplorerObject;
                while (pathObject != null)
                {
                    if (pathObject is IExplorerParentObject)
                    {
                        try
                        {
                            _toolStripAddress.Items.Insert(0, await SubPathParentToolStripItem.Create(this, (IExplorerParentObject)pathObject));
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                            MessageBox.Show(ex.Message);
                        }
                    }

                    SubPathToolStripItem item = new SubPathToolStripItem(pathObject);
                    item.Click += new EventHandler(SubPathItem_Click);
                    _toolStripAddress.Items.Insert(0, item);

                    pathObject = pathObject.ParentExplorerObject;
                }

                _selected.Clear();
                _selected.Add(_exObject = ((ExplorerObjectNode)node).ExplorerObject);

                await ViewTabPages(_exObject);

                if (_createNewRibbonGroupBox != null)
                {
                    RemoveCreateNewButtons();
                    PlugInManager compMan = new PlugInManager();
                    foreach (var compType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IExplorerObject))
                    {
                        IExplorerObject ex = compMan.CreateInstance<IExplorerObject>(compType);
                        if (ex is IExplorerObjectCreatable)
                        {
                            if (!((IExplorerObjectCreatable)ex).CanCreate(_exObject))
                            {
                                continue;
                            }

                            //if (_toolStripCreateNew.Items.Count == 0)
                            //{
                            //    _toolStripCreateNew.Items.Add(new System.Windows.Forms.ToolStripLabel(gView.Framework.Globalisation.LocalizedResources.GetResString("Create.New", "Create new") + ":"));
                            //}

                            CreateNewToolStripItem createNewItem = new CreateNewToolStripItem(ex);
                            createNewItem.Click += createNewItem_Click;
                            _createNewRibbonGroupBox.Items.Add(createNewItem);
                        }
                    }
                    if (_createNewRibbonGroupBox.Items.Count > 0)
                    {
                        _createNewRibbonGroupBox.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                RemovePathButtons();
                await ViewTabPages(null);
            }

            ValidateButtons();
        }

        private void tree_NodeDeleted(IExplorerObject exObject)
        {
            ValidateButtons();
        }

        private void tree_NodeRenamed(IExplorerObject exObject)
        {
            ValidateButtons();
        }
        #endregion

        #region Ribbon
        public void MakeRibbon()
        {
            PlugInManager pm = new PlugInManager();

            foreach (IExplorerRibbonTab exRibbonTab in OrderedPluginList<IExplorerRibbonTab>.Sort(
                pm.GetPlugins(gView.Framework.system.Plugins.Type.IExplorerRibbonTab).Select(p => pm.CreateInstance<IExplorerRibbonTab>(p))))
            {
                Fluent.RibbonTabItem tabItem = new Fluent.RibbonTabItem();
                tabItem.Header = exRibbonTab.Header;
                ribbon.Tabs.Add(tabItem);
                tabItem.DataContext = exRibbonTab;

                foreach (RibbonGroupBox exGroupBox in exRibbonTab.Groups)
                {
                    Fluent.RibbonGroupBox groupBox = new Fluent.RibbonGroupBox();
                    groupBox.Header = exGroupBox.Header;
                    tabItem.Groups.Add(groupBox);

                    foreach (RibbonItem cartoRibbonItem in exGroupBox.Items)
                    {
                        Guid toolGUID = cartoRibbonItem.Guid;
                        if (toolGUID == new Guid("00000000-0000-0000-0000-000000000000"))
                        {
                            groupBox.Items.Add(new Separator());
                            continue;
                        }

                        object tool = pm.CreateInstance(toolGUID);
                        if (tool == null)
                        {
                            continue;
                        }

                        #region IToolItem
                        if (tool is IToolItem)
                        {
                            if (((IToolItem)tool).ToolItem != null)
                            {
                                if (((IToolItem)tool).ToolItem is global::System.Windows.Forms.ToolStripItem)
                                {
                                    StackPanel panel = new StackPanel();
                                    panel.Margin = new Thickness(0, 32, 0, 0);

                                    global::System.Windows.Forms.ToolStripItem stripItem = ((IToolItem)tool).ToolItem;

                                    global::System.Windows.Forms.MenuStrip bar = new global::System.Windows.Forms.MenuStrip();
                                    bar.BackColor = global::System.Drawing.Color.Transparent; //.FromArgb(223, 234, 246);

                                    bar.Items.Add(stripItem);

                                    WindowsFormsHost host = new WindowsFormsHost();
                                    host.Background = new SolidColorBrush(Color.FromRgb(223, 234, 246));
                                    host.Child = bar;

                                    if (tool is IToolItemLabel)
                                    {
                                        IToolItemLabel label = (IToolItemLabel)tool;
                                        panel.Orientation = label.LabelPosition == ToolItemLabelPosition.top ||
                                                            label.LabelPosition == ToolItemLabelPosition.bottom ? Orientation.Vertical : Orientation.Horizontal;

                                        if (panel.Orientation == Orientation.Vertical)
                                        {
                                            panel.Margin = new Thickness(0, 13, 0, 0);
                                        }

                                        TextBlock text = new TextBlock();
                                        text.Text = label.Label;
                                        text.Padding = new Thickness(3);

                                        if (label.LabelPosition == ToolItemLabelPosition.top || label.LabelPosition == ToolItemLabelPosition.left)
                                        {
                                            panel.Children.Insert(0, text);
                                        }
                                        else
                                        {
                                            panel.Children.Add(text);
                                        }
                                    }

                                    panel.Children.Add(host);

                                    groupBox.Items.Add(panel);
                                }
                            }
                        }
                        #endregion

                        #region IToolMenu
                        else if (tool is IExToolMenu)
                        {
                            DropDownToolButton button = new DropDownToolButton(tool as IExToolMenu);
                            button.Click += new RoutedEventHandler(ToolButton_Click);
                            groupBox.Items.Add(button);
                        }
                        #endregion

                        #region ITool
                        else if (tool is IExTool)
                        {
                            ((IExTool)tool).OnCreate(_application);
                            ToolButton button = new ToolButton(tool as IExTool);
                            button.SizeDefinition = cartoRibbonItem.SizeDefinition;

                            groupBox.Items.Add(button);
                            button.Click += new RoutedEventHandler(ToolButton_Click);

                        }
                        #endregion
                    }
                }

                if (ribbon.Tabs.Count == 1)
                {
                    #region Favorites
                    Fluent.RibbonGroupBox favBox = new Fluent.RibbonGroupBox();
                    favBox.Header = String.Empty;
                    _favDropDownButton = new Fluent.DropDownButton();
                    _favDropDownButton.Header = "Favorites";
                    _favDropDownButton.Icon = _favDropDownButton.LargeIcon = ImageFactory.FromBitmap(global::gView.Win.DataExplorer.Properties.Resources.folder_heart);

                    Fluent.MenuItem add2fav = new Fluent.MenuItem();
                    add2fav.Header = "Add to favorites...";
                    add2fav.Icon = ImageFactory.FromBitmap(global::gView.Win.DataExplorer.Properties.Resources.folder_heart);
                    add2fav.Click += new RoutedEventHandler(Add2Favorites_Click);
                    _favDropDownButton.Items.Add(add2fav);

                    bool first = true;
                    foreach (MyFavorites.Favorite fav in (new MyFavorites().Favorites))
                    {
                        if (fav == null)
                        {
                            continue;
                        }

                        WpfFavoriteMenuItem fItem = new WpfFavoriteMenuItem(fav);
                        fItem.Click += new RoutedEventHandler(MenuItem_Favorite_Click);

                        if (first)
                        {
                            first = false;
                            _favDropDownButton.Items.Add(new Separator());
                        }
                        _favDropDownButton.Items.Add(fItem);
                    }

                    favBox.Items.Add(_favDropDownButton);
                    ribbon.Tabs[0].Groups.Add(favBox);
                    #endregion

                    _createNewRibbonGroupBox = new Fluent.RibbonGroupBox();
                    _createNewRibbonGroupBox.Header = "Create New";
                    _createNewRibbonGroupBox.Visibility = Visibility.Visible;
                    _createNewRibbonGroupBox.Background = new SolidColorBrush(Colors.GreenYellow);

                    ribbon.Tabs[0].Groups.Add(_createNewRibbonGroupBox);
                }
            }

            #region Options
            Fluent.RibbonTabItem optionsTab = new Fluent.RibbonTabItem() { Header = "Options" };
            Fluent.RibbonGroupBox optionsBox = new Fluent.RibbonGroupBox() { Header = String.Empty };
            optionsTab.Groups.Add(optionsBox);

            foreach (var pageType in pm.GetPlugins(gView.Framework.system.Plugins.Type.IExplorerOptionPage))
            {
                IExplorerOptionPage page = pm.CreateInstance<IExplorerOptionPage>(pageType);
                if (page == null)
                {
                    continue;
                }

                OptionsButton button = new OptionsButton(page);
                button.Click += new RoutedEventHandler(OptoinButton_Click);
                optionsBox.Items.Add(button);
            }
            ribbon.Tabs.Add(optionsTab);
            #endregion
        }

        void OptoinButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsButton button = (OptionsButton)sender;
            IExplorerOptionPage page = button.ExplorerOptionPage;

            if (page != null)
            {
                FormOptions dlg = new FormOptions(page);
                dlg.ShowDialog();
            }
        }

        #endregion

        #region Events UI
        internal void SubPathItem_Click(object sender, EventArgs e)
        {
            if (_tree != null && sender is SubPathToolStripItem)
            {
                SubPathToolStripItem item = (SubPathToolStripItem)sender;

                this.Cursor = global::System.Windows.Input.Cursors.Wait;
                try
                {
                    if (!_tree.MoveToNode(item.SubPath))
                    {
                        global::System.Windows.MessageBox.Show("Can't move to '" + item.SubPath + "'", "Error");
                    }
                }
                catch (Exception ex)
                {
                    global::System.Windows.MessageBox.Show(ex.Message, "Error");
                }
                this.Cursor = global::System.Windows.Input.Cursors.Arrow;
            }
        }

        private void ContentsList_ItemSelected(List<IExplorerObject> node)
        {
            _selected.Clear();
            if (node == null)
            {
                return;
            }

            foreach (IExplorerObject exObject in node)
            {
                _selected.Add(exObject);
            }
            if (_selected.Count == 0 && _exObject != null)
            {
                _selected.Add(_exObject);
            }
        }

        async private void addNetworkDirectory_Click(object sender, EventArgs e)
        {
            gView.Framework.UI.Dialogs.FormAddNetworkDirectory dlg = new gView.Framework.UI.Dialogs.FormAddNetworkDirectory();
            if (dlg.ShowDialog() == global::System.Windows.Forms.DialogResult.OK && !String.IsNullOrEmpty(dlg.Path))
            {
                ConfigConnections connStream = new ConfigConnections("directories");
                connStream.Add(dlg.Path, dlg.Path);

                _tree.SelectRootNode();
                await RefreshContents();
            }
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            if (_application != null)
            {
                _application.RefreshContents();
            }
        }

        private LayoutDocument _selectedLayoutDoc = null;
        async void explorerDocPane_PropertyChanged(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedContent")
            {
                if (explorerDocPane.SelectedContent is LayoutDocument && explorerDocPane.SelectedContent != _selectedLayoutDoc)
                {
                    _selectedLayoutDoc = (LayoutDocument)explorerDocPane.SelectedContent;
                    if (_selectedLayoutDoc != null)
                    {
                        IExplorerTabPage selectedPage = _tabPages.ExplorerTabPageByLayoutDocument(_selectedLayoutDoc);
                        if (selectedPage != null && selectedPage != _activeTabPage)
                        {
                            _activeTabPage = selectedPage;
                            if (_selected.Count == 1)
                            {
                                this.Cursor = global::System.Windows.Input.Cursors.Wait;
                                await _activeTabPage.SetExplorerObjectAsync(_selected[0]);
                                await _activeTabPage.OnShow();
                                this.Cursor = global::System.Windows.Input.Cursors.Arrow;
                            }
                        }
                    }
                }
            }
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            IExTool tool = null;
            if (sender is ToolButton)
            {
                tool = ((ToolButton)sender).Tool;
            }
            else if (sender is DropDownToolButton)
            {
                tool = ((DropDownToolButton)sender).Tool;
            }
            else if (sender is WpfToolMenuItem)
            {
                tool = ((WpfToolMenuItem)sender).Tool;
            }
            else if (sender is ToolMenuItem)
            {
                tool = ((ToolMenuItem)sender).Tool;
            }

            if (tool == null)
            {
                return;
            }

            if (tool is IToolWindow)
            {
                _application.AddDockableWindow(((IToolWindow)tool).ToolWindow, "");
                _application.ShowDockableWindow(((IToolWindow)tool).ToolWindow);
            }
            tool.OnEvent(null);

            ValidateButtons();
        }

        async public void createNewItem_Click(object sender, EventArgs e)
        {
            if (!(sender is CreateNewToolStripItem) || _content == null)
            {
                return;
            }

            IExplorerObject exObject = ((CreateNewToolStripItem)sender).ExplorerObject;
            await _content.CreateNewItem(exObject);

            ValidateButtons();
        }

        private void MenuItem_Plugins_Click(object sender, RoutedEventArgs e)
        {
            gView.Framework.UI.Dialogs.FormComponents dlg = new gView.Framework.UI.Dialogs.FormComponents();
            dlg.ShowDialog();
        }

        private void MenuItem_Options_Click(object sender, RoutedEventArgs e)
        {
            gView.Framework.UI.Dialogs.FormExplorerOptions dlg = new gView.Framework.UI.Dialogs.FormExplorerOptions(null);
            dlg.ShowDialog();
        }

        private void Add2Favorites_Click(object sender, RoutedEventArgs e)
        {
            if (_tree == null)
            {
                return;
            }

            IExplorerObject exObject = _tree.SelectedExplorerObject;
            if (exObject == null)
            {
                return;
            }

            gView.Framework.UI.Dialogs.FormAddToFavorites dlg = new gView.Framework.UI.Dialogs.FormAddToFavorites(exObject.FullName, false);
            if (dlg.ShowDialog() == global::System.Windows.Forms.DialogResult.OK)
            {
                MyFavorites favs = new MyFavorites();

                favs.AddFavorite(dlg.FavoriteName, exObject.FullName, (exObject.Icon != null) ? exObject.Icon.Image : null);

                WpfFavoriteMenuItem fItem = new WpfFavoriteMenuItem(new MyFavorites.Favorite(dlg.FavoriteName, dlg.FavoritePath, (exObject.Icon != null) ? exObject.Icon.Image : null));
                fItem.Click += new RoutedEventHandler(MenuItem_Favorite_Click);

                _favDropDownButton.Items.Add(fItem);
            }
        }

        private void MenuItem_Favorite_Click(object sender, EventArgs e)
        {
            if (_tree != null && sender is WpfFavoriteMenuItem)
            {
                WpfFavoriteMenuItem item = (WpfFavoriteMenuItem)sender;

                this.Cursor = Cursors.Wait;
                try
                {
                    if (!this.MoveToTreeNode(item.Favorite.Path))
                    {
                        MessageBox.Show("Can't move to '" + item.Favorite.Path + "'", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ViewToolStripItem_Click(object sender, EventArgs e)
        {
            if (!(sender is WpfViewToolStripItem))
            {
                return;
            }

            _application_OnShowDockableWindow(((WpfViewToolStripItem)sender).DockableToolWindow);
        }
        #endregion

        #region Members UI
        private void RemovePathButtons()
        {
            if (_toolStripAddress == null)
            {
                return;
            }

            for (int i = 0; i < _toolStripAddress.Items.Count; i++)
            {
                if (_toolStripAddress.Items[i] is SubPathToolStripItem ||
                    _toolStripAddress.Items[i] is SubPathParentToolStripItem)
                {
                    _toolStripAddress.Items.Remove(_toolStripAddress.Items[i]);
                    i--;
                }
            }
        }

        private void RemoveCreateNewButtons()
        {
            if (_createNewRibbonGroupBox != null)
            {
                _createNewRibbonGroupBox.Visibility = Visibility.Hidden;
                _createNewRibbonGroupBox.Items.Clear();
            }
        }

        async private Task ViewTabPages(IExplorerObject exObject)
        {
            explorerDocPane.PropertyChanged -= new global::System.ComponentModel.PropertyChangedEventHandler(explorerDocPane_PropertyChanged);
            List<IOrder> pages = new List<IOrder>();
            foreach (TabPage tabPage in _tabPages)
            {
                IExplorerTabPage exTabPage = tabPage.ExTabPage;
                if (exTabPage == null || exTabPage.Control == null)
                {
                    continue;
                }

                if (await exTabPage.ShowWith(exObject))
                {
                    pages.Add(exTabPage);
                }
            }
            pages.Sort(new SortByIOrder());
            explorerDocPane.Children.Clear();

            foreach (IOrder page in pages)
            {
                foreach (TabPage tabPage in _tabPages)
                {
                    if (tabPage.ExTabPage == page)
                    {
                        explorerDocPane.Children.Add(tabPage.LayoutDoc);
                        break;
                    }
                }

            }

            explorerDocPane.SelectedContentIndex = 0;
            if (explorerDocPane.SelectedContent is LayoutDocument)
            {
                IExplorerTabPage selectedPage = ((WindowsFormsHost)explorerDocPane.SelectedContent.Content).Child as IExplorerTabPage;
                //if (selectedPage != _activeTabPage)
                {
                    _activeTabPage = selectedPage;
                    _selectedLayoutDoc = explorerDocPane.SelectedContent as LayoutDocument;
                    if (_selected.Count == 1)
                    {
                        this.Cursor = global::System.Windows.Input.Cursors.Wait;
                        this.Refresh();
                        await _activeTabPage.SetExplorerObjectAsync(_selected[0]);
                        await _activeTabPage.OnShow();
                        this.Cursor = global::System.Windows.Input.Cursors.Arrow;
                        this.Refresh();
                    }
                }
            }
            explorerDocPane.PropertyChanged += new global::System.ComponentModel.PropertyChangedEventHandler(explorerDocPane_PropertyChanged);
        }

        private IExplorerTabPage ActiveTabPage
        {
            get
            {
                return _activeTabPage;
            }
        }

        private void MakeMainMenuBar(XmlNode node)
        {
            if (node == null)
            {
                return;
            }

            backstageTabControl.Items.Clear();

            PlugInManager compManager = new PlugInManager();
            foreach (XmlNode menu in node.SelectNodes("Menu[@name]"))
            {
                if (menu.Attributes["name"].Value.ToLower() == "file")
                {
                    foreach (XmlNode menuItem in menu.SelectNodes("MenuItem"))
                    {
                        if (menuItem.Attributes["guid"] == null)
                        {
                            //backstageTabControl.Items.Add(new Separator());
                        }
                        else
                        {
                            IExTool tool = (IExTool)compManager.CreateInstance(new Guid(menuItem.Attributes["guid"].Value));
                            if (tool == null)
                            {
                                continue;
                            }

                            tool.OnCreate(_application);

                            if (tool is IToolControl)
                            {
                                Fluent.BackstageTabItem backItem = new Fluent.BackstageTabItem();
                                backItem.Header = tool.Name;

                                object control = ((IToolControl)tool).Control;
                                if (control is global::System.Windows.Forms.Control)
                                {
                                    WindowsFormsHost host = WinHostFactory.ToWindowsHost((global::System.Windows.Forms.Control)control);
                                    backItem.Content = host;
                                }
                                else if (control is FrameworkElement)
                                {
                                    backItem.Content = control;
                                }

                                backstageTabControl.Items.Add(backItem);
                            }
                            else
                            {
                                Items.ToolButton button = new Items.ToolButton(tool);
                                button.Click += new RoutedEventHandler(ToolButton_Click);

                                backstageTabControl.Items.Add(button);
                            }
                        }
                    }
                }

                if (menu.Attributes["name"].Value.ToLower() == "quickaccess")
                {
                    foreach (XmlNode menuItem in menu.SelectNodes("MenuItem"))
                    {
                        IExTool tool = compManager.CreateInstance(new Guid(menuItem.Attributes["guid"].Value)) as IExTool;
                        if (tool == null)
                        {
                            continue;
                        }

                        tool.OnCreate(_application);

                        ToolButton button = new ToolButton(tool);
                        button.Click += new RoutedEventHandler(ToolButton_Click);

                        ribbon.QuickAccessItems.Add(new Fluent.QuickAccessMenuItem() { Target = button });
                    }
                }
            }
        }
        #endregion

        #region Item Classes

        #region Forms
        public class SubPathToolStripItem : global::System.Windows.Forms.ToolStripMenuItem
        {
            private string _subPath;

            public SubPathToolStripItem(IExplorerObject exObject)
            {

                if (exObject != null)
                {
                    base.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;

                    _subPath = exObject.FullName;
                    base.Text = exObject.Name;
                    if (exObject.Icon != null)
                    {
                        base.Image = exObject.Icon.Image;
                    }
                }
                base.BackColor = global::System.Drawing.Color.White;

                //base.Size = new Size(base.Size.Width, 14);
            }

            public string SubPath
            {
                get { return _subPath; }
            }
        }

        public class SubPathParentToolStripItem : global::System.Windows.Forms.ToolStripDropDownButton
        {
            private SubPathParentToolStripItem()
            {

            }

            async static public Task<SubPathParentToolStripItem> Create(MainWindow window, IExplorerParentObject exObject)
            {
                var item = new SubPathParentToolStripItem();
                if (exObject != null)
                {
                    item.DisplayStyle = global::System.Windows.Forms.ToolStripItemDisplayStyle.Image;
                    item.Image = gView.Win.DataExplorer.Properties.Resources.pfeil_r_s;
                    item.ImageScaling = global::System.Windows.Forms.ToolStripItemImageScaling.None;
                    item.ShowDropDownArrow = false;

                    if (await exObject.ChildObjects() != null)
                    {
                        foreach (IExplorerObject childObject in await exObject.ChildObjects())
                        {
                            SubPathToolStripItem child = new SubPathToolStripItem(childObject);
                            child.Click += new EventHandler(window.SubPathItem_Click);
                            item.DropDownItems.Add(child);

                        }
                    }
                }
                item.BackColor = global::System.Drawing.Color.White;

                //base.Size = new Size(base.Size.Width, 14);

                return item;
            }
        }

        private class ToolMenuItem : global::System.Windows.Forms.ToolStripMenuItem
        {
            private IExTool _tool;

            public ToolMenuItem(IExTool tool)
                : base(tool.Name, (global::System.Drawing.Image)tool.Image)
            {
                _tool = tool;

                if (_tool is IShortCut)
                {
                    base.ShortcutKeys = ((IShortCut)_tool).ShortcutKeys;
                    base.ShortcutKeyDisplayString = ((IShortCut)_tool).ShortcutKeyDisplayString;
                    base.ShowShortcutKeys = true;
                }
            }

            public IExTool Tool
            {
                get { return _tool; }
            }

            public override bool Enabled
            {
                get
                {
                    if (_tool != null)
                    {
                        return _tool.Enabled;
                    }

                    return base.Enabled;
                }
                set
                {
                    base.Enabled = value;
                }
            }
        }

        private interface ICheckAbleButton
        {
            bool Checked { get; set; }
        }
        #endregion

        #region Wpf
        private class CreateNewToolStripItem : Fluent.Button
        {
            IExplorerObject _exObject;

            public CreateNewToolStripItem(IExplorerObject exObject)
                : base()
            {
                _exObject = exObject;
                string name = String.IsNullOrEmpty(_exObject.Name) ? _exObject.Type : _exObject.Name;

                base.Header = name;
                if (exObject.Icon != null)
                {
                    base.Icon = base.LargeIcon = ImageFactory.FromBitmap(exObject.Icon.Image);
                }
            }

            public IExplorerObject ExplorerObject
            {
                get { return _exObject; }
            }
        }

        private class WpfFavoriteMenuItem : Fluent.MenuItem
        {
            MyFavorites.Favorite _fav;

            public WpfFavoriteMenuItem(MyFavorites.Favorite fav)
            {
                _fav = fav;
                if (_fav != null)
                {
                    base.Header = _fav.Name;
                    if (fav.Image == null)
                    {
                        base.Icon = ImageFactory.FromBitmap(global::gView.Win.DataExplorer.Properties.Resources.folder_go);
                    }
                    else
                    {
                        base.Icon = ImageFactory.FromBitmap(fav.Image);
                    }
                }
            }

            public MyFavorites.Favorite Favorite
            {
                get { return _fav; }
            }
        }


        #endregion

        private class TabPage
        {
            public IExplorerTabPage ExTabPage { get; private set; }
            public LayoutDocument LayoutDoc { get; private set; }

            public TabPage(IExplorerTabPage exTabPage, LayoutDocument layoutDoc)
            {
                ExTabPage = exTabPage;
                LayoutDoc = layoutDoc;
            }
        }

        private class TabPages : List<TabPage>
        {
            public IExplorerTabPage ExplorerTabPageByLayoutDocument(LayoutDocument layoutDoc)
            {
                foreach (TabPage page in this)
                {
                    if (page.LayoutDoc == layoutDoc)
                    {
                        return page.ExTabPage;
                    }
                }
                return null;
            }
        }

        internal class OptionsButton : Fluent.Button
        {
            public OptionsButton(IExplorerOptionPage page)
            {
                this.ExplorerOptionPage = page;

                base.Header = page.Title;
                base.Icon = base.LargeIcon = ImageFactory.FromBitmap(
                    page.Image == null ? global::gView.Win.DataExplorer.Properties.Resources.options : page.Image as global::System.Drawing.Image
                    );
            }

            public IExplorerOptionPage ExplorerOptionPage { get; private set; }
        }
        #endregion

        #region IFormExplorer
        private delegate void ValidateButtonsCallback();
        public void ValidateButtons()
        {
            if (!ribbon.Dispatcher.CheckAccess())
            {
                ribbon.Dispatcher.Invoke(
                    global::System.Windows.Threading.DispatcherPriority.Normal,
                    new ValidateButtonsCallback(ValidateButtons));
            }
            else
            {
                foreach (Fluent.RibbonTabItem tabItem in ribbon.Tabs)
                {
                    foreach (Fluent.RibbonGroupBox groupBox in tabItem.Groups)
                    {
                        foreach (object button in groupBox.Items)
                        {
                            if (button is ToolButton && ((ToolButton)button).Tool != null)
                            {
                                IExTool tool = ((ToolButton)button).Tool;
                                ((ToolButton)button).IsEnabled = tool.Enabled;
                            }
                            else if (button is DropDownToolButton && ((DropDownToolButton)button).Tool != null)
                            {
                                IExTool tool = ((DropDownToolButton)button).Tool;
                                ((DropDownToolButton)button).IsEnabled = tool.Enabled;
                            }
                        }
                    }
                }
            }
        }

        public bool InvokeRequired
        {
            get
            {
                return _tree != null ? _tree.InvokeRequired : false;
            }
        }

        public object Invoke(Delegate method)
        {
            return _tree.Invoke(method);
        }

        public void SetCursor(object cursor)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    this.SetCursor(cursor);
                });
            }
            else
            {
                this.Cursor = gView.Desktop.Wpf.CursorFactory.ToWpfCursor(cursor);
                var focused = global::System.Windows.Input.FocusManager.GetFocusedElement(this);
                if (focused is WindowsFormsHost && ((WindowsFormsHost)focused).Child != null)
                {
                    ((WindowsFormsHost)focused).Child.Cursor = gView.Desktop.Wpf.CursorFactory.ToFormsCursor(cursor);
                }
            }
        }

        public void AppendContextMenuItems(global::System.Windows.Forms.ContextMenuStrip strip, object context)
        {
            PlugInManager compMan = new PlugInManager();

            List<IOrder> items = new List<IOrder>();
            foreach (var toolType in compMan.GetPlugins(gView.Framework.system.Plugins.Type.IExTool))
            {
                IContextMenuItem item = compMan.TryCreateInstance<IContextMenuItem>(toolType);
                if (item == null || !item.ShowWith(context) || !(item is IExTool))
                {
                    continue;
                } ((IExTool)item).OnCreate(_application);
                items.Add(item);
            }

            items.Sort(new SortByIOrder());

            int l = -1;
            foreach (IContextMenuItem item in items)
            {
                if (Math.Abs(l - item.SortOrder / 10) != 0)
                {
                    l = item.SortOrder / 10;
                    if (strip.Items.Count > 0)
                    {
                        strip.Items.Add(new global::System.Windows.Forms.ToolStripSeparator());
                    }
                }
                ToolMenuItem toolItem = new ToolMenuItem(item as IExTool);
                toolItem.Click += new EventHandler(ToolButton_Click);
                strip.Items.Add(toolItem);
            }
        }

        async public Task<bool> RefreshContents()
        {
            if (_activeTabPage != null)
            {
                await _activeTabPage.RefreshContents();
            }

            await _tree.RefreshSelectedNode();

            return true;
        }

        public gView.Framework.UI.Dialogs.FormCatalogTree CatalogTree
        {
            get { return _tree; }
        }

        public string Text
        {
            get; set;
        }

        public List<IExplorerObject> SelectedObjects
        {
            get { return ListOperations<IExplorerObject>.Clone(_selected); }
        }

        private delegate void SetStatusbarTextCallback(string text);
        public void SetStatusbarText(string text)
        {
            if (!statusBarLabel1.Dispatcher.CheckAccess())
            {
                statusBarLabel1.Dispatcher.Invoke(
                    global::System.Windows.Threading.DispatcherPriority.Normal,
                    new SetStatusbarTextCallback(SetStatusbarText), new object[] { text });
            }
            else
            {
                statusBarLabel1.Text = String.IsNullOrEmpty(text) ? "Ready" : text;
                statusBarLabel1.Refresh();
            }
        }

        private delegate void SetStatusbarProgressVisibilityCallback(bool vis);
        public void SetStatusbarProgressVisibility(bool vis)
        {
            if (!statusbarProgress.Dispatcher.CheckAccess())
            {
                statusbarProgress.Dispatcher.Invoke(
                    global::System.Windows.Threading.DispatcherPriority.Normal,
                    new SetStatusbarProgressVisibilityCallback(SetStatusbarProgressVisibility), new object[] { vis });
            }
            else
            {
                statusbarProgress.Visibility = (vis == true ? Visibility.Visible : Visibility.Collapsed);
                statusbarProgress.Refresh();
            }
        }

        private delegate void SetStatusbarProgressValueCallback(int val);
        public void SetStatusbarProgressValue(int val)
        {
            if (!statusbarProgress.Dispatcher.CheckAccess())
            {
                statusbarProgress.Dispatcher.Invoke(
                    global::System.Windows.Threading.DispatcherPriority.Normal,
                    new SetStatusbarProgressValueCallback(SetStatusbarProgressValue), new object[] { val });
            }
            else
            {
                statusbarProgress.Value = val;
                statusbarProgress.Refresh();
            }
        }

        public void RefreshStatusbar()
        {

        }

        public bool MoveToTreeNode(string path)
        {
            return _tree.MoveToNode(path);
        }
        #endregion

        #region IWin32Window Member

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new System.Windows.Interop.WindowInteropHelper(this);
                return interopHelper.Handle;
            }
        }

        #endregion
    }
}

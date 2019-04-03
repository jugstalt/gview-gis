using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Microsoft.Win32;
using System.Data;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Globalisation;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.UI.Dialogs;

namespace gView.Framework.UI.Controls
{
    internal partial class ExplorerDialogControl : UserControl
    {
        public delegate void ItemClickedEvent(List<IExplorerObject> exObject);
        public event ItemClickedEvent ItemSelected = null;
        internal delegate void ElementTextStatusChangedEvent(TextBox tb);
        public event ElementTextStatusChangedEvent ElementTextStatusChanged=null;

        private List<Filter.ExplorerDialogFilter> _filters = null;
        private List<IExplorerObject> _selectedObjects = new List<IExplorerObject>();

        private bool _open = true;

        public ExplorerDialogControl()
        {
            InitializeComponent();

            contentsList1.View = View.List;
            contentsList1.AllowContextMenu = false;

            using (Graphics graphics = this.CreateGraphics())
            {
                this.FontScaleFactor = graphics.DpiX / 96f;
            }
        }

        private float FontScaleFactor { get; set; }

        public List<Filter.ExplorerDialogFilter> Filters
        {
            get { return _filters; }
            set
            {
                _filters = value;

                cmbFilters.Items.Clear();
                if (_filters == null) return;

                foreach (ExplorerDialogFilter filter in _filters)
                {
                    cmbFilters.Items.Add(filter);
                }
                if (cmbFilters.Items.Count > 0) cmbFilters.SelectedIndex = 0;
            }
        }

        private void ExplorerDialogControl_Load(object sender, EventArgs e)
        {
            contentsList1.ItemSelected += new ContentsList.ItemClickedEvent(contentsList1_ItemSelected);
            contentsList1.ItemDoubleClicked += new ContentsList.ItemDoubleClickedEvent(contentsList1_ItemDoubleClicked);
            contentsList1.SmallImageList = ExplorerImageList.List.ImageList;
            catalogComboBox1.InitComboBox();

            LocalizedResources.GlobalizeMenuItem(btnFavorites);
            bool first = true;
            foreach (MyFavorites.Favorite fav in (new MyFavorites()).Favorites)
            {
                if (fav == null) continue;
                FavoriteMenuItem fItem = new FavoriteMenuItem(fav);
                fItem.Click += new EventHandler(fItem_Click);

                if (first)
                {
                    first = false;
                    btnFavorites.DropDownItems.Add(new ToolStripSeparator());
                }
                btnFavorites.DropDownItems.Add(fItem);
            }
        }

        public List<IExplorerObject> ExplorerObjects
        {
            get
            {
                if (_open)
                {
                    return _selectedObjects;
                }
                else
                {
                    if (catalogComboBox1.SelectedExplorerObject == null) return null;
                    List<IExplorerObject> selected = new List<IExplorerObject>();
                    selected.Add(catalogComboBox1.SelectedExplorerObject);
                    return selected;
                }
            }
        }
        public string TargetName
        {
            get { return txtElement.Text; }
        }
        public ExplorerDialogFilter SelectedExplorerDialogFilter
        {
            get { return contentsList1.Filter; }
        }

        public bool MultiSelection
        {
            get { return contentsList1.MultiSelection; }
            set { contentsList1.MultiSelection = value; }
        }

        public bool IsOpenDialog
        {
            get { return _open; }
            set
            {
                contentsList1.IsOpenDialog = _open = value;
                if (!_open) MultiSelection = false;
            }
        }

        void contentsList1_ItemSelected(List<IExplorerObject> exObjects)
        {
            _selectedObjects.Clear();
            if (_open)
            {
                txtElement.Text = "";

                foreach (IExplorerObject exObject in exObjects)
                {
                    if (contentsList1.Filter != null && contentsList1.Filter.Match(exObject))
                    {
                        _selectedObjects.Add(exObject);

                        if (txtElement.Text != "") txtElement.Text += ";";
                        txtElement.Text += exObject.Name;
                    }
                }
            }
            else
            {
                
            }

            if (ItemSelected != null) ItemSelected(_selectedObjects);
        }

        void contentsList1_ItemDoubleClicked(ListViewItem node)
        {
            if (node is ExplorerObjectListViewItem)
            {
                ExplorerObjectListViewItem n = (ExplorerObjectListViewItem)node;
                if (n.ExplorerObject is IExplorerParentObject)
                {
                    //contentsList1.ExplorerObject = n.ExplorerObject;
                    if (_open)
                    {
                        if (contentsList1.Filter != null && (!contentsList1.Filter.Match(n.ExplorerObject) || 
                            contentsList1.Filter.BrowseAll ||
                            n.ExplorerObject is DirectoryObject   // Bei Directory immer weiter hineinbrowsen
                            ))
                        {
                            Cursor = Cursors.WaitCursor;
                            catalogComboBox1.AddChildNode(n.ExplorerObject);
                            Cursor = Cursors.Default;
                            txtElement.Text = "";
                        }
                    }
                    else
                    {
                        Cursor = Cursors.WaitCursor;
                        catalogComboBox1.AddChildNode(n.ExplorerObject);
                        Cursor = Cursors.Default;

                        txtElement.Enabled = contentsList1.Filter != null && contentsList1.Filter.Match(n.ExplorerObject);
                        if (ElementTextStatusChanged != null) ElementTextStatusChanged(txtElement);
                    }

                    StoreLastPath(n.ExplorerObject);
                }
            }
        }

        private void StoreLastPath(IExplorerObject exObject)
        {
            if (!(exObject is ISerializableExplorerObject)) return;

            try
            {
                //RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\gViewGisOS\ExplorerDialog", true);
                //if (key == null) key = Registry.CurrentUser.CreateSubKey(@"Software\gViewGisOS\ExplorerDialog");
                
                //key.SetValue("path", exObject.FullName);
                //key.SetValue("guid", exObject.GUID);
                //key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmbFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            contentsList1.Filter = (ExplorerDialogFilter)cmbFilters.SelectedItem;
            Cursor = Cursors.WaitCursor;
            contentsList1.RefreshContents();
            contentsList1.ExplorerObject = contentsList1.ExplorerObject;
            Cursor = Cursors.Default;

            SetElementTextBoxVisibility();
        }

        private void catalogComboBox1_SelectedItemChanged(CatalogComboItem item)
        {
            if (contentsList1.ShowWith(item.ExplorerObject))
            {
                contentsList1.ExplorerObject = item.ExplorerObject;
            }
            else
            {
                contentsList1.ExplorerObject = null;
            }

            SetElementTextBoxVisibility();

            if (ItemSelected != null)
            {
                _selectedObjects.Clear();
                ItemSelected(_selectedObjects);
            }
        }

        private void SetElementTextBoxVisibility()
        {
            bool changed = false;
            if (!_open)
            {
                //if (contentsList1.Filter is ExplorerSaveDialogFilter && ((ExplorerSaveDialogFilter)contentsList1.Filter).MatchParent(catalogComboBox1.SelectedExplorerObject))
                if(contentsList1.Filter!=null && contentsList1.Filter.Match(catalogComboBox1.SelectedExplorerObject))
                {
                    if (txtElement.Enabled == false) changed = true;
                    txtElement.Enabled = true;
                }
                else
                {
                    if (txtElement.Enabled == true) changed = true;
                    txtElement.Enabled = false;
                }
            }
            else
            {
                if (txtElement.Enabled == false) changed = true;
                txtElement.Enabled = true;
            }
            if (changed && ElementTextStatusChanged != null) ElementTextStatusChanged(txtElement);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            catalogComboBox1.MoveUp();

            if (!_open)
            {
                txtElement.Enabled = contentsList1.Filter.Match(catalogComboBox1.SelectedExplorerObject);
                if (ElementTextStatusChanged != null) ElementTextStatusChanged(txtElement);
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            contentsList1.View = View.Details;
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            contentsList1.View = View.List;
        }

        private void txtElement_TextChanged(object sender, EventArgs e)
        {
            if (ElementTextStatusChanged != null) ElementTextStatusChanged(txtElement);
        }

        private void toolAddToFavorites_Click(object sender, EventArgs e)
        {
            IExplorerObject selected = catalogComboBox1.SelectedExplorerObject;
            if (selected == null) return;

            FormAddToFavorites dlg = new FormAddToFavorites(selected.FullName,false);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                MyFavorites favs = new MyFavorites();

                favs.AddFavorite(dlg.FavoriteName, selected.FullName, (selected.Icon != null) ? selected.Icon.Image : null);

                FavoriteMenuItem fItem = new FavoriteMenuItem(new MyFavorites.Favorite(dlg.FavoriteName, dlg.FavoritePath, (selected.Icon != null) ? selected.Icon.Image : null));
                fItem.Click += new EventHandler(fItem_Click);

                if (btnFavorites.DropDownItems.Count < 2)
                    btnFavorites.DropDownItems.Add(new ToolStripSeparator());

                btnFavorites.DropDownItems.Add(fItem);
            }
        }

        void fItem_Click(object sender, EventArgs e)
        {
            if (sender is FavoriteMenuItem)
            {
                FavoriteMenuItem item = (FavoriteMenuItem)sender;

                this.Cursor = Cursors.WaitCursor;

                catalogComboBox1.InitComboBox();
                try
                {
                    StringBuilder fullPath = new StringBuilder();
                    foreach (string subPath in item.Favorite.Path.Split('\\'))
                    {
                        if (fullPath.Length > 0) fullPath.Append(@"\");
                        fullPath.Append(subPath);

                        ListViewItem listItem = contentsList1.GetItemPerPath(fullPath.ToString());
                        if (item == null) return;

                        contentsList1_ItemDoubleClicked(listItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
                this.Cursor = Cursors.Default;
            }
        }
    }
}

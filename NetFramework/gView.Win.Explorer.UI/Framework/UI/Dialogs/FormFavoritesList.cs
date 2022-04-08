using gView.Framework.Globalisation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormFavoritesList : UserControl, IDockableWindow
    {
        private FormCatalogTree _tree;

        public FormFavoritesList(FormCatalogTree tree)
        {
            InitializeComponent();

            this.Name = LocalizedResources.GetResString("MenuHeader.Favorites", "Favorites");
            _tree = tree;
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
            get { return global::gView.Win.Explorer.UI.Properties.Resources.folder_heart; }
        }

        #endregion

        private void FormFavoritesList_Load(object sender, EventArgs e)
        {
            BuildList();
        }

        public void BuildList()
        {
            lstFavorites.Items.Clear();

            lstFavorites.Items.Add(new ListViewItem(LocalizedResources.GetResString("Menu.AddToFavorites", "Add To Favorites..."), 1));

            foreach (MyFavorites.Favorite fav in (new MyFavorites()).Favorites)
            {
                if (fav == null)
                {
                    continue;
                }

                FavoriteItem item = new FavoriteItem(fav);
                if (fav.Image != null)
                {
                    imageList1.Images.Add(fav.Image);
                    item.ImageIndex = imageList1.Images.Count - 1;
                }

                lstFavorites.Items.Add(item);
            }
        }

        #region ItemClasses
        private class FavoriteItem : ListViewItem
        {
            private MyFavorites.Favorite _fav;

            public FavoriteItem(MyFavorites.Favorite fav)
            {
                _fav = fav;

                this.Text = fav.Name;
            }

            public MyFavorites.Favorite Favorite
            {
                get { return _fav; }
            }
        }
        #endregion

        private FavoriteItem _contextItem = null;
        private void lstFavorites_MouseDown(object sender, MouseEventArgs e)
        {
            _contextItem = null;
            ListViewItem item = lstFavorites.GetItemAt(e.X, e.Y);
            if (item == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (item is FavoriteItem)
                {
                    _tree.MoveToNode(((FavoriteItem)item).Favorite.Path);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (item is FavoriteItem)
                {
                    _contextItem = (FavoriteItem)item;
                    mnStripFavorite.Show(this, e.X, e.Y);
                }
            }
        }

        private void lstFavorites_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = lstFavorites.GetItemAt(e.X, e.Y);
            if (item == null)
            {
                return;
            }

            if (item.ImageIndex == 1 && _tree != null)
            {
                IExplorerObject selected = _tree.SelectedExplorerObject;
                if (selected == null)
                {
                    return;
                }

                FormAddToFavorites dlg = new FormAddToFavorites(selected.FullName, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    MyFavorites favs = new MyFavorites();

                    favs.AddFavorite(dlg.FavoriteName, selected.FullName, (selected.Icon != null) ? selected.Icon.Image : null);

                    BuildList();
                }
            }
        }

        private void mnRemove_Click(object sender, EventArgs e)
        {
            if (_contextItem != null)
            {
                if (new MyFavorites().RemoveFavorite(_contextItem.Favorite.Name, _contextItem.Favorite.Path))
                {
                    BuildList();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormAddToFavorites : Form
    {
        private string _fullPath;

        public FormAddToFavorites(string fullPath, bool enablePath)
        {
            InitializeComponent();

            _fullPath = fullPath;
            txtFavPath.Text = _fullPath;

            if (_fullPath.Contains(@"\"))
            {
                txtFavName.Text = _fullPath.Substring(
                    _fullPath.LastIndexOf(@"\") + 1, _fullPath.Length - _fullPath.LastIndexOf(@"\") - 1);
            }
            else
            {
                txtFavName.Text = _fullPath;
            }
            txtFavPath.Enabled = enablePath;
        }

        public string FavoritePath
        {
            get { return txtFavPath.Text; }
        }
        public string FavoriteName
        {
            get { return txtFavName.Text; }
        }

        private void FormAddToFavorites_Load(object sender, EventArgs e)
        {
            txtFavName.Select();
        }
    }
}
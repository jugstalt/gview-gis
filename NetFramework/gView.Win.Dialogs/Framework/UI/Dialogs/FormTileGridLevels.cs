using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormTileGridLevels : Form
    {
        private int _maxLevel;
        private List<int> _levels;
        private double _tileX, _tileY, _resX, _resY;
        private TileLevelType _type;

        public FormTileGridLevels(int maxLevel, List<int> levels, double tileX, double tileY, double resX, double resY, TileLevelType type)
        {
            InitializeComponent();

            _maxLevel = maxLevel;
            _levels = levels;
            _tileX = tileX;
            _tileY = tileY;
            _resX = resX;
            _resY = resY;
            _type = type;
        }

        private void FormTileGridLevels_Load(object sender, EventArgs e)
        {
            _levels.Sort();

            for (int i = 0; i < _maxLevel; i++)
            {
                string tileSize = _tileX.ToString() + " / " + _tileY.ToString();
                string res = _resX.ToString() + " / " + _resY.ToString();
                string imgSize = ((int)(_tileX / _resX)).ToString() + " / " + ((int)(_tileY / _resY)).ToString();

                ListViewItem item = new ListViewItem(new string[] { i.ToString(), tileSize, res, imgSize });
                item.Checked = _levels.Contains(i);
                lstLevels.Items.Add(item);

                if (_type == TileLevelType.ConstantImagesize)
                {
                    _tileX *= 2.0; _tileY *= 2.0;
                    _resX *= 2.0; _resY *= 2.0;
                }
                else if (_type == TileLevelType.ConstantGeographicTilesize)
                {
                    _resX *= 2.0; _resY *= 2.0;
                }
            }
        }

        public List<int> SelectedLevels
        {
            get
            {
                List<int> selected = new List<int>();
                for (int i = 0; i < lstLevels.Items.Count; i++)
                {
                    ListViewItem item = lstLevels.Items[i];
                    if (item.Checked)
                        selected.Add(i);
                }
                return selected;
            }
        }
    }
}
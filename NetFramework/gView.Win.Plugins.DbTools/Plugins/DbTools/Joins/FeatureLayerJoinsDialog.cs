using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.UI;

namespace gView.Plugins.DbTools.Joins
{
    public partial class FeatureLayerJoinsDialog : Form
    {
        private IFeatureLayer _layer;
        private IMapDocument _mapDocument;

        public FeatureLayerJoinsDialog(IMapDocument mapDocument, IFeatureLayer layer)
        {
            InitializeComponent();

            _mapDocument = mapDocument;
            _layer = layer;
            FillList();
        }

        #region Gui
        private void FillList()
        {
            lstJoins.Items.Clear();
            if (_layer.Joins != null)
                foreach (IFeatureLayerJoin join in _layer.Joins)
                {
                    lstJoins.Items.Add(new FeatureLayerJoinItem() { FeatureLayerJoin = join });
                }

            btnEdit.Enabled = btnRemove.Enabled = false;
        }
        #endregion

        #region Events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddJoinDialog dlg = new AddJoinDialog(_mapDocument, _layer);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IFeatureLayerJoin join = dlg.FeatureLayerJoin;
                if (join != null)
                {
                    if (_layer.Joins == null)
                        _layer.Joins = new Framework.Data.FeatureLayerJoins();
                    _layer.Joins.Add(join);
                    _layer.FirePropertyChanged();

                    FillList();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!(lstJoins.SelectedItem is FeatureLayerJoinItem))
                return;

            IFeatureLayerJoin join = ((FeatureLayerJoinItem)lstJoins.SelectedItem).FeatureLayerJoin;

            AddJoinDialog dlg = new AddJoinDialog(_mapDocument, _layer);
            dlg.FeatureLayerJoin = join;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IFeatureLayerJoin newJoin = dlg.FeatureLayerJoin;
                if (newJoin != null)
                {
                    _layer.Joins.Remove(join);
                    _layer.Joins.Add(newJoin);
                    _layer.FirePropertyChanged();

                    FillList();
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!(lstJoins.SelectedItem is FeatureLayerJoinItem))
                return;

            IFeatureLayerJoin join = ((FeatureLayerJoinItem)lstJoins.SelectedItem).FeatureLayerJoin;

            if (join != null && _layer.Joins != null)
            {
                _layer.Joins.Remove(join);
                if (_layer.Joins.Count == 0)
                    _layer.Joins = null;
                _layer.FirePropertyChanged();
            }
            FillList();
        }

        private void lstJoins_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = btnRemove.Enabled = (lstJoins.SelectedIndex >= 0);
        }
        #endregion

        #region Item Classes

        private class FeatureLayerJoinItem
        {
            public IFeatureLayerJoin FeatureLayerJoin { get; set; }

            public override string ToString()
            {
                if (FeatureLayerJoin == null)
                    return base.ToString();

                return FeatureLayerJoin.JoinName;
            }
        }

        #endregion
    }
}

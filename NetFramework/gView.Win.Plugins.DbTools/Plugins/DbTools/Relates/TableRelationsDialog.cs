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

namespace gView.Plugins.DbTools.Relates
{
    public partial class TableRelationsDialog : Form
    {
        private IFeatureLayer _layer;
        private IMapDocument _mapDocument;

        public TableRelationsDialog(IMapDocument mapDocument, IFeatureLayer layer)
        {
            InitializeComponent();

            _mapDocument = mapDocument;
            _layer = layer;
            FillList();
        }

        #region Gui

        private void FillList()
        {
            lstRelates.Items.Clear();
            if (_mapDocument.TableRelations != null)
                foreach (ITableRelation relation in _mapDocument.TableRelations)
                {
                    lstRelates.Items.Add(new TableRelationItem() { TableRelation = relation });
                }

            btnEdit.Enabled = btnRemove.Enabled = false;
        }

        #endregion

        #region Events

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddTableRelationDialog dlg = new AddTableRelationDialog(_mapDocument, _layer);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ITableRelation relation = dlg.TableRelation;
                if (relation != null && relation.LeftTable != null && relation.RightTable != null)
                {
                    _mapDocument.TableRelations.Add(relation);

                    relation.LeftTable.FirePropertyChanged();
                    relation.RightTable.FirePropertyChanged();

                    FillList();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!(lstRelates.SelectedItem is TableRelationItem))
                return;

            ITableRelation relation = ((TableRelationItem)lstRelates.SelectedItem).TableRelation;

            AddTableRelationDialog dlg = new AddTableRelationDialog(_mapDocument, _layer);
            dlg.TableRelation = relation;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ITableRelation newRelation = dlg.TableRelation;
                if (newRelation != null && newRelation.LeftTable != null && newRelation.RightTable != null)
                {
                    _mapDocument.TableRelations.Remove(relation);
                    _mapDocument.TableRelations.Add(newRelation);

                    if (relation.LeftTable != null && relation.RightTable != null)
                    {
                        relation.LeftTable.FirePropertyChanged();
                        relation.RightTable.FirePropertyChanged();
                    }

                    newRelation.LeftTable.FirePropertyChanged();
                    newRelation.RightTable.FirePropertyChanged();

                    FillList();
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!(lstRelates.SelectedItem is TableRelationItem))
                return;

            ITableRelation relation = ((TableRelationItem)lstRelates.SelectedItem).TableRelation;

            if (relation != null)
            {
                _mapDocument.TableRelations.Remove(relation);
                if (relation.LeftTable != null)
                    relation.LeftTable.FirePropertyChanged();
                if (relation.RightTable != null)
                    relation.RightTable.FirePropertyChanged();
            }
            FillList();
        }

        private void lstRelates_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = btnRemove.Enabled = (lstRelates.SelectedIndex >= 0);
        }

        #endregion

        #region Item Classes

        private class TableRelationItem
        {
            public ITableRelation TableRelation { get; set; }

            public override string ToString()
            {
                if (TableRelation == null)
                    return base.ToString();

                return TableRelation.RelationName;
            }
        }

        #endregion
    }
}

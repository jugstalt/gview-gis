using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Db;
using gView.Framework.Db.UI;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Data.Joins;
using gView.Framework.system;

namespace gView.Plugins.DbTools.Joins
{
    public partial class AddJoinDialog : Form
    {
        private IMapDocument _mapDocument;

        public AddJoinDialog(IMapDocument mapDocument, IFeatureLayer layer)
        {
            _mapDocument = mapDocument;

            InitializeComponent();

            foreach (IField field in layer.Fields.ToEnumerable())
            {
                cmbFeatureLayerField.Items.Add(field.name);
            }

            PlugInManager pm = new PlugInManager();
            foreach (IFeatureLayerJoin join in pm.GetPluginInstances(typeof(IFeatureLayerJoin)))
            {
                if (join == null) continue;
                cmbJoinClasses.Items.Add(new FeatureLayerJoinItem() { FeatureLayerJoin = join });
            }
            cmbJoinClasses.SelectedIndex = 0;
        }

        #region Events

        private void AddJoinDialog_Load(object sender, EventArgs e)
        {
        }

        private void cmbJoinMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelJoinTypeUI.Controls.Clear();
            if (!(cmbJoinClasses.SelectedItem is FeatureLayerJoinItem))
                return;

            IFeatureLayerJoin join = ((FeatureLayerJoinItem)cmbJoinClasses.SelectedItem).FeatureLayerJoin;
            if (join is IPropertyPage)
            {
                object ctrl = ((IPropertyPage)join).PropertyPage(_mapDocument);
                if (ctrl is Control)
                {
                    ((Control)ctrl).Dock = DockStyle.Fill;
                    panelJoinTypeUI.Controls.Add((Control)ctrl);
                }
            }
        }

        #endregion

        #region Properties
        public string JoinName
        {
            get { return txtName.Text; }
            set { txtName.Text = value; }
        }

        public string FeatureLayerField
        {
            get { return cmbFeatureLayerField.Text; }
            set { cmbFeatureLayerField.Text = value; }
        }

        public joinType JoinType
        {
            get { return btnInnerJoin.Checked ? joinType.LeftInnerJoin : joinType.LeftOuterJoin; }
            set
            {
                switch (value)
                {
                    case joinType.LeftOuterJoin:
                        btnOuterJoin.Checked = true;
                        btnInnerJoin.Checked = false;
                        break;
                    case joinType.LeftInnerJoin:
                        btnOuterJoin.Checked = false;
                        btnInnerJoin.Checked = true;
                        break;
                }
            }
        }

        public IFeatureLayerJoin FeatureLayerJoin
        {
            get
            {
                IFeatureLayerJoin join = ((FeatureLayerJoinItem)cmbJoinClasses.SelectedItem).FeatureLayerJoin;

                if (join != null)
                {
                    join.JoinName = this.JoinName;
                    join.Field = this.FeatureLayerField;
                    join.JoinType = this.JoinType;

                    if (panelJoinTypeUI.Controls.Count > 0 && panelJoinTypeUI.Controls[0] is IPage)
                    {
                        ((IPage)panelJoinTypeUI.Controls[0]).Commit();
                    }
                }
                return join;
            }
            set
            {
                foreach (FeatureLayerJoinItem item in cmbJoinClasses.Items)
                {
                    if (item.FeatureLayerJoin.GetType().Equals(value.GetType()))
                    {
                        item.FeatureLayerJoin = value;
                        cmbJoinClasses.SelectedIndex = -1;
                        cmbJoinClasses.SelectedItem = item;

                        this.JoinName = item.FeatureLayerJoin.JoinName;
                        this.FeatureLayerField = item.FeatureLayerJoin.Field;
                        this.JoinType = item.FeatureLayerJoin.JoinType;
                        break;
                    }
                }

                //if (value is FeatureLayerDatabaseJoin)
                //{
                //    cmbJoinClasses.SelectedIndex = 0;

                //    FeatureLayerDatabaseJoin dbJoin = (FeatureLayerDatabaseJoin)value;

                //    this.JoinName = dbJoin.Name;
                //    this.FeatureLayerField = dbJoin.Field;
                //    gView.Framework.Db.DbConnectionString connStr = new Framework.Db.DbConnectionString();
                //    connStr.UseProviderInConnectionString = true;
                //    connStr.FromString(dbJoin.JoinConnectionString);
                //    //this.JoinDbConnectionString = connStr;
                //    //this.JoinTableName = dbJoin.JoinTable;
                //    //this.JoinTableField = dbJoin.JoinField;
                //    this.JoinType = dbJoin.JoinType;
                //}
                //else if (value is FeatureLayerJoin)
                //{
                //    cmbJoinClasses.SelectedIndex = 1;

                //    FeatureLayerJoin join = (FeatureLayerJoin)value;

                //    this.JoinName = join.Name;
                //    this.FeatureLayerField = join.Field;
                //    if (join.FeatureLayer != null)
                //    {
                //        //foreach (DatasetElementItem item in cmbJoinedLayer.Items)
                //        //{
                //        //    if (item.DatasetElement.ID == join.FeatureLayer.ID)
                //        //    {
                //        //        cmbJoinedLayer.SelectedItem = item;
                //        //        break;
                //        //    }
                //        //}
                //    }
                //    //cmbJoinedFeatureLayerJoinField.SelectedItem = join.JoinField;
                //    this.JoinType = join.JoinType;
                //}
                //else
                //{
                //    throw new ArgumentException();
                //}
            }
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

                return FeatureLayerJoin.Name;
            }
        }

        #endregion
    }
}
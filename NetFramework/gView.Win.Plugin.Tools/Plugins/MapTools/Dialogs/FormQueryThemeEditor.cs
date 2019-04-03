using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.UI.Dialogs;

namespace gView.Plugins.MapTools.Dialogs
{
    internal partial class FormQueryThemeEditor : Form
    {
        private IMapDocument _doc = null;

        public FormQueryThemeEditor(IMapDocument doc)
        {
            if (doc == null) return;
            _doc = doc;

            InitializeComponent();

            tabControlLayers.Dock = DockStyle.Fill;
            dgQueries.Dock = DockStyle.Fill;
        }

        private void btnAddQuery_Click(object sender, EventArgs e)
        {
            QueryTheme node = new QueryTheme("Query" + (treeQueries.Nodes.Count + 1).ToString());
            treeQueries.Nodes.Add(node);
        }

        private void btnAddSeperator_Click(object sender, EventArgs e)
        {
            QueryTheme node = new QueryTheme();
            treeQueries.Nodes.Add(node);
        }

        private QueryThemeTable _actTableNode = null;
        private void StoreFieldDefs()
        {
            if (_actTableNode == null) return;

            QueryTheme theme = _actTableNode.Parent as QueryTheme;
            if (theme == null || theme.PromptDef == null) return;


            DataTable table = _actTableNode.QueryFieldDef;
            table.Rows.Clear();

            for (int i = 0; i < dgLayers.Rows.Count - 1; i++)
            {
                DataGridViewRow dgRow = dgLayers.Rows[i];

                DataRow[] rows = theme.PromptDef.Select("Prompt='" + dgRow.Cells[1].Value + "'");
                if (rows.Length == 0) continue;

                table.Rows.Add(
                    new object[] { dgRow.Cells[0].Value, rows[0]["ID"], dgRow.Cells[2].Value });
            }

            IFeatureLayer fLayer = _actTableNode.GetLayer(_doc) as IFeatureLayer;
            if (fLayer != null && fLayer.Fields != null && _actTableNode.VisibleFieldDef != null)
            {
                _actTableNode.VisibleFieldDef.UseDefault = chkUseStandard.Checked;
                _actTableNode.VisibleFieldDef.PrimaryDisplayField = cmbPrimaryField.Text;

                table = _actTableNode.VisibleFieldDef;
                table.Rows.Clear();
                foreach (IField field in fLayer.Fields.ToEnumerable())
                {
                    for (int i = 0; i < dgVisibleFields.Rows.Count; i++)
                    {
                        if ((string)dgVisibleFields.Rows[i].Cells[1].Value == field.name &&
                            ((string)dgVisibleFields.Rows[i].Cells[2].Value != field.aliasname ||
                             (bool)dgVisibleFields.Rows[i].Cells[0].Value == true))
                        {
                            table.Rows.Add(
                                new object[] { (bool)dgVisibleFields.Rows[i].Cells[0].Value, field.name, (string)dgVisibleFields.Rows[i].Cells[2].Value });
                        }
                    }
                }
            }
            _actTableNode = null;
        }

        private void treeQueries_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnAddTable.Enabled = treeQueries.SelectedNode is QueryTheme && ((QueryTheme)treeQueries.SelectedNode).Type == QueryTheme.NodeType.query;
            tabControlLayers.Visible = treeQueries.SelectedNode is QueryThemeTable;
            dgQueries.Visible = treeQueries.SelectedNode is QueryTheme;

            TreeNode node = treeQueries.SelectedNode;
            if (node is QueryTheme)
            {
                StoreFieldDefs();
                DataTable tab = ((QueryTheme)node).PromptDef;

                dgQueries.DataSource = tab;
            }
            else if (node is QueryThemeTable)
            {
                #region Query Fields
                StoreFieldDefs();
                dgLayers.Rows.Clear();

                IFeatureLayer layer = ((QueryThemeTable)node).GetLayer(_doc) as IFeatureLayer;
                if (layer != null)
                {
                    colField.Items.Clear();
                    if (layer.Fields != null)
                    {
                        foreach (IField field in layer.Fields.ToEnumerable())
                        {
                            colField.Items.Add(field.name);
                        }
                    }
                }
                QueryTheme theme = node.Parent as QueryTheme;
                if (theme == null && theme.PromptDef == null) return;

                colPrompt.Items.Clear();
                foreach (DataRow row in theme.PromptDef.Select())
                {
                    colPrompt.Items.Add(row["Prompt"].ToString());
                }

                foreach (DataRow row in ((QueryThemeTable)node).QueryFieldDef.Rows)
                {
                    try
                    {
                        string field = row["Field"].ToString();
                        string op = row["Operator"].ToString();
                        int promptID = (int)row["Prompt"];

                        if (colField.Items.IndexOf(field) == -1) field = "";
                        if (colOperator.Items.IndexOf(op) == -1) op = "";
                        DataRow[] rows = theme.PromptDef.Select("ID=" + promptID);
                        if (rows.Length == 0) continue;

                        dgLayers.Rows.Add(
                            new object[] { field, rows[0]["Prompt"], op });
                    }
                    catch { }
                }
                #endregion

                #region Visible Fields
                if (((QueryThemeTable)node).VisibleFieldDef != null && layer != null && layer.Fields!=null)
                {
                    QueryThemeVisibleFieldDef visFields = ((QueryThemeTable)node).VisibleFieldDef;

                    chkUseStandard.Checked = visFields.UseDefault;

                    cmbPrimaryField.Items.Clear();
                    dgVisibleFields.Rows.Clear();

                    foreach (IField field in layer.Fields.ToEnumerable())
                    {
                        cmbPrimaryField.Items.Add(field.name);
                        if (field.name == visFields.PrimaryDisplayField ||
                            (visFields.PrimaryDisplayField == "" &&
                            layer.Fields.PrimaryDisplayField!=null &&
                            field.name == layer.Fields.PrimaryDisplayField.name))
                        {
                            cmbPrimaryField.SelectedIndex = cmbPrimaryField.Items.Count - 1;
                        }

                        DataRow[] r = visFields.Select("Name='" + field.name + "'");
                        if (r.Length == 1)
                        {
                            dgVisibleFields.Rows.Add(new object[] { (bool)r[0]["Visible"], field.name, (string)r[0]["Alias"], field.type.ToString() });
                        }
                        else
                        {
                            dgVisibleFields.Rows.Add(new object[] { false, field.name, field.aliasname, field.type.ToString() });
                        }
                    }
                }
                #endregion

                _actTableNode = node as QueryThemeTable;

            }
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            QueryTheme parent = treeQueries.SelectedNode as QueryTheme;
            if (parent == null) return;

            FormSelectLayer selLayer = new FormSelectLayer(_doc);
            if (selLayer.ShowDialog() == DialogResult.OK)
            {
                if (selLayer.SelectedLayer == null) return;

                QueryThemeTable node = new QueryThemeTable(selLayer.SelectedLayer, selLayer.SelectedLayerAlias);
                parent.Nodes.Add(node);
                parent.Expand();
            }
        }

        private void treeQueries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            QueryTheme node = treeQueries.GetNodeAt(e.X, e.Y) as QueryTheme;
            if (node != null && node.Type == QueryTheme.NodeType.query)
            {
                node.Expand();
                node.BeginEdit();
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            TreeNode node = treeQueries.SelectedNode;
            if (node == null) return;

            TreeNodeCollection collection = (node.Parent != null) ? node.Parent.Nodes : treeQueries.Nodes;
            int index = collection.IndexOf(node);
            if (index < 1) return;

            collection.Remove(node);
            collection.Insert(index - 1, node);
            treeQueries.SelectedNode = node;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            TreeNode node = treeQueries.SelectedNode;
            if (node == null) return;

            TreeNodeCollection collection = (node.Parent != null) ? node.Parent.Nodes : treeQueries.Nodes;
            int index = collection.IndexOf(node);
            if (index < 0 || index >= collection.Count - 1) return;

            collection.Remove(node);
            collection.Insert(index + 1, node);
            treeQueries.SelectedNode = node;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            TreeNode node = treeQueries.SelectedNode;
            if (node == null) return;

            TreeNodeCollection collection = (node.Parent != null) ? node.Parent.Nodes : treeQueries.Nodes;

            if (MessageBox.Show("Remove selected item?", "Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                collection.Remove(node);
            }
        }

        private QueryThemes _queryThemes = null;
        public QueryThemes QueryThemes
        {
            get
            {
                return _queryThemes;
            }
            set
            {
                treeQueries.Nodes.Clear();
                if (value == null) return;

                foreach (QueryTheme query in value.Queries)
                {
                    treeQueries.Nodes.Add(query);
                }
                _queryThemes = new QueryThemes(treeQueries.Nodes);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            StoreFieldDefs();
            _queryThemes = new QueryThemes(treeQueries.Nodes);
        }

        private void FormQueryThemeEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            treeQueries.Nodes.Clear();
        }

        private void chkUseStandard_CheckedChanged(object sender, EventArgs e)
        {
            dgVisibleFields.Visible = groupPrimaryField.Visible =
                btnSelectAll.Visible = btnClearAll.Visible = !chkUseStandard.Checked;
        }

    }

    internal class QueryThemes : gView.Framework.system.Cloner, IPersistable
    {
        List<QueryTheme> _queries;

        public QueryThemes()
        {
            _queries = new List<QueryTheme>();
            
        }
        public QueryThemes(TreeNodeCollection collection)
            : this()
        {
            if (collection == null) return;

            foreach (TreeNode node in collection)
            {
                if (!(node is QueryTheme)) continue;
                _queries.Add(node as QueryTheme);
            }
        }

        public List<QueryTheme> Queries
        {
            get
            {
                return _queries;
            }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_queries == null) _queries = new List<QueryTheme>();

            _queries.Clear();
            while (true)
            {
                QueryTheme theme = stream.Load("Query", null, new QueryTheme()) as QueryTheme;
                if (theme == null) break;

                _queries.Add(theme);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_queries == null) return;

            foreach (QueryTheme theme in _queries)
            {
                stream.Save("Query", theme);
            }
        }

        #endregion
    }

    internal class QueryTheme : TreeNode, IPersistable
    {
        public enum NodeType { query=0, seperator=1 }
        private NodeType _type;
        QueryThemePromptDef _table;

        public QueryTheme()
        {
            base.Text = "Seperator";
            base.ImageIndex = base.SelectedImageIndex = 1;
            _type = NodeType.seperator;
        }
        public QueryTheme(string name)
        {
            base.Text = name;
            base.ImageIndex = base.SelectedImageIndex = 0;
            _type = NodeType.query;

            _table = new QueryThemePromptDef();
        }

        public NodeType Type
        {
            get { return _type; }
        }

        public DataTable PromptDef
        {
            get { return _table; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _type = (NodeType)stream.Load("Type", (int)NodeType.query);

            if (_type == NodeType.query)
            {
                base.Text = (string)stream.Load("Name", "");

                _table = stream.Load("PromptDefs", null, new QueryThemePromptDef()) as QueryThemePromptDef;
                if (_table == null) _table = new QueryThemePromptDef();

                base.ImageIndex = base.SelectedImageIndex = 0;
                while (true)
                {
                    QueryThemeTable tabNode = stream.Load("Table", null, new QueryThemeTable()) as QueryThemeTable;
                    if (tabNode == null) break;

                    Nodes.Add(tabNode);
                }
            }
            else
            {
                base.ImageIndex = base.SelectedImageIndex = 1;
            }

        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Type", (int)_type);

            if (_type == NodeType.query)
            {
                stream.Save("Name", base.Text);
                stream.Save("PromptDefs", _table);

                foreach (TreeNode node in Nodes)
                {
                    if (!(node is QueryThemeTable)) continue;
                    stream.Save("Table", node);
                }
            }
        }

        #endregion
    }

    internal class QueryThemePromptDef : DataTable, IPersistable
    {
        public QueryThemePromptDef()
            : base()
        {
            DataColumn ID = this.Columns.Add("ID", typeof(int));
            ID.Unique = true;
            ID.AutoIncrement = true;
            this.Columns.Add("Prompt", typeof(string));
            this.Columns.Add("Obliging", typeof(bool)).DefaultValue = false;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            Rows.Clear();

            while (true)
            {
                DataRow row = NewRow();
                QueryThemeTableRowPersist per = stream.Load("PromptDef", null, new QueryThemeTableRowPersist(row)) as QueryThemeTableRowPersist;
                if (per == null) break;

                Rows.Add(row);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (DataRow row in Rows)
            {
                QueryThemeTableRowPersist per = new QueryThemeTableRowPersist(row);
                stream.Save("PromptDef", per);
            }
        }

        #endregion
    }

    internal class QueryThemeTable : TreeNode,IPersistable
    {
        int _layerID = -1;
        int _datasetID = -1;
        QueryThemeQueryFieldDef _queryFields;
        QueryThemeVisibleFieldDef _visFields;
 
        internal QueryThemeTable()
        {
            base.ImageIndex = base.SelectedImageIndex = 2;
        }
        public QueryThemeTable(ILayer layer, string name) : this()
        {
            if (layer == null) return;

            base.Text = name;

            _layerID = layer.ID;
            _datasetID = layer.DatasetID;
            _queryFields = new QueryThemeQueryFieldDef();
            _visFields = new QueryThemeVisibleFieldDef();
        }

        public int LayerID
        {
            get
            {
                return _layerID;
            }
        }
        public int DatasetID
        {
            get { return _datasetID; }
        }

        public ILayer GetLayer(IMapDocument doc)
        {
            if (doc == null) return null;

            foreach (IMap map in doc.Maps)
            {
                foreach (IDatasetElement element in map.MapElements)
                {
                    if (element is ILayer && element.ID == _layerID && element.DatasetID == _datasetID)
                        return element as ILayer;
                }
            }
            return null;
        }

        public QueryThemeQueryFieldDef QueryFieldDef
        {
            get { return _queryFields; }
        }
        public QueryThemeVisibleFieldDef VisibleFieldDef
        {
            get { return _visFields; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            base.Text = (string)stream.Load("Text","");
            _layerID = (int)stream.Load("LayerID", -1);
            _datasetID = (int)stream.Load("DatasetID", -1);

            _queryFields = stream.Load("FieldDefs", null, new QueryThemeQueryFieldDef()) as QueryThemeQueryFieldDef;
            if (_queryFields == null) _queryFields = new QueryThemeQueryFieldDef();
            _visFields = stream.Load("VisibleFieldDefs", null, new QueryThemeVisibleFieldDef()) as QueryThemeVisibleFieldDef;
            if (_visFields == null) _visFields = new QueryThemeVisibleFieldDef();
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Text", base.Text);
            stream.Save("LayerID", _layerID);
            stream.Save("DatasetID", _datasetID);

            stream.Save("FieldDefs", _queryFields);
            stream.Save("VisibleFieldDefs", _visFields);
        }

        #endregion
    }

    internal class QueryThemeQueryFieldDef : DataTable, IPersistable
    {
        public QueryThemeQueryFieldDef()
            : base()
        {
            this.Columns.Add("Field", typeof(string));
            this.Columns.Add("Prompt", typeof(int));
            this.Columns.Add("Operator", typeof(string));
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            Rows.Clear();

            while (true)
            {
                DataRow row = NewRow();
                QueryThemeTableRowPersist per = stream.Load("FieldDef", null, new QueryThemeTableRowPersist(row)) as QueryThemeTableRowPersist;
                if (per == null) break;

                Rows.Add(row);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (DataRow row in Rows)
            {
                QueryThemeTableRowPersist per = new QueryThemeTableRowPersist(row);
                stream.Save("FieldDef", per);
            }
        }

        #endregion
    }

    internal class QueryThemeVisibleFieldDef : DataTable,IPersistable
    {
        private string _primaryField = "";
        private bool _useDefault = true;

        public QueryThemeVisibleFieldDef()
        {
            this.Columns.Add("Visible", typeof(bool));
            this.Columns.Add("Name", typeof(string));
            this.Columns.Add("Alias", typeof(string));
        }

        public bool UseDefault
        {
            get { return _useDefault; }
            set { _useDefault = value; }
        }
        public string PrimaryDisplayField
        {
            get { return _primaryField; }
            set { _primaryField = value; }
        }
        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _useDefault = (bool)stream.Load("UseDefault", true);
            _primaryField = (string)stream.Load("PrimaryField", "");

            Rows.Clear();

            while (true)
            {
                DataRow row = NewRow();
                QueryThemeTableRowPersist per = stream.Load("FieldDef", null, new QueryThemeTableRowPersist(row)) as QueryThemeTableRowPersist;
                if (per == null) break;

                Rows.Add(row);
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("UseDefault", _useDefault);
            stream.Save("PrimaryField", _primaryField);
            foreach (DataRow row in Rows)
            {
                QueryThemeTableRowPersist per = new QueryThemeTableRowPersist(row);
                stream.Save("FieldDef", per);
            }
        }

        #endregion
    }

    internal class QueryThemeTableRowPersist : IPersistable
    {
        private DataRow _row;
        public QueryThemeTableRowPersist(DataRow row)
        {
            _row = row;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_row == null || _row.Table == null || _row.Table.Columns == null) return;

            foreach (DataColumn col in _row.Table.Columns)
            {
                try
                {
                    _row[col.ColumnName] = stream.Load(col.ColumnName);
                }
                catch { }
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_row == null || _row.Table == null || _row.Table.Columns == null) return;

            foreach (DataColumn col in _row.Table.Columns)
            {
                stream.Save(col.ColumnName, _row[col.ColumnName]);
            }
        }

        #endregion
    }
}
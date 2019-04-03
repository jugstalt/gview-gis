using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Carto.Rendering;
using gView.Framework.UI.Controls;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormChartWizard : Form
    {
        private IDisplay _display;
        private IFeatureLayer _layer;
        private gvChart _gvChart;
        private List<Series> _series = new List<Series>();

        public FormChartWizard(IDisplay display, IFeatureLayer layer)
        {
            _display = display;
            _layer = layer;

            InitializeComponent();

            _gvChart = new gvChart(chartPreview);
            chartPreview.Series.Clear();
            chartPreview.ChartAreas.Clear();

            cmbDisplayMode.SelectedIndex = (int)gvChart.DisplayMode.EverySerieInTheSameChartArea;
        }

        private void FormChartWizard_Load(object sender, EventArgs e)
        {
            #region Tab Chart Type

            foreach (gvChart.gvChartType chartType in gvChart.ChartTypes)
            {
                lstChartTypes.Items.Add(chartType);
            }
            lstChartTypes.SelectedItem = 0;

            Series ser1 = new Series("Serie 1");
            ser1.Points.AddXY("A", 10);
            ser1.Points.AddXY("B", 6);
            ser1.Points.AddXY("C", 8);
            ser1.Points.AddXY("D", 12);
            ser1.Points.AddXY("E", 3);
            ser1.Color = Color.Red;
            ser1.ChartArea = "Area1";

            Series ser2 = new Series("Serie 2");
            ser2.Points.AddXY("A", 5);
            ser2.Points.AddXY("B", 3);
            ser2.Points.AddXY("C", 7);
            ser2.Points.AddXY("D", 8);
            ser2.Points.AddXY("E", 6);
            ser2.Color = Color.Blue;
            ser2.ChartArea = "Area1";

            _gvChart.Series.Add(ser1);
            _gvChart.Series.Add(ser2);
            _gvChart.Refresh();

            #endregion

            #region Tab Data

            ITableClass tc = (ITableClass)_layer.Class;
            foreach (IField field in _layer.Fields.ToEnumerable())
            {
                if (field.type == FieldType.biginteger ||
                    field.type == FieldType.Double ||
                    field.type == FieldType.Float ||
                    field.type == FieldType.integer ||
                    field.type == FieldType.smallinteger)
                {
                    lstSeries.Items.Add(new FieldItem() { Field = field });
                }

                if (field.type != FieldType.binary &&
                    field.type != FieldType.GEOGRAPHY &&
                    field.type != FieldType.GEOMETRY &&
                    field.type != FieldType.Shape)
                {
                    cmbDataFields.Items.Add(new FieldItem() { Field = field });
                }
            }
            if (cmbDataFields.Items.Count > 0)
                cmbDataFields.SelectedIndex = 0;

            #region Filter
            //All Features
            //Selected Features
            //Features in actual extent
            cmbFeatures.Items.Add(new ExportMethodItem("All Features", new QueryFilter()));

            if (_layer is IFeatureSelection &&
                ((IFeatureSelection)_layer).SelectionSet != null &&
                ((IFeatureSelection)_layer).SelectionSet.Count > 0)
            {
                ISelectionSet selectionSet = ((IFeatureSelection)_layer).SelectionSet;
                IQueryFilter selFilter = null;
                if (selectionSet is IIDSelectionSet)
                {
                    selFilter = new RowIDFilter(tc.IDFieldName, ((IIDSelectionSet)selectionSet).IDs);
                }
                else if (selectionSet is IGlobalIDSelectionSet)
                {
                    selFilter = new GlobalRowIDFilter(tc.IDFieldName, ((IGlobalIDSelectionSet)selectionSet).IDs);
                }
                else if (selectionSet is IQueryFilteredSelectionSet)
                {
                    selFilter = ((IQueryFilteredSelectionSet)selectionSet).QueryFilter.Clone() as IQueryFilter;
                }

                if (selFilter != null)
                {
                    selFilter.SubFields = "*";
                    ExportMethodItem item = new ExportMethodItem("Selected Features", selFilter);
                    cmbFeatures.Items.Add(item);
                    cmbFeatures.SelectedItem = item;
                }
            }

            if (_display != null && _display.Envelope != null)
            {
                SpatialFilter dispFilter = new SpatialFilter();
                dispFilter.SubFields = "*";

                dispFilter.FilterSpatialReference = _display.SpatialReference;
                dispFilter.Geometry = _display.Envelope;
                dispFilter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

                cmbFeatures.Items.Add(new ExportMethodItem("Features in actual extent", dispFilter));
            }

            if (cmbFeatures.SelectedIndex == -1)
                cmbFeatures.SelectedIndex = 0;
            #endregion

            #endregion
        }

        #region Properties

        public Series[] Series
        {
            get { return _series.ToArray(); }
        }
        public gvChart.DisplayMode DisplayMode
        {
            get { return _gvChart.ChartDisplayMode; }
        }
        public gvChart.gvChartType ChartType
        {
            get { return _gvChart.ChartType; }
        }

        public string ChartTitle
        {
            get { return txtChartTitle.Text; }
            set { txtChartTitle.Text = value; }
        }

        #endregion

        #region Item Classes

        private class FieldItem
        {
            public IField Field { get; set; }

            public override string ToString()
            {
                return String.IsNullOrEmpty(Field.aliasname) ?
                    Field.aliasname : Field.name;
            }
        }

        private class ExportMethodItem
        {
            private string _text;
            private IQueryFilter _filter;
            public ExportMethodItem(string text, IQueryFilter filter)
            {
                _text = text;
                _filter = filter;
            }

            public IQueryFilter QueryFilter
            {
                get { return _filter; }
            }

            public override string ToString()
            {
                return _text;
            }
        }

        #endregion

        #region Events

        private void lstChartTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                gvChart.gvChartType chartType = (gvChart.gvChartType)lstChartTypes.SelectedItem;
                panelDisplayMode.Visible = (chartType.DisplayMode == gvChart.DisplayMode.Mixed);

                _gvChart.ChartType = chartType;
                if (panelDisplayMode.Visible == true)
                    _gvChart.ChartDisplayMode = (gvChart.DisplayMode)cmbDisplayMode.SelectedIndex;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lstSeries_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = lstSeries.SelectedIndices != null && lstSeries.SelectedIndices.Count > 0;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstSeries.SelectedItems.Count; i++)
            {
                string fieldName = ((FieldItem)lstSeries.SelectedItems[i]).Field.name;

                if (seriesListView.Items != null)
                {
                    bool has = false;
                    foreach (ListViewItem item in seriesListView.Items)
                    {
                        if (item.Text == fieldName)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (has) continue;
                }

                SimpleFillSymbol symbol = (SimpleFillSymbol)RendererFunctions.CreateStandardSymbol(geometryType.Polygon);
                symbol.OutlineSymbol = null;
                if (symbol is ILegendItem)
                {
                    ((ILegendItem)symbol).LegendLabel = fieldName;
                    seriesListView.addSymbol(symbol, new string[] { fieldName, ((ILegendItem)symbol).LegendLabel });
                }
                else
                {
                    seriesListView.addSymbol(symbol, new string[] { fieldName, String.Empty });
                }
            }
        }

        private void seriesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = seriesListView.SelectedItems != null && seriesListView.SelectedItems.Count > 0;
        }

        private void seriesListView_OnLabelChanged(ISymbol symbol, int nr, string label)
        {
            if (symbol is ILegendItem)
                ((ILegendItem)symbol).LegendLabel = label;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            seriesListView.RemoveSelected();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ExportMethodItem filterItem = (ExportMethodItem)cmbFeatures.SelectedItem;
            IQueryFilter filter = filterItem.QueryFilter;
            filter.SubFields = String.Empty;

            DataTable tab = new DataTable();
            IField dataField = ((FieldItem)cmbDataFields.SelectedItem).Field;
            filter.AddField(dataField.name);
            tab.Columns.Add(new DataFieldColumn(dataField.name));

            Fields seriesFields = new Fields();
            foreach (SymbolsListView.SymbolListViewItem item in seriesListView.Items)
            {
                seriesFields.Add(((ITableClass)_layer.Class).Fields.FindField(item.Text));
                filter.AddField(item.Text);

                IBrushColor brushColor = item.Symbol as IBrushColor;
                Color col = brushColor != null ? brushColor.FillColor : Color.Red;

                tab.Columns.Add(new SeriesDataColumn(item.Text) { Color = col, SeriesName = item.SubItems[1].Text });
            }

            using (ICursor cursor = ((ITableClass)_layer.Class).Search(filterItem.QueryFilter))
            {
                IRow row = null;
                while ((row = NextRow(cursor)) != null)
                {
                    object dataValue = row[dataField.name];
                    if (dataValue == System.DBNull.Value) continue;

                    DataRow dataRow = GetDataRow(tab, dataValue);
                    for(int i=1,to=tab.Columns.Count;i<to;i++)
                    {
                        double val = Convert.ToDouble((row[tab.Columns[i].ColumnName] == System.DBNull.Value ? 0D : row[tab.Columns[i].ColumnName]));
                        dataRow[tab.Columns[i].ColumnName] = Convert.ToDouble(dataRow[tab.Columns[i].ColumnName]) + val;
                    }
                }
            }

            DataFieldColumn dataCol = this.DataFieldCol(tab);
            List<SeriesDataColumn> serCols = this.SeriesColumns(tab);

            if (tab != null && tab.Rows.Count > 0 && dataCol != null && serCols != null)
            {
                Series[] ser = new Series[serCols.Count];
                for (int i = 0; i < ser.Length; i++)
                {
                    ser[i] = new Series(serCols[i].SeriesName);
                    ser[i].Color = serCols[i].Color;
                }

                foreach (DataRow row in tab.Rows)
                {
                    for (int i = 0; i < ser.Length; i++)
                    {
                        ser[i].Points.AddXY(row[dataCol.ColumnName], row[serCols[i].ColumnName]);
                    }
                }

                for (int i = 0; i < ser.Length; i++)
                {
                    _series.Add(ser[i]);
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (tabWizard.SelectedIndex < tabWizard.TabPages.Count - 1)
                tabWizard.SelectedIndex++;

            btnBack.Enabled = tabWizard.SelectedIndex > 0;
            btnNext.Enabled = tabWizard.SelectedIndex < tabWizard.TabPages.Count - 1;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (tabWizard.SelectedIndex > 0)
                tabWizard.SelectedIndex--;

            btnBack.Enabled = tabWizard.SelectedIndex > 0;
            btnNext.Enabled = tabWizard.SelectedIndex < tabWizard.TabPages.Count - 1;
        }

        private void cmbDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _gvChart.ChartDisplayMode = (gvChart.DisplayMode)cmbDisplayMode.SelectedIndex;
        }

        #endregion

        #region Table

        public class SeriesDataColumn : DataColumn
        {
            public SeriesDataColumn(string name)
                : base(name, typeof(object))
            {
            }

            public Color Color { get; set; }
            public string SeriesName { get; set; }
        }

        public class DataFieldColumn : DataColumn
        {
            public DataFieldColumn(string name)
                : base(name, typeof(object))
            {
            }
        }

        private DataFieldColumn DataFieldCol(DataTable tab)
        {
            foreach (DataColumn col in tab.Columns)
            {
                if (col is DataFieldColumn)
                    return (DataFieldColumn)col;
            }
            return null;
        }

        public List<SeriesDataColumn> SeriesColumns(DataTable tab)
        {
            List<SeriesDataColumn> ret = new List<SeriesDataColumn>();

            foreach (DataColumn col in tab.Columns)
            {
                if (col is SeriesDataColumn)
                    ret.Add((SeriesDataColumn)col);
            }

            return ret.Count > 0 ? ret : null;
        }

        private IRow NextRow(ICursor cursor)
        {
            if (cursor is IFeatureCursor)
                return ((IFeatureCursor)cursor).NextFeature;

            if (cursor is IRowCursor)
                return ((IRowCursor)cursor).NextRow;

            return null;
        }

        private DataRow GetDataRow(DataTable tab, object dataValue)
        {
            foreach (DataRow row in tab.Rows)
            {
                if (row[0].Equals(dataValue))
                    return row;
            }

            DataRow newRow = tab.NewRow();
            newRow[0] = dataValue;
            tab.Rows.Add(newRow);
            for (int i = 1; i < tab.Columns.Count; i++)
                newRow[i] = 0D;
            return newRow;
        }

        #endregion
    }
}

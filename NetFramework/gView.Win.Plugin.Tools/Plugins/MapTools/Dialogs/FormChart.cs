using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using gView.Framework.UI;
using gView.Framework.system.UI;
using System.IO;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormChart : UserControl, IDockableWindow, IContextTools
    {
        private gvChart _gvChart;
        private Series[] _series;

        public FormChart(string title, Series[] series)
        {
            InitializeComponent();
 
            _series=series;
            _gvChart = new gvChart(chart1);

            chart1.Titles.Add(title);
            this.Name = title;
        }

        public gvChart.gvChartType ChartType
        {
            get { return _gvChart.ChartType; }
            set { _gvChart.ChartType = value; }
        }

        public gvChart.DisplayMode DisplayMode
        {
            get { return _gvChart.ChartDisplayMode; }
            set { _gvChart.ChartDisplayMode = value; }
        }

        private void FormChart_Load(object sender, EventArgs e)
        {
            foreach (Series ser in _series)
                _gvChart.Series.Add(ser);

            _gvChart.Refresh();
        }

        #region IDockableWindow Member

        public DockWindowState DockableWindowState
        {
            get
            {
                return DockWindowState.none;
            }
            set
            {
                
            }
        }

        public Image Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.pie_diagram; }
        }

        #endregion

        #region IContextTools Member

        public ITool[] ContextTools
        {
            get
            {
                return new ITool[]{
                    new SaveChart(),
                    new ChartTypeMenu(_gvChart.ChartType)
                };
            }
        }

        #endregion

        #region Context Tool Classes

        private class SaveChart : MapToolCls
        {
            public SaveChart()
                : base("Save Chart", global::gView.Plugins.Tools.Properties.Resources.save)
            {
            }

            public override void OnEvent(object MapEvent)
            {
                if (MapEvent is FormChart)
                {
                    FormChart chart = (FormChart)MapEvent;

                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "PNG|*.png|JPEG|*.jpg|Bitmap (*.bmp)|*.bmp|Tiff|*.tiff";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        FileInfo fi = new FileInfo(dlg.FileName);

                        switch (fi.Extension.ToLower())
                        {
                            case ".png":
                                chart.chart1.SaveImage(fi.FullName, ChartImageFormat.Png);
                                break;
                            case ".jpg":
                            case "jpeg":
                                chart.chart1.SaveImage(fi.FullName, ChartImageFormat.Jpeg);
                                break;
                            case ".bmp":
                                chart.chart1.SaveImage(fi.FullName, ChartImageFormat.Bmp);
                                break;
                            case ".tif":
                            case ".tiff":
                                chart.chart1.SaveImage(fi.FullName, ChartImageFormat.Tiff);
                                break;
                        }
                    }


                    //chart.chart1.SaveImage();
                }
            }
        }

        private class ChartTypeMenu : MapToolCls, IToolMenu
        {
            private ITool _selectedTool = null;
            private gvChart.gvChartType _chartType;

            public ChartTypeMenu(gvChart.gvChartType chartType)
                : base("Chart Type")
            {
                _chartType = chartType;
            }

            #region IToolMenu Member

            public List<ITool> DropDownTools
            {
                get
                {
                    List<ITool> tools = new List<ITool>();

                    foreach (gvChart.gvChartType chartType in gvChart.ChartTypes)
                    {
                        tools.Add(new ChartTypeTool(chartType));
                        if (chartType.ChartType == _chartType.ChartType)
                            _selectedTool = tools[tools.Count - 1];
                    }

                    return tools;
                }
            }

            public ITool SelectedTool
            {
                get
                {
                    return _selectedTool;
                }
                set
                {
                    _selectedTool = value;
                }
            }

            #endregion

            #region Tool Items

            private class ChartTypeTool : MapToolCls
            {
                public ChartTypeTool(gvChart.gvChartType chartType)
                    : base(chartType.Name)
                {
                    this.ChartType = chartType;
                }

                public gvChart.gvChartType ChartType
                {
                    get;
                    private set;
                }

                public override void OnEvent(object MapEvent)
                {
                    if (MapEvent is FormChart)
                    {
                        gvChart.gvChartType oldChartType = ((FormChart)MapEvent).ChartType;
                        try
                        {

                            ((FormChart)MapEvent).ChartType = this.ChartType;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            ((FormChart)MapEvent).ChartType = oldChartType;
                        }
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
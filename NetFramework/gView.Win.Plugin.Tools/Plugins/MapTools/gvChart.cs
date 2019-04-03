using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using gView.Framework.Globalisation;

namespace gView.Plugins.MapTools
{
    public class gvChart
    {
        public enum DisplayMode
        {
            EverySerieInTheSameChartArea = 0,
            EverySerieInExtraChartArea = 1,
            Mixed = 2
        }

        private Chart _chart;
        private DisplayMode _displayMode=DisplayMode.EverySerieInTheSameChartArea;
        private gvChartType _chartType = gvChart.ChartTypes[0]; // Column

        public gvChart(Chart chart)
        {
            _chart = chart;
            Series = new List<Series>();
        }

        public DisplayMode ChartDisplayMode
        {
            get
            {
                if (_chartType.DisplayMode != DisplayMode.Mixed)
                    return _chartType.DisplayMode;

                return _displayMode;
            }
            set
            {
                _displayMode = value;
                this.Refresh();
            }
        }
        
        public gvChartType ChartType
        {
            get { return _chartType; }
            set
            {
                _chartType = value;
                if (_chartType.DisplayMode != DisplayMode.Mixed)
                    _displayMode = _chartType.DisplayMode;
                this.Refresh();
            }
        }

        public List<Series> Series { get; set; }

        public void Refresh()
        {
            _chart.ChartAreas.Clear();
            _chart.Series.Clear();

            if (ChartDisplayMode == DisplayMode.EverySerieInTheSameChartArea || ChartDisplayMode==DisplayMode.Mixed)
            {
                _chart.ChartAreas.Add(CreateChartArea("area"));

                foreach (Series series in this.Series)
                {
                    Series ser = CopySeries(series);
                    ser.ChartArea = "area";
                    ser.ChartType = _chartType.ChartType;
                    _chart.Series.Add(ser);
                }
            }
            if (ChartDisplayMode == DisplayMode.EverySerieInExtraChartArea || ChartDisplayMode == DisplayMode.Mixed)
            {
                int i = 1;
                foreach (Series series in this.Series)
                {
                    _chart.ChartAreas.Add(CreateChartArea("area" + i));

                    Series ser = CopySeries(series);
                    ser.ChartArea = "area" + i;
                    ser.ChartType = _chartType.ChartType;
                    _chart.Series.Add(ser);

                    i++;
                }
            }
        }

        private Series CopySeries(Series ser)
        {
            Series copy = new Series(ser.Name);
            copy.Color = ser.Color;
            foreach (DataPoint dataPoint in ser.Points)
            {
                DataPoint dp = new DataPoint(dataPoint.XValue, dataPoint.YValues);
                dp.AxisLabel = dataPoint.AxisLabel;
                copy.Points.Add(dp);
            }
            return copy;
        }

        private ChartArea CreateChartArea(string name)
        {
            ChartArea area = new ChartArea(name);
            area.Area3DStyle.Enable3D = true;
            area.Area3DStyle.Rotation = 20;
            area.Area3DStyle.Inclination = 20;

            return area;
        }

        public class gvChartType
        {
            public SeriesChartType ChartType { get; set; }
            public string Name { get; set; }
            public DisplayMode DisplayMode { get; set; }

            public override string ToString()
            {
                return LocalizedResources.GetResString("Tools." + ChartType.ToString(), Name);
            }
        }

        public static gvChartType[] ChartTypes = new gvChartType[]
        {
            new gvChartType(){ ChartType = SeriesChartType.Column, DisplayMode = DisplayMode.Mixed, Name = "Column Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedColumn, DisplayMode = DisplayMode.Mixed, Name = "Stacked Column Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedColumn100, DisplayMode = DisplayMode.EverySerieInTheSameChartArea, Name = "Stacked Column Chart (100%)"},

            new gvChartType(){ ChartType = SeriesChartType.Point, DisplayMode = DisplayMode.Mixed, Name = "Point Chart"},
            new gvChartType(){ ChartType = SeriesChartType.FastPoint, DisplayMode = DisplayMode.Mixed, Name = "Fast Point Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Bubble, DisplayMode = DisplayMode.Mixed, Name = "Bubble Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Line, DisplayMode = DisplayMode.Mixed, Name = "Line Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Spline, DisplayMode = DisplayMode.Mixed, Name = "Spline Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StepLine, DisplayMode = DisplayMode.Mixed, Name = "Step Line Chart"},
            new gvChartType(){ ChartType = SeriesChartType.FastLine, DisplayMode = DisplayMode.Mixed, Name = "Fast Line Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Bar, DisplayMode = DisplayMode.Mixed, Name = "Bar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedBar, DisplayMode = DisplayMode.Mixed, Name = "Staked Bar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedBar100, DisplayMode = DisplayMode.EverySerieInTheSameChartArea, Name = "Stacket Bar Chart (100%)"},
            
            new gvChartType(){ ChartType = SeriesChartType.Area, DisplayMode = DisplayMode.Mixed, Name = "Area Chart"},
            new gvChartType(){ ChartType = SeriesChartType.SplineArea, DisplayMode = DisplayMode.Mixed, Name = "Spline Area Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedArea, DisplayMode = DisplayMode.Mixed, Name = "Stacked Area Chart"},
            new gvChartType(){ ChartType = SeriesChartType.StackedArea100, DisplayMode = DisplayMode.EverySerieInTheSameChartArea, Name = "Stacked Area Chart (100%)"},
            new gvChartType(){ ChartType = SeriesChartType.Pie, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Pie Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Doughnut, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Doughnut Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Stock, DisplayMode = DisplayMode.Mixed, Name = "Stock Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Candlestick, DisplayMode = DisplayMode.Mixed, Name = "Candelstick Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Range, DisplayMode = DisplayMode.Mixed, Name = "Range Chart"},
            new gvChartType(){ ChartType = SeriesChartType.SplineRange, DisplayMode = DisplayMode.Mixed, Name = "Spline Range Chart"},
            new gvChartType(){ ChartType = SeriesChartType.RangeBar, DisplayMode = DisplayMode.Mixed, Name = "Range Bar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.RangeColumn, DisplayMode = DisplayMode.Mixed, Name = "Range Column Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Radar, DisplayMode = DisplayMode.Mixed, Name = "Radar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Polar, DisplayMode = DisplayMode.Mixed, Name = "Polar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.ErrorBar, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Error Bar Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Renko, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Renko Chart"},
            new gvChartType(){ ChartType = SeriesChartType.ThreeLineBreak, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Three Line Break Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Kagi, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Kagi Chart"},
            new gvChartType(){ ChartType = SeriesChartType.PointAndFigure, DisplayMode = DisplayMode.EverySerieInExtraChartArea, Name = "Point&Figure Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Funnel, DisplayMode = DisplayMode.Mixed, Name = "Funnel Chart"},
            new gvChartType(){ ChartType = SeriesChartType.Pyramid, DisplayMode = DisplayMode.Mixed, Name = "Pyramid Chart"}
        };
    }
}

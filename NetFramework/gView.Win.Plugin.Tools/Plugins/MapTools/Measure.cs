using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using gView.Plugins.MapTools.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("D185D794-4BC8-4f3c-A5EA-494155692EAC")]
    public class Measure : gView.Framework.Snapping.Core.SnapTool, ITool, IToolContextMenu, IToolWindow
    {
        internal delegate void ShapeChangedEventHandler(MeasureGraphicsElement grElement);
        internal event ShapeChangedEventHandler ShapeChanged = null;

        IMapDocument _doc = null;
        GraphicsContainer _container;
        MeasureGraphicsElement _grElement;
        GeoUnits _lengthUnit = GeoUnits.Unknown, _areaUnit = GeoUnits.Unknown;
        private FormMeasure _dlg = null;

        public Measure()
        {
            this.InitializeComponents();

            _container = new GraphicsContainer();
            _container.Elements.Add(_grElement = new MeasureGraphicsElement());
        }

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Measure", "Measure"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.click; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[19]; }
        }

        public override void OnCreate(object hook)
        {
            base.OnCreate(hook);

            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _dlg = new FormMeasure(_doc);

                if (_doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).ActiveMapToolChanged += new ActiveMapToolChangedEvent(Measure_ActiveMapToolChanged);
                }
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventClick))
            {
                return Task.FromResult(true);
            }

            MapEventClick ev = (MapEventClick)MapEvent;

            if (_grElement.Stopped)
            {
                RemoveGraphicFromMap();
            }

            if (_lengthUnit == GeoUnits.Unknown && ev.Map != null && ev.Map.Display != null && ev.Map.Display.DisplayUnits != GeoUnits.Unknown)
            {
                _lengthUnit = ev.Map.Display.DisplayUnits;
            }
            if (_areaUnit == GeoUnits.Unknown && ev.Map != null && ev.Map.Display != null && ev.Map.Display.DisplayUnits != GeoUnits.Unknown)
            {
                _areaUnit = ev.Map.Display.DisplayUnits;
            }

            _grElement.AddPoint(ev.x, ev.y);
            ev.Map.Display.DrawOverlay(_container, true);
            if (ShapeChanged != null)
            {
                ShapeChanged(_grElement);
            }

            SetStatusText();

            return Task.FromResult(true);
        }

        #endregion

        void Measure_ActiveMapToolChanged(ITool OldTool, ITool NewTool)
        {
            if (_doc != null && _doc.Application is IMapApplication)
            {
                if (NewTool == this)
                {
                    ((IMapApplication)_doc.Application).OnCursorPosChanged -= new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                    ((IMapApplication)_doc.Application).OnCursorPosChanged += new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                }
                else if (OldTool == this)
                {
                    ((IMapApplication)_doc.Application).OnCursorPosChanged -= new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                }
            }
        }

        private double _X, _Y;
        void Application_OnCursorPosChanged(double X, double Y)
        {
            SetStatusText();
            _X = X;
            _Y = Y;

            IPoint moveAble = ((MeasureGraphicsElement)_container.Elements[0]).MoveAble;
            if (moveAble == null)
            {
                return;
            }

            moveAble.X = X;
            moveAble.Y = Y;

            if (cmnDynamic.Checked)
            {
                _doc.FocusMap.Display.DrawOverlay(_container, true);
            }

            _grElement.Dynamic = cmnDynamic.Checked;
            SetStatusText();
        }

        internal void RemoveGraphicFromMap()
        {
            if (_doc == null)
            {
                return;
            }

            foreach (IMap map in _doc.Maps)
            {
                if (map.Display == null || map.Display.GraphicsContainer == null)
                {
                    continue;
                }

                if (map.Display.GraphicsContainer.Elements.Contains(_grElement))
                {
                    map.Display.GraphicsContainer.Elements.Remove(_grElement);
                    //if (map == _doc.FocusMap) map.RefreshMap(DrawPhase.Graphics, null);
                    if (_doc.Application is IMapApplication)
                    {
                        ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
                    }
                }
            }
            _grElement.BeginNew();
            if (ShapeChanged != null)
            {
                ShapeChanged(_grElement);
            }
        }

        internal void Stop(bool close)
        {
            _grElement.Stop();
            if (close)
            {
                _grElement.Close();
            }

            if (!_doc.FocusMap.Display.GraphicsContainer.Elements.Contains(_grElement))
            {
                _doc.FocusMap.Display.GraphicsContainer.Elements.Add(_grElement);
            }
            //if (_doc.Application != null) _doc.Application.RefreshActiveMap(DrawPhase.Graphics);

            SetStatusText();
        }

        private void SetStatusText()
        {
            if (_doc == null || !(_doc.Application is IMapApplication) || _doc.FocusMap == null || _doc.FocusMap.Display == null)
            {
                return;
            }

            IStatusBar statusbar = ((IMapApplication)_doc.Application).StatusBar;
            if (statusbar == null)
            {
                return;
            }

            double length = _grElement.Length;
            double area = _grElement.Area;
            double segLength = _grElement.SegmentLength;
            double segAngle = _grElement.SegmentAngle;

            GeoUnitConverter converter = new GeoUnitConverter();
            length = Math.Round(converter.Convert(length, _doc.FocusMap.Display.MapUnits, _lengthUnit), 2);
            area = Math.Round(converter.Convert(area, _doc.FocusMap.Display.MapUnits, _areaUnit, 2), 2);
            segLength = Math.Round(converter.Convert(segLength, _doc.FocusMap.Display.MapUnits, _lengthUnit), 2);
            segAngle = Math.Round(segAngle, 2);

            string msg = "";
            if (!_grElement.Stopped)
            {
                msg += "Segment Length=" + segLength + " " + _lengthUnit.ToString();
                msg += " Angle=" + segAngle + "\u00b0 ";
            }
            msg += "Total Length=" + length + " " + _lengthUnit.ToString();
            if (area > 0.0)
            {
                msg += " Area=" + area + " " + _areaUnit.ToString() + "\u00b2";  // ^2
            }

            statusbar.Text = msg;

            if (_dlg != null)
            {
                _dlg.SetSegmentLength(segLength, _lengthUnit);
                _dlg.SetSegmentAngle(segAngle);
                _dlg.SetTotalLength(length, _lengthUnit);
                _dlg.SetTotalArea(area, _areaUnit);
            }
        }

        #region IToolContextMenu Member

        #region GUI
        private ContextMenuStrip contextMenuStripMeasure;
        private ToolStripMenuItem cmnDynamic;
        private ToolStripMenuItem cmnStopMeasuring;
        private ToolStripMenuItem cmnClosePolygonAndStop;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem cmnShowArea;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem cmnDistanceUnit;
        private ToolStripMenuItem cmnAreaUnit;

        private void InitializeComponents()
        {
            contextMenuStripMeasure = new System.Windows.Forms.ContextMenuStrip();
            cmnDynamic = new ToolStripMenuItem();
            cmnStopMeasuring = new System.Windows.Forms.ToolStripMenuItem();
            cmnClosePolygonAndStop = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            cmnShowArea = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            cmnDistanceUnit = new System.Windows.Forms.ToolStripMenuItem();
            cmnAreaUnit = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // contextMenuStripMeasure
            // 
            this.contextMenuStripMeasure.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmnDynamic,
            this.cmnStopMeasuring,
            this.cmnClosePolygonAndStop,
            this.toolStripSeparator1,
            this.cmnShowArea,
            this.toolStripSeparator2,
            cmnDistanceUnit,
            cmnAreaUnit});
            this.contextMenuStripMeasure.Name = "contextMenuStripMeasure";
            this.contextMenuStripMeasure.Size = new System.Drawing.Size(248, 104);
            // 
            // cmnDynamic
            // 
            this.cmnDynamic.Name = "cmnDynamic";
            this.cmnDynamic.Size = new System.Drawing.Size(247, 22);
            this.cmnDynamic.Text = "Dynamic";
            this.cmnDynamic.Checked = true;
            this.cmnDynamic.Click += new EventHandler(cmnDynamic_Click);
            // 
            // 
            // cmnStopMeasuring
            // 
            this.cmnStopMeasuring.Name = "cmnStopMeasuring";
            this.cmnStopMeasuring.Size = new System.Drawing.Size(247, 22);
            this.cmnStopMeasuring.Text = "Stop Measuring";
            this.cmnStopMeasuring.Click += new EventHandler(cmnStopMeasuring_Click);
            // 
            // cmnClosePolygonAndStop
            // 
            this.cmnClosePolygonAndStop.Name = "cmnClosePolygonAndStop";
            this.cmnClosePolygonAndStop.Size = new System.Drawing.Size(247, 22);
            this.cmnClosePolygonAndStop.Text = "Close Polygon and Stop Measuring";
            this.cmnClosePolygonAndStop.Click += new EventHandler(cmnClosePolygonAndStop_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnShowArea
            // 
            this.cmnShowArea.Name = "cmnShowArea";
            this.cmnShowArea.Size = new System.Drawing.Size(247, 22);
            this.cmnShowArea.Text = "Show Area";
            this.cmnShowArea.Click += new EventHandler(cmnShowArea_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnDistanceUnit
            // 
            this.cmnDistanceUnit.Name = "cmnDistanceUnit";
            this.cmnDistanceUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnDistanceUnit.Text = "Distance Unit";
            // 
            // cmnAreaUnit
            // 
            this.cmnAreaUnit.Name = "cmnAreaUnit";
            this.cmnAreaUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnAreaUnit.Text = "Area Unit";

            // Distance Units
            foreach (GeoUnits unit in Enum.GetValues(typeof(GeoUnits)))
            {
                if (unit <= 0)
                {
                    continue;
                }

                UnitMenuItem item1 = new UnitMenuItem(unit, false);
                UnitMenuItem item2 = new UnitMenuItem(unit, true);

                item1.Click += new EventHandler(UnitItem_Click);
                item2.Click += new EventHandler(UnitItem_Click);
                cmnDistanceUnit.DropDownItems.Add(item1);
                cmnAreaUnit.DropDownItems.Add(item2);
            }
        }

        void UnitItem_Click(object sender, EventArgs e)
        {
            if (!(sender is UnitMenuItem))
            {
                return;
            }

            if (((UnitMenuItem)sender).Square)
            {
                _areaUnit = ((UnitMenuItem)sender).Unit;
            }
            else
            {
                _lengthUnit = ((UnitMenuItem)sender).Unit;
            }

            SetStatusText();
        }

        void cmnDynamic_Click(object sender, EventArgs e)
        {
            cmnDynamic.Checked = !cmnDynamic.Checked;
            _grElement.Dynamic = cmnDynamic.Checked;

            _doc.FocusMap.Display.DrawOverlay(_container, true);
        }

        void cmnShowArea_Click(object sender, EventArgs e)
        {
            ((MeasureGraphicsElement)_container.Elements[0]).ShowArea = !((MeasureGraphicsElement)_container.Elements[0]).ShowArea;
        }

        void cmnClosePolygonAndStop_Click(object sender, EventArgs e)
        {
            this.Stop(true);
        }

        void cmnStopMeasuring_Click(object sender, EventArgs e)
        {
            this.Stop(false);
        }

        #endregion

        public ContextMenuStrip ContextMenu
        {
            get
            {
                cmnStopMeasuring.Enabled = !_grElement.Stopped;
                cmnClosePolygonAndStop.Enabled = !_grElement.Stopped;
                cmnShowArea.Checked = _grElement.ShowArea;

                foreach (UnitMenuItem item in cmnDistanceUnit.DropDownItems)
                {
                    item.Checked = _lengthUnit == item.Unit;
                }

                foreach (UnitMenuItem item in cmnAreaUnit.DropDownItems)
                {
                    item.Checked = _areaUnit == item.Unit;
                }

                return contextMenuStripMeasure;
            }
        }

        #endregion

        #region IToolWindow Member

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion

        public override bool ShowSnapMarker
        {
            get
            {
                if (_grElement != null && _grElement.PointCount > 0)
                {
                    return false;
                }

                return base.ShowSnapMarker;
            }
        }
    }
}

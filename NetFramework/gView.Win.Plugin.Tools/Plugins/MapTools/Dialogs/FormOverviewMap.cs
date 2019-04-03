using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.Symbology;
using gView.Framework.UI.Events;
using System.Threading;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormOverviewMap : Form
    {
        private IMapDocument _doc;
        private IMap _ovmap;
        private EnvelopeGraphics _envGraphics;
        private OVTool _tool;

        public FormOverviewMap(IMapDocument doc)
        {
            InitializeComponent();

            _doc = doc;
            if (_doc.Application is IMapApplication)
            {
                this.ShowInTaskbar = false;
                this.TopLevel = true;
                this.Owner = ((IMapApplication)_doc.Application).DocumentWindow as Form;
            }
            _envGraphics = new EnvelopeGraphics();
            _tool = new OVTool();
            _tool.OnCreate(_doc);
            _tool.RefreshOverviewMap += new OVTool.RefreshOverviewMapHandler(Tool_RefreshOverviewMap);
        }

        private void Tool_RefreshOverviewMap()
        {
            if (_doc == null || _doc.FocusMap == null)
            {
                InsertMapExtent(_doc.FocusMap.Display);
            }
            BeginRefreshThread(DrawPhase.All);
        }

        private void FormOverviewMap_Load(object sender, EventArgs e)
        {
            RefreshOverviewMap();
        }

        public void RefreshOverviewMap()
        {
            if (_doc == null || _doc.FocusMap == null) return;

            _ovmap = new Map(_doc.FocusMap as Map, false);
            mapView1.Map = _ovmap;
            mapView1.resizeMode = gView.Framework.UI.Controls.MapView.ResizeMode.SameExtent;

            _ovmap.Display.ZoomTo(_doc.FocusMap.Display.Limit);
            _ovmap.NewBitmap += new NewBitmapEvent(mapView1.NewBitmapCreated);
            _ovmap.Display.SpatialReference = _doc.FocusMap.Display.SpatialReference;

            InsertMapExtent(_doc.FocusMap.Display);
            _tool.OvMap = _ovmap;
            mapView1.Tool = _tool;

            BeginRefreshThread(DrawPhase.All);
        }

        private void BeginRefreshThread(DrawPhase phase)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(this.StartRefreshTread));
            thread.Start(phase);
        }
        private void StartRefreshTread(object phase)
        {
            //this.Cursor = Cursors.WaitCursor;
            mapView1.RefreshMap((DrawPhase)phase);
            //this.Cursor = Cursors.Cross;
        }

        private void InsertMapExtent(IDisplay display)
        {
            if (_ovmap == null || display == null ||
                _ovmap.Display == null) return;

            IGeometry mapEnv = display.Envelope;
            _ovmap.Display.GraphicsContainer.Elements.Clear();
            if (display.SpatialReference != null &&
                !display.SpatialReference.Equals(_ovmap.Display.SpatialReference))
            {
                mapEnv = GeometricTransformer.Transform2D(
                    mapEnv, display.SpatialReference,
                    _ovmap.Display.SpatialReference);
            }

            _envGraphics.LimitEnvelope = _ovmap.Display.Envelope;
            _envGraphics.Geometry = mapEnv;
            _ovmap.Display.GraphicsContainer.Elements.Add(_envGraphics);
        }
        public void DrawMapExtent(IDisplay display)
        {
            InsertMapExtent(display);
            BeginRefreshThread(DrawPhase.Graphics);
        }

        #region HelperClasses
        private class EnvelopeGraphics : IGraphicElement
        {
            private ISymbol _symbol, _symbol2;
            private IGeometry _geometry = null;
            private IPolyline _cross = null;
            private IEnvelope _limit = null;

            public EnvelopeGraphics()
            {
                _symbol = PlugInManager.Create(KnownObjects.Symbology_SimpleFillSymbol) as ISymbol;

                if (_symbol is IBrushColor)
                    ((IBrushColor)_symbol).FillColor = Color.FromArgb(155, Color.White);

                _symbol2 = PlugInManager.Create(KnownObjects.Symbology_SimpleLineSymbol) as ISymbol;
                if (_symbol2 is IPenColor)
                    ((IPenColor)_symbol2).PenColor = Color.Blue;
            }

            public IGeometry Geometry
            {
                get { return _geometry; }
                set
                {
                    if (value is IPolygon ||
                        value is IEnvelope)
                    {
                        _geometry = value;

                        if (_limit != null)
                        {
                            IPoint center = _geometry.Envelope.Center;

                            _cross = new Polyline();
                            Path p1 = new Path();
                            p1.AddPoint(new gView.Framework.Geometry.Point(_limit.minx, center.Y));
                            p1.AddPoint(new gView.Framework.Geometry.Point(_limit.maxx, center.Y));
                            Path p2 = new Path();
                            p2.AddPoint(new gView.Framework.Geometry.Point(center.X, _limit.miny));
                            p2.AddPoint(new gView.Framework.Geometry.Point(center.X, _limit.maxy));
                            _cross.AddPath(p1);
                            _cross.AddPath(p2);
                        }
                    }
                }
            }

            public IEnvelope LimitEnvelope
            {
                get { return _limit; }
                set { _limit = value; }
            }
            #region IGraphicElement Member

            public void Draw(IDisplay display)
            {
                if (_geometry != null && _symbol != null)
                    display.Draw(_symbol, _geometry);

                if (_cross != null && _symbol2 != null)
                    display.Draw(_symbol2, _cross);

                if (_geometry != null && _symbol2 != null)
                    display.Draw(_symbol2, _geometry);
            }

            #endregion
        }
        private class OVTool : ITool, IToolContextMenu
        {
            private IMapDocument _doc = null;
            private ContextMenuStrip _contextMenu;
            private IMap _ovmap = null;
            public delegate void RefreshOverviewMapHandler();
            public event RefreshOverviewMapHandler RefreshOverviewMap = null;

            public OVTool()
            {
                _contextMenu = new ContextMenuStrip();

                ToolStripMenuItem item = new ToolStripMenuItem(
                    "Zoom to actual extent",
                    global::gView.Plugins.Tools.Properties.Resources.zoom
                    );
                item.Click += new EventHandler(zoom2actual_Click);
                _contextMenu.Items.Add(item);

                item = new ToolStripMenuItem(
                    "Zoom to maximum extent",
                    global::gView.Plugins.Tools.Properties.Resources.map16);

                item.Click += new EventHandler(zoom2max_Click);
                _contextMenu.Items.Add(item);

                if (RefreshOverviewMap != null)
                    RefreshOverviewMap();
            }

            void zoom2actual_Click(object sender, EventArgs e)
            {
                if (_doc == null || _doc.FocusMap == null || _doc.FocusMap.Display == null ||
                    _ovmap == null || _ovmap.Display == null) return;

                IEnvelope extent = _doc.FocusMap.Display.Envelope;
                if (_doc.FocusMap.Display.SpatialReference != null &&
                    !_doc.FocusMap.Display.SpatialReference.Equals(_ovmap.Display.SpatialReference))
                {
                    extent = GeometricTransformer.Transform2D(
                        extent,
                        _doc.FocusMap.Display.SpatialReference,
                        _ovmap.Display.SpatialReference).Envelope;
                }

                _ovmap.Display.ZoomTo(extent);

                if (RefreshOverviewMap != null)
                    RefreshOverviewMap();
            }
            void zoom2max_Click(object sender, EventArgs e)
            {
                if (_doc == null || _doc.FocusMap == null || _doc.FocusMap.Display == null ||
                    _ovmap == null || _ovmap.Display == null) return;

                IEnvelope extent = _doc.FocusMap.Display.Limit;
                if (_doc.FocusMap.Display.SpatialReference != null &&
                    !_doc.FocusMap.Display.SpatialReference.Equals(_ovmap.Display.SpatialReference))
                {
                    extent = GeometricTransformer.Transform2D(
                        extent,
                        _doc.FocusMap.Display.SpatialReference,
                        _ovmap.Display.SpatialReference).Envelope;
                }

                _ovmap.Display.ZoomTo(extent);

                if (RefreshOverviewMap != null)
                    RefreshOverviewMap();
            }

            public IMap OvMap
            {
                get { return _ovmap; }
                set { _ovmap = value; }
            }
            #region ITool Member

            public string Name
            {
                get { return "OVTool"; }
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
                get { return ToolType.rubberband; }
            }

            public object Image
            {
                get { return null; }
            }

            public void OnCreate(object hook)
            {
                if (hook is IMapDocument)
                    _doc = (IMapDocument)hook;
            }

            public void OnEvent(object MapEvent)
            {
                if (_doc == null || _doc.FocusMap == null || _doc.FocusMap.Display == null) return;
                if (!(MapEvent is MapEventRubberband)) return;

                MapEventRubberband ev = (MapEventRubberband)MapEvent;
                if (ev.Map == null) return;
                
                if (!(ev.Map.Display is Display)) return;
                Display nav = (Display)ev.Map.Display;

                IEnvelope extent = new Envelope(ev.minX, ev.minY, ev.maxX, ev.maxY);
                if (ev.Map.Display.SpatialReference != null &&
                    !ev.Map.Display.SpatialReference.Equals(_doc.FocusMap.Display.SpatialReference))
                {
                    extent = GeometricTransformer.Transform2D(
                        extent,
                        ev.Map.Display.SpatialReference,
                        _doc.FocusMap.Display.SpatialReference).Envelope;
                }
                if (Math.Abs(ev.maxX - ev.minX) < 1e-5 ||
                    Math.Abs(ev.maxY - ev.minY) < 1e-5)
                {
                    IEnvelope dispEnv = new Envelope(_doc.FocusMap.Display.Envelope);
                    dispEnv.Center = extent.Center;
                    _doc.FocusMap.Display.ZoomTo(dispEnv);
                }
                else
                {
                    _doc.FocusMap.Display.ZoomTo(extent); 
                }

                if (_doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                }

                //ev.refreshMap = true;
                //ev.drawPhase = DrawPhase.Graphics;
            }

            #endregion

            #region IToolContextMenu Member

            public ContextMenuStrip ContextMenu
            {
                get
                {
                    return _contextMenu;
                }
            }

            #endregion
        }
        #endregion

        private void mapView1_MouseDown(object sender, MouseEventArgs e)
        {
            //this.MouseLeave -= new EventHandler(mapView1_MouseLeave);
            //this.MouseEnter -= new EventHandler(mapView1_MouseEnter);
        }

        private void mapView1_MouseUp(object sender, MouseEventArgs e)
        {
            //this.MouseLeave += new EventHandler(mapView1_MouseLeave);
            //this.MouseEnter += new EventHandler(mapView1_MouseEnter);
        }

        private void FormOverviewMap_MouseEnter(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void FormOverviewMap_MouseLeave(object sender, EventArgs e)
        {
            if (this.Owner != null)
                this.Owner.Activate();
        }
    }
}
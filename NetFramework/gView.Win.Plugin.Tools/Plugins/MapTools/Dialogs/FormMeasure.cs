using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.Globalisation;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormMeasure : UserControl,IDockableToolWindow
    {
        IMapDocument _doc = null;
        Measure _tool = null;

        public FormMeasure(IMapDocument doc)
        {
            InitializeComponent();

            this.Name = LocalizedResources.GetResString("Tools.Measure", "Measure");
            _doc = doc;
            _tool = MeasureTool;
            _tool.ShapeChanged += new Measure.ShapeChangedEventHandler(tool_ShapeChanged);
        }

        private delegate void tool_ShapeChangedCallback(MeasureGraphicsElement grElement);
        void tool_ShapeChanged(MeasureGraphicsElement grElement)
        {
            if (grElement == null) return;
            if (this.toolStrip1.InvokeRequired)
            {
                tool_ShapeChangedCallback d = new tool_ShapeChangedCallback(tool_ShapeChanged);
                this.Invoke(d, new object[] { grElement });
            }
            else
            {
                btnNew.Enabled = grElement.PointCount > 0;
                btnStop.Enabled = grElement.PointCount > 1;
                btnStopAndClose.Enabled = grElement.PointCount > 2;
            }
        }

        #region IDockableWindow Member

        private DockWindowState _dockState = DockWindowState.none;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _dockState;
            }
            set
            {
                _dockState = value;
            }
        }

        public Image Image
        {
            get { return (new Buttons()).imageList1.Images[19]; }
        }

        #endregion

        private delegate void SetSegmentLengthCallback(double length, GeoUnits unit);
        public void SetSegmentLength(double length, GeoUnits unit)
        {
            if (txtSegmentLength.InvokeRequired)
            {
                SetSegmentLengthCallback d = new SetSegmentLengthCallback(SetSegmentLength);
                this.Invoke(d, new object[] { length, unit });
            }
            else
            {
                txtSegmentLength.Text = length.ToString();
                lblSegmentLenghtUnit.Text = unit.ToString();
            }
        }

        private delegate void SetSegmentAngleCallback(double angle);
        public void SetSegmentAngle(double angle)
        {
            if (txtSegmentLength.InvokeRequired)
            {
                SetSegmentAngleCallback d = new SetSegmentAngleCallback(SetSegmentAngle);
                this.Invoke(d, new object[] { angle });
            }
            else
            {
                txtSegmentAngle.Text = angle.ToString();
            }
        }

        private delegate void SetTotalLengthCallback(double length, GeoUnits unit);
        public void SetTotalLength(double length, GeoUnits unit)
        {
            if (txtTotalLength.InvokeRequired)
            {
                SetTotalLengthCallback d = new SetTotalLengthCallback(SetTotalLength);
                this.Invoke(d, new object[] { length, unit });
            }
            else
            {
                txtTotalLength.Text = length.ToString();
                lblTotalLengthUnit.Text = unit.ToString();
            }
        }

        private delegate void SetTotalAreaCallback(double length, GeoUnits unit);
        public void SetTotalArea(double length, GeoUnits unit)
        {
            if (txtTotalArea.InvokeRequired)
            {
                SetTotalAreaCallback d = new SetTotalAreaCallback(SetTotalArea);
                this.Invoke(d, new object[] { length, unit });
            }
            else
            {
                txtTotalArea.Text = length.ToString();
                lblTotalAreaUnit.Text = ((unit != GeoUnits.Unknown) ? "Square " : "") + unit.ToString();
            }
        }

        private Measure MeasureTool
        {
            get
            {
                if (_doc == null || !(_doc.Application is IGUIApplication)) return null;
                return ((IGUIApplication)_doc.Application).Tool(new Guid("D185D794-4BC8-4f3c-A5EA-494155692EAC")) as Measure;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (_tool == null) return;

            _tool.Stop(false);
            _tool.RemoveGraphicFromMap();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_tool == null) return;

            _tool.Stop(false);
            if (_doc.Application is IMapApplication) 
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
        }

        private void btnStopAndClose_Click(object sender, EventArgs e)
        {
            if (_tool == null) return;

            _tool.Stop(true);
            if (_doc.Application is IMapApplication) 
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
        }
    }
}
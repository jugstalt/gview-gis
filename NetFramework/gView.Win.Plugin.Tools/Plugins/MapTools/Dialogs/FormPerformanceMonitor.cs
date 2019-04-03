using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Carto;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormPerformanceMonitor : UserControl, IDockableToolWindow
    {
        private IMapDocument _doc;
        private TimeStatistics _statistics;
        private IMap _activeMap = null;

        public FormPerformanceMonitor(IMapDocument doc)
        {
            InitializeComponent();

            _statistics = new TimeStatistics();
            _statistics.TimeEventsRemoved += new TimeEventsRemovedEventHandler(_statistics_TimeEventsRemoved);
            _statistics.TimeEventAdded += new TimeEventAddedEventHandler(_statistics_TimeEventAdded);

            _doc = doc;
            _doc.AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
            this.Name = "Performance Monitor";
        }

        void _doc_AfterSetFocusMap(gView.Framework.Carto.IMap map)
        {
            SetEvents();
        }

        void _statistics_TimeEventAdded(ITimeStatistics sender, ITimeEvent timeEvent)
        {
            if (this.InvokeRequired)
            {
                TimeEventAddedEventHandler d = new TimeEventAddedEventHandler(_statistics_TimeEventAdded);
                this.BeginInvoke(d, new object[] { sender, timeEvent });
            }
            else
            {
                lstEvents.Items.Add(new ListViewItem(
                    new string[] { 
                        timeEvent.Name, 
                        timeEvent.Duration.TotalMilliseconds.ToString() + "ms",
                        timeEvent.Counter>=0 ?timeEvent.Counter.ToString() : String.Empty },
                        0));
            }
        }

        void _statistics_TimeEventsRemoved(ITimeStatistics sender)
        {
            if (this.InvokeRequired)
            {
                TimeEventsRemovedEventHandler d = new TimeEventsRemovedEventHandler(_statistics_TimeEventsRemoved);
                this.BeginInvoke(d, new object[] { sender });
            }
            else
            {
                lstEvents.Items.Clear();
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
            get { return global::gView.Plugins.Tools.Properties.Resources.time; }
        }

        #endregion

        private void chkPerform_CheckedChanged(object sender, EventArgs e)
        {
            if (_doc == null || _doc.FocusMap == null) return;

            SetEvents();
        }

        private void SetEvents()
        {
            if (_activeMap != null)
            {
                _activeMap.DrawingLayerFinished -= new gView.Framework.Carto.DrawingLayerFinishedEvent(FocusMap_DrawingLayerFinished);
                _activeMap.StartRefreshMap -= new gView.Framework.Carto.StartRefreshMapEvent(FocusMap_StartRefreshMap);
            }
            if (_doc == null)
            {
                _activeMap = null;
                return;
            }
            _activeMap = _doc.FocusMap;

            if (chkPerform.Checked)
            {
                if (_activeMap != null)
                {
                    _activeMap.StartRefreshMap += new gView.Framework.Carto.StartRefreshMapEvent(FocusMap_StartRefreshMap);
                    _activeMap.DrawingLayerFinished += new gView.Framework.Carto.DrawingLayerFinishedEvent(FocusMap_DrawingLayerFinished);
                }
            }
        }

        void FocusMap_StartRefreshMap(gView.Framework.Carto.IMap sender)
        {
            _statistics.RemoveTimeEvents();
        }

        void FocusMap_DrawingLayerFinished(gView.Framework.Carto.IMap sender, gView.Framework.system.ITimeEvent timeEvent)
        {
            _statistics.AddTimeEvent(timeEvent);
        }
    }
}
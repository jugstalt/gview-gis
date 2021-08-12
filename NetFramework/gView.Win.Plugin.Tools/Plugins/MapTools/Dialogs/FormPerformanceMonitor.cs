using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormPerformanceMonitor : UserControl, IDockableToolWindow
    {
        private IMapDocument _doc;
        private TimeStatistics _statistics;
        private IMap _activeMap = null;
        private ListViewColumnSorter lvwColumnSorter;

        public FormPerformanceMonitor(IMapDocument doc)
        {
            InitializeComponent();

            _statistics = new TimeStatistics();
            _statistics.TimeEventsRemoved += new TimeEventsRemovedEventHandler(_statistics_TimeEventsRemoved);
            _statistics.TimeEventAdded += new TimeEventAddedEventHandler(_statistics_TimeEventAdded);

            _doc = doc;
            _doc.AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
            this.Name = "Performance Monitor";

            lvwColumnSorter = new ListViewColumnSorter();
            this.lstEvents.ListViewItemSorter = lvwColumnSorter;
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
                            ((int)timeEvent.Duration.TotalMilliseconds).ToString(),
                            timeEvent.Counter.ToString() 
                        },
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
            get { return gView.Win.Plugin.Tools.Properties.Resources.time; }
        }

        #endregion

        private void chkPerform_CheckedChanged(object sender, EventArgs e)
        {
            if (_doc == null || _doc.FocusMap == null)
            {
                return;
            }

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

        private void lstEvents_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.lstEvents.Sort();
        }

        #region Classes

        public class ListViewColumnSorter : IComparer
        {
            /// <summary>
            /// Specifies the column to be sorted
            /// </summary>
            private int ColumnToSort;

            /// <summary>
            /// Specifies the order in which to sort (i.e. 'Ascending').
            /// </summary>
            private SortOrder OrderOfSort;

            /// <summary>
            /// Case insensitive comparer object
            /// </summary>
            private CaseInsensitiveComparer ObjectCompare;

            /// <summary>
            /// Class constructor. Initializes various elements
            /// </summary>
            public ListViewColumnSorter()
            {
                // Initialize the column to '0'
                ColumnToSort = 0;

                // Initialize the sort order to 'none'
                OrderOfSort = SortOrder.None;

                // Initialize the CaseInsensitiveComparer object
                ObjectCompare = new CaseInsensitiveComparer();
            }

            /// <summary>
            /// This method is inherited from the IComparer interface. It compares the two objects passed using a case insensitive comparison.
            /// </summary>
            /// <param name="x">First object to be compared</param>
            /// <param name="y">Second object to be compared</param>
            /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                var textX = listviewX.SubItems[ColumnToSort].Text;
                var textY = listviewY.SubItems[ColumnToSort].Text;

                if (int.TryParse(textX, out int intX) && int.TryParse(textY, out int intY))
                {
                    compareResult = intX.CompareTo(intY); 
                }
                else
                {
                    compareResult = ObjectCompare.Compare(textX, textY);
                }

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal
                    return 0;
                }
            }

            /// <summary>
            /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            /// <summary>
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
            /// </summary>
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }

        }

        #endregion

        
    }
}
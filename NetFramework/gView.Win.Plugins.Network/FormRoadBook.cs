using gView.Framework.Network;
using gView.Framework.Network.Algorthm;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.Network
{
    public partial class FormRoadBook : Form, IDockableToolWindow
    {
        public FormRoadBook()
        {
            InitializeComponent();

            this.Name = "Road Book";
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
            get { return null; }
        }

        #endregion

        #region Members
        async public Task ShowPathDescription(INetworkFeatureClass nfc, Dijkstra dijkstra, int targetNodeId)
        {
            lstItems.Items.Clear();
            lstItems.Columns.Clear();

            RoadBook roadbook = new RoadBook(nfc);
            RoadBook.Items items = await roadbook.PathDescription(dijkstra, targetNodeId);

            lstItems.Columns.Add(new ColumnHeader());
            lstItems.Columns[0].Text = "Distance";
            foreach (string attribute in items.AttributeNames)
            {
                lstItems.Columns.Add(new ColumnHeader());
                lstItems.Columns[lstItems.Columns.Count - 1].Text = attribute;
            }

            foreach (RoadBook.Item item in items)
            {
                if (item.Feature == null)
                {
                    continue;
                }

                List<string> tags = new List<string>();
                tags.Add(Math.Round(item.Distance, 2).ToString());

                for (int i = 1; i < lstItems.Columns.Count - 1; i++)
                {
                    object obj = item.Feature[lstItems.Columns[i].Text];
                    if (obj == null)
                    {
                        tags.Add(String.Empty);
                    }
                    else
                    {
                        tags.Add(obj.ToString());
                    }
                }
                lstItems.Items.Add(new ListViewItem(tags.ToArray()));
            }
        }
        #endregion
    }
}

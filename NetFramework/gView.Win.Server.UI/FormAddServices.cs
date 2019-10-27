using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.UI.Dialogs;

namespace gView.MapServer.Lib.UI
{
    public partial class FormAddServices : Form
    {
        private List<IMap> _maps;

        public FormAddServices(IMap map)
        {
            if (map == null)
            {
                this.Close();
                return;
            }
            _maps = new List<IMap>();
            _maps.Add(map);

            InitializeComponent();

            MakeList();
        }
        public FormAddServices(List<IMap> maps)
        {
            _maps = maps;
            if (maps == null || maps.Count == 0)
            {
                this.Close();
                return;
            }

            InitializeComponent();

            MakeList();
        }

        private void MakeList()
        {
            foreach (IMap map in _maps)
            {
                dgMaps.Rows.Add(new object[] { true, map.Name, "", map.Name });
            }
        }

        public List<IMap> Maps
        {
            get
            {
                List<IMap> maps = new List<IMap>();

                for (int i = 0; i < dgMaps.Rows.Count; i++)
                {
                    if ((bool)dgMaps[0, i].Value)
                    {
                        maps.Add(_maps[i]);
                    }
                }

                return maps;
            }
        }

        public List<string> ServiceNames
        {
            get
            {
                List<string> names = new List<string>();
                for (int i = 0; i < dgMaps.Rows.Count; i++)
                {
                    if ((bool)dgMaps[0, i].Value)
                    {
                        names.Add(dgMaps[3, i].Value.ToString());
                    }
                }
                return names;
            }
        }

        private void dgMaps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                FormMapProperties dlg = new FormMapProperties(
                    null,
                    _maps[e.RowIndex], _maps[e.RowIndex].Display);
                dlg.ShowDialog();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                this.Parent.Cursor = Cursors.WaitCursor;
            }
        }
    }
}
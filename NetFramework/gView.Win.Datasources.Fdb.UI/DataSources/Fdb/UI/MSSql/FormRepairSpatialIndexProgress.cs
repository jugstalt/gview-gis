using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormRepairSpatialIndexProgress : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        private bool _finished = true;
        private Task _processTask = null;

        public FormRepairSpatialIndexProgress(gView.DataSources.Fdb.MSAccess.AccessFDB fdb, IFeatureClass fc)
        {
            InitializeComponent();

            _fdb = fdb;
            _fc = fc;

            if (_fc != null)
            {
                this.Text += ": " + _fc.Name;
            }
        }

        private void FormRepairSpatialIndexProgress_Load(object sender, EventArgs e)
        {

        }

        private void FormRepairSpatialIndexProgress_Shown(object sender, EventArgs e)
        {
            if (_fdb != null && _fc != null)
            {
                _finished = false;
                _processTask = Process();
            }
            else
            {
                this.Close();
            }
        }

        private void FormRepairSpatialIndexProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_finished)
            {
                e.Cancel = true;
            }
        }

        // Thread
        async private Task Process()
        {
            if (!await _fdb.RepairSpatialIndex(_fc.Name, new EventHandler(ProcessEventHandler)))
            {
                MessageBox.Show(_fdb.LastErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _finished = true;
            ProcessEventHandler(this, new EventArgs());
        }

        private void ProcessEventHandler(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler d = new EventHandler(ProcessEventHandler);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                if (sender == this)
                {
                    btnClose.Enabled = true;
                    this.Refresh();
                }
                else if (e is RepairSICheckNodes)
                {
                    if (((RepairSICheckNodes)e).Pos == 1)
                    {
                        this.Refresh();
                    }

                    if (((RepairSICheckNodes)e).Pos == ((RepairSICheckNodes)e).Count ||
                       ((RepairSICheckNodes)e).Pos % 100 == 0)
                    {
                        if (progressBar1.Maximum != ((RepairSICheckNodes)e).Count)
                        {
                            progressBar1.Maximum = ((RepairSICheckNodes)e).Count;
                        }

                        progressBar1.Value = Math.Min(progressBar1.Maximum, ((RepairSICheckNodes)e).Pos);

                        plabel1.Text = progressBar1.Value + "/" + progressBar1.Maximum;
                        if (((RepairSICheckNodes)e).WrongNIDs != progressBar2.Maximum)
                        {
                            progressBar2.Maximum = ((RepairSICheckNodes)e).WrongNIDs;
                            plabel2.Text = "0/" + progressBar2.Maximum;
                        }

                        progressBar1.Refresh();
                        plabel1.Refresh();
                        plabel2.Refresh();
                    }
                }
                else if (e is RepairSIUpdateNodes)
                {
                    if (((RepairSIUpdateNodes)e).Pos == ((RepairSIUpdateNodes)e).Count ||
                       ((RepairSIUpdateNodes)e).Pos % 100 == 0)
                    {
                        if (progressBar2.Maximum != ((RepairSIUpdateNodes)e).Count)
                        {
                            progressBar2.Maximum = ((RepairSIUpdateNodes)e).Count;
                        }

                        progressBar2.Value = Math.Min(progressBar2.Maximum, ((RepairSIUpdateNodes)e).Pos);

                        plabel2.Text = progressBar2.Value + "/" + progressBar2.Maximum;

                        if (((RepairSIUpdateNodes)e).Count % 100 == 0)
                        {
                            progressBar2.Refresh();
                            plabel2.Refresh();
                        }
                    }
                }
            }
        }
    }
}
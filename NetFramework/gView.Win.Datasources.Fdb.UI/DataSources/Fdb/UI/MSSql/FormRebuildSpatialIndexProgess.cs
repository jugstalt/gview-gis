using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormRebuildSpatialIndexProgess : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        private BinaryTreeDef _def;
        private bool _finished = true;
        private Task _processTask = null;

        public FormRebuildSpatialIndexProgess(gView.DataSources.Fdb.MSAccess.AccessFDB fdb, IFeatureClass fc, BinaryTreeDef def)
        {
            InitializeComponent();

            _fdb = fdb;
            _fc = fc;
            _def = def;

        }

        private void FormRebuildSpatialIndexProgess_Load(object sender, EventArgs e)
        {
            
        }

        private void FormRebuildSpatialIndexProgess_Shown(object sender, EventArgs e)
        {
            if (_fdb != null && _fc != null && _def != null)
            {
                _finished = false;
                _processTask = Process();
            }
            else
            {
                this.Close();
            }
        }

        async private Task Process()
        {
            if (!await _fdb.RebuildSpatialIndexDef(_fc.Name, _def, new EventHandler(ProcessEventHandler)))
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
                    this.Close();
                }
                else if (e is UpdateSIDefEventArgs && ((UpdateSIDefEventArgs)e).Finished)
                {
                    progressBar1.Value = progressBar1.Maximum;
                }
                else if (e is UpdateSICalculateNodes)
                {
                    if (((UpdateSICalculateNodes)e).Pos == 1)
                    {
                        this.Refresh();
                    }
                    if (((UpdateSICalculateNodes)e).Pos == ((UpdateSICalculateNodes)e).Count ||
                       ((UpdateSICalculateNodes)e).Pos % 100 == 0)
                    {
                        if (progressBar2.Maximum != ((UpdateSICalculateNodes)e).Count)
                        {
                            progressBar2.Maximum = ((UpdateSICalculateNodes)e).Count;
                        }

                        progressBar2.Value = Math.Min(progressBar2.Maximum, ((UpdateSICalculateNodes)e).Pos);

                        plabel2.Text = progressBar2.Value + "/" + progressBar2.Maximum;

                        progressBar2.Refresh();
                        plabel2.Refresh();
                    }
                }
                else if (e is UpdateSIUpdateNodes)
                {
                    if (((UpdateSIUpdateNodes)e).Pos == ((UpdateSIUpdateNodes)e).Count ||
                       ((UpdateSIUpdateNodes)e).Pos % 100 == 0)
                    {
                        if (progressBar3.Maximum != ((UpdateSIUpdateNodes)e).Count)
                        {
                            progressBar3.Maximum = ((UpdateSIUpdateNodes)e).Count;
                        }

                        progressBar3.Value = Math.Min(progressBar3.Maximum, ((UpdateSIUpdateNodes)e).Pos);

                        plabel3.Text = progressBar3.Value + "/" + progressBar3.Maximum;

                        progressBar3.Refresh();
                        plabel2.Refresh();
                    }
                }
            }
        }

        private void FormRebuildSpatialIndexProgess_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_finished)
            {
                e.Cancel = true;
            }
        }
    }
}
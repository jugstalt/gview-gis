using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using System.Threading;
using gView.DataSources.Fdb.MSAccess;

namespace gView.DataSources.Fdb.UI.MSSql
{
    public partial class FormRepairSpatialIndexProgress : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        private bool _finished = true;

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
            if (_fdb != null && _fc != null)
            {
                _finished = false;
                Thread thread = new Thread(new ThreadStart(Process));
                thread.Start();
            }
            else
                this.Close();
        }

        private void FormRepairSpatialIndexProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_finished)
                e.Cancel = true;
        }

        private void Process()
        {
            if (!_fdb.RepairSpatialIndex(_fc.Name, new EventHandler(ProcessEventHandler)))
            {
                MessageBox.Show(_fdb.lastErrorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                }
                else if (e is RepairSICheckNodes)
                {
                    if (progressBar1.Maximum != ((RepairSICheckNodes)e).Count)
                        progressBar1.Maximum = ((RepairSICheckNodes)e).Count;
                    progressBar1.Value = Math.Min(progressBar1.Maximum, ((RepairSICheckNodes)e).Pos);

                    plabel1.Text = progressBar1.Value + "/" + progressBar1.Maximum;
                    if (((RepairSICheckNodes)e).WrongNIDs != progressBar2.Maximum)
                    {
                        progressBar2.Maximum = ((RepairSICheckNodes)e).WrongNIDs;
                        plabel2.Text = "0/" + progressBar2.Maximum;
                    }
                }
                else if (e is RepairSIUpdateNodes)
                {
                    if (progressBar2.Maximum != ((RepairSIUpdateNodes)e).Count)
                        progressBar2.Maximum = ((RepairSIUpdateNodes)e).Count;
                    progressBar2.Value = Math.Min(progressBar2.Maximum, ((RepairSIUpdateNodes)e).Pos);

                    plabel2.Text = progressBar2.Value + "/" + progressBar2.Maximum;
                }
            }
        }
    }
}
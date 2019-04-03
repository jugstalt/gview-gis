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
    public partial class FormRebuildSpatialIndexProgess : Form
    {
        private gView.DataSources.Fdb.MSAccess.AccessFDB _fdb;
        private IFeatureClass _fc;
        private BinaryTreeDef _def;
        private bool _finished = true;

        public FormRebuildSpatialIndexProgess(gView.DataSources.Fdb.MSAccess.AccessFDB fdb,IFeatureClass fc,BinaryTreeDef def)
        {
            InitializeComponent();

            _fdb = fdb;
            _fc = fc;
            _def = def;
            
        }

        private void FormRebuildSpatialIndexProgess_Load(object sender, EventArgs e)
        {
            if (_fdb != null && _fc != null && _def != null)
            {
                _finished = false;
                Thread thread = new Thread(new ThreadStart(Process));
                thread.Start();
            }
            else 
                this.Close();
        }

        private void Process()
        {
            if (!_fdb.RebuildSpatialIndexDef(_fc.Name, _def, new EventHandler(ProcessEventHandler)))
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
                    this.Close();
                }
                else if (e is UpdateSIDefEventArgs && ((UpdateSIDefEventArgs)e).Finished)
                {
                    progressBar1.Value = progressBar1.Maximum;
                }
                else if (e is UpdateSICalculateNodes)
                {
                    if (progressBar2.Maximum != ((UpdateSICalculateNodes)e).Count)
                        progressBar2.Maximum = ((UpdateSICalculateNodes)e).Count;
                    progressBar2.Value = Math.Min(progressBar2.Maximum, ((UpdateSICalculateNodes)e).Pos);

                    plabel2.Text = progressBar2.Value + "/" + progressBar2.Maximum;
                }
                else if (e is UpdateSIUpdateNodes)
                {
                    if (progressBar3.Maximum != ((UpdateSIUpdateNodes)e).Count)
                        progressBar3.Maximum = ((UpdateSIUpdateNodes)e).Count;
                    progressBar3.Value = Math.Min(progressBar3.Maximum, ((UpdateSIUpdateNodes)e).Pos);

                    plabel3.Text = progressBar3.Value + "/" + progressBar3.Maximum;
                }
            }
        }

        private void FormRebuildSpatialIndexProgess_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_finished)
                e.Cancel = true;
        }
    }
}
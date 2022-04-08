using gView.Framework.system;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public enum ProgressMode { ProgressBar, ProgressDisk }

    /// <summary>
    /// Zusammenfassung für FormTaskProgress.
    /// </summary>
    public class FormTaskProgress : System.Windows.Forms.Form, IProgressTaskDialog
    {
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer timer1;
        private System.ComponentModel.IContainer components;
        private Task _task = null;
        private gView.Framework.UI.Controls.ProgressDisk progressDisk1;
        private ICancelTracker _cancelTracker = null;
        private Panel panelTime;
        private Label lblTime;
        private Label label1;
        private ProgressMode _mode = ProgressMode.ProgressBar;
        private DateTime startTime;

        public FormTaskProgress()
        {
            InitializeComponent();
        }

        public FormTaskProgress(Task task)
        {
            InitializeComponent();

            btnCancel.Visible = false;
            _task = task;
        }
        public FormTaskProgress(IProgressReporter reporter, Task task)
            : this(task)
        {
            _cancelTracker = reporter.CancelTracker;
            btnCancel.Visible = (_cancelTracker != null);

            if (reporter != null)
            {
                reporter.ReportProgress += new ProgressReporterEvent(HandleProgressEvent);
            }
        }

        #region IProgressDialog Member


        public void ShowProgressDialog(IProgressReporter reporter, Task task)
        {
            _task = task;
            if (_task == null)
            {
                return;
            }

            if (reporter != null)
            {
                _cancelTracker = reporter.CancelTracker;
                btnCancel.Visible = (_cancelTracker != null);
                reporter.ReportProgress += new ProgressReporterEvent(HandleProgressEvent);
            }

            this.ShowDialog();
        }

        public bool UserInteractive
        {
            get { return SystemInformation.UserInteractive; }
        }

        #endregion

        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public ProgressMode Mode
        {
            get { return _mode; }
            set
            {
                switch (value)
                {
                    case ProgressMode.ProgressBar:
                        progressBar1.Visible = true;
                        progressDisk1.Visible = false;
                        progressDisk1.Stop();
                        break;
                    case ProgressMode.ProgressDisk:
                        progressBar1.Visible = false;
                        progressDisk1.Visible = true;
                        progressDisk1.Start(100);
                        break;
                }
                _mode = value;
            }
        }

        public void HandleProgressEvent(object report)
        {
            try
            {
                if (report is ProgressReport)
                {
                    labelMessage.Text = ((ProgressReport)report).Message;
                    if (((ProgressReport)report).featureMax != -1)
                    {
                        if (_mode == ProgressMode.ProgressBar)
                        {
                            if (progressBar1.Visible == false)
                            {
                                progressBar1.Visible = true;
                            }

                            labelProgress.Text = ((ProgressReport)report).featurePos + "/" + ((ProgressReport)report).featureMax;
                            if (((ProgressReport)report).featureMax > progressBar1.Value &&
                                progressBar1.Value != 0)
                            {
                                progressBar1.Value = 0;
                            }

                            if (((ProgressReport)report).featureMax != progressBar1.Maximum &&
                                progressBar1.Maximum != ((ProgressReport)report).featureMax)
                            {
                                progressBar1.Maximum = ((ProgressReport)report).featureMax;
                            }

                            if (progressBar1.Value != ((ProgressReport)report).featurePos)
                            {
                                progressBar1.Value = ((ProgressReport)report).featurePos;
                            }

                            if (((ProgressReport)report).featureMax > 0)
                            {
                                double percent = ((ProgressReport)report).featurePos / (double)((ProgressReport)report).featureMax;
                                this.Text = ((int)(percent * 100.0)).ToString() + " %";

                                if (percent > 0.0)
                                {
                                    if (panelTime.Visible == false)
                                    {
                                        panelTime.Visible = true;
                                    }

                                    TimeSpan ts = DateTime.Now - startTime;
                                    double minutes = ts.TotalMinutes / percent - ts.TotalMinutes;

                                    if (minutes > 60)
                                    {
                                        int h = (int)minutes / 60;
                                        lblTime.Text = h.ToString() + "h " + ((int)minutes - h * 60).ToString() + "min";
                                    }
                                    else
                                    {
                                        lblTime.Text = ((int)minutes).ToString() + "min";
                                    }
                                }
                            }
                        }
                        else
                        {
                            labelProgress.Text = ((ProgressReport)report).featurePos.ToString();
                        }
                    }
                    else
                    {
                        progressBar1.Visible = false;
                        labelProgress.Text = ((ProgressReport)report).featurePos.ToString();
                    }
                }
                this.Refresh();
            }
            catch
            {
            }
        }
        #region Vom Windows Form-Designer generierter Code
        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTaskProgress));
            this.labelMessage = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.progressDisk1 = new gView.Framework.UI.Controls.ProgressDisk();
            this.panelTime = new System.Windows.Forms.Panel();
            this.lblTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelTime.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            resources.ApplyResources(this.labelMessage, "labelMessage");
            this.labelMessage.Name = "labelMessage";
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Step = 1;
            // 
            // labelProgress
            // 
            resources.ApplyResources(this.labelProgress, "labelProgress");
            this.labelProgress.Name = "labelProgress";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressDisk1
            // 
            resources.ApplyResources(this.progressDisk1, "progressDisk1");
            this.progressDisk1.ActiveForeColor1 = System.Drawing.Color.Red;
            this.progressDisk1.ActiveForeColor2 = System.Drawing.Color.Coral;
            this.progressDisk1.BlockSize = gView.Framework.UI.Controls.BlockSize.Large;
            this.progressDisk1.Name = "progressDisk1";
            this.progressDisk1.SquareSize = 170;
            // 
            // panelTime
            // 
            resources.ApplyResources(this.panelTime, "panelTime");
            this.panelTime.Controls.Add(this.lblTime);
            this.panelTime.Controls.Add(this.label1);
            this.panelTime.Name = "panelTime";
            // 
            // lblTime
            // 
            resources.ApplyResources(this.lblTime, "lblTime");
            this.lblTime.Name = "lblTime";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // FormTaskProgress
            // 
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.panelTime);
            this.Controls.Add(this.progressDisk1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormTaskProgress";
            this.Shown += new System.EventHandler(this.FormTaskProgress_Shown);
            this.panelTime.ResumeLayout(false);
            this.panelTime.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (_task == null || _task.IsCompleted)
            {
                this.Close();
            }
        }

        private void FormTaskProgress_Shown(object sender, EventArgs e)
        {
            startTime = DateTime.Now;

            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;

            if (_cancelTracker == null)
            {
                return;
            }

            _cancelTracker.Cancel();
        }
    }
}

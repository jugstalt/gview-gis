using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.UI.Dialogs;
using gView.Framework.system;
using gView.DataSources.Fdb.MSAccess;

namespace gView.DataSources.Fdb.UI
{
    internal class SpatialIndexShrinker : IProgressReporter
    {
        private CancelTracker _cancelTracker;
        private ProgressReport _report = new ProgressReport();

        public SpatialIndexShrinker()
        {
            _cancelTracker = new CancelTracker();
        }

        public bool RebuildIndices(List<IClass> classes)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(RebuildIndecesAsync));

            FormProgress progress = new FormProgress(this, thread, classes);
            progress.Text = "Shrink Spatial Indices...";
            progress.Show();

            return true;
        }

        private void RebuildIndecesAsync(object argument)
        {
            if (!(argument is List<IClass>)) return;
            List<IClass> classes = argument as List<IClass>;

            if (ReportProgress != null)
            {
                _report.featureMax = classes.Count;
                _report.featurePos = 0;
                ReportProgress(_report);
            }
            foreach (IClass cl in classes)
            {
                if (!(cl is IFeatureClass) || cl.Dataset == null || !(cl.Dataset.Database is AccessFDB)) continue;

                if (ReportProgress != null)
                {
                    _report.featurePos = classes.IndexOf(cl);
                    _report.Message = "Featureclass " + cl.Name;
                    ReportProgress(_report);
                }
                
                AccessFDB fdb = cl.Dataset.Database as AccessFDB;
                if (!fdb.ShrinkSpatialIndex(cl.Name))
                {
                    MessageBox.Show("Error rebuilding " + cl.Name + " index:\n" + fdb.lastErrorMsg);
                    return;
                }
                if (!_cancelTracker.Continue) return;
            }
        }

        #region IProgressReporter Member

        public event ProgressReporterEvent ReportProgress;

        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.system;

namespace gView.Plugins.Network
{
    class NetworkProgressReporter : IProgressReporter
    {
        private CancelTracker _cancelTracker = new CancelTracker();
        private IMapDocument _doc;

        public NetworkProgressReporter(IMapDocument doc)
        {
            _doc = doc;
        }

        public IMapDocument MapDocument
        {
            get { return _doc; }
        }

        #region IProgressReporter Member

        public event ProgressReporterEvent ReportProgress = null;

        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }

        #endregion

        public void FireProgressReporter(ProgressReport progressEventReport)
        {
            if (ReportProgress != null)
                ReportProgress(progressEventReport);
        }
    }
}

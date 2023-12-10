using gView.Framework.Core.Common;

namespace gView.Framework.Core.UI
{
    public interface IProgressReporter : IProgressReporterEvent
    {
        ICancelTracker CancelTracker { get; }
    }


}
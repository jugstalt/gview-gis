using gView.Framework.Core.system;

namespace gView.Framework.Core.UI
{
    public interface IProgressReporter : IProgressReporterEvent
    {
        ICancelTracker CancelTracker { get; }
    }


}
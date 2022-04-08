using gView.Framework.system;

namespace gView.Framework.UI
{
    public interface IProgressReporter : IProgressReporterEvent
    {
        ICancelTracker CancelTracker { get; }
    }

    
}
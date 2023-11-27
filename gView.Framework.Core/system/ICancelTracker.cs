/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.system
{
    public interface ICancelTracker
    {
        void Cancel();
        void Pause();
        bool Continue { get; }
        bool Paused { get; }
    }
}

using System.Threading.Tasks;

namespace gView.Framework.Core.UI
{
    public interface IProgressTaskDialog
    {
        string Text { get; set; }
        void ShowProgressDialog(IProgressReporter reporter, Task task);
        bool UserInteractive { get; }
    }


}
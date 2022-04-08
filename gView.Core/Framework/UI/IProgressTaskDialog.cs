using System.Threading.Tasks;

namespace gView.Framework.UI
{
    public interface IProgressTaskDialog
    {
        string Text { get; set; }
        void ShowProgressDialog(IProgressReporter reporter, Task task);
        bool UserInteractive { get; }
    }

    
}